using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CamelUpData.Script
{
	public class Board
	{
		public string BoardState { get; private set; }
		public int[] CasesLandedOn { get; private set; }

		public string HighestCaseLandedOn
		{
			get
			{
				int highestCase = 0;

				for(int i = 1; i < CasesLandedOn.Length; i++)
					if (CasesLandedOn[i] > CasesLandedOn[highestCase])
						highestCase = i;

				return string.Format("Case: {0}. Time: {1}" ,highestCase, CasesLandedOn[highestCase]);
			}
		}

		public int HighestTimeLandedOnSameCase
		{
			get
			{
				int highest = 0;

				for (int i = 1; i < CasesLandedOn.Length; i++)
					if (CasesLandedOn[i] > highest)
						highest = CasesLandedOn[i];

				return highest;
			}
		}

		public string DicesHistory { get; private set; } //Start from initial board. Used for debugging

		private Dictionary<char, int> m_Position = new Dictionary<char, int>();
		private Dictionary<char, string> m_Neighbouring = new Dictionary<char, string>();
		public List<Board> m_SubBoard = new List<Board>();
		private string m_Rank = string.Empty;
		private int m_NbRound;

		private Board m_ParentBoard { get; set; } //Used for debugging
    
		public bool IsCamelReachEnd { get { return FirstCamelPos >= GameRules.CASE_NUMBER; } }  

		public bool IsAllCamelRolled
		{
			get
			{
				string camels = GetRank();
				foreach (char camel in camels)
				{
					if (!IsCamelRolled(camel))
						return false;
				}

				return true;
			}
		}

		public int FirstCamelPos
		{
			get
			{
				string rank = GetRank();
				return GetCamelPos(rank[rank.Length - 1]);
			}
		}

		public Board(string aBoardId)
		{
			BoardState = aBoardId;
			CasesLandedOn = new int[GameRules.CASE_NUMBER];

			PopulateNeighbouring();
			PopulateSubBoard();
		} 
    
		private Board(Board aInitialBoard, string aPattern, char aRolledCamel, string aDicesHistory, int aRoundNb)
		{
			m_ParentBoard = aInitialBoard;

			StringBuilder pattern = new StringBuilder(aPattern);     
			string camels = aInitialBoard.GetCamelsNeighbouring(aRolledCamel);

			for (int i = 0; i < pattern.Length; i++)
			{
				if (GameRules.IsCharPatternCamel(pattern[i]))
				{
					char camel = camels[GameRules.PATTER_NAME_NUMBER(pattern[i])];
					pattern[i] = camel == aRolledCamel ? Char.ToLower(camel) : camel;
				}
			}

			int startingPos = aInitialBoard.BoardState.IndexOf(camels[0]) - 1;
			StringBuilder newBoardState = new StringBuilder(aInitialBoard.BoardState);

			newBoardState.Remove(startingPos, Math.Min(pattern.Length, newBoardState.Length - startingPos));
			newBoardState.Insert(startingPos, pattern);

			BoardState = newBoardState.ToString();
			DicesHistory = aDicesHistory;

			m_NbRound = aRoundNb;
			if (String.IsNullOrEmpty(GetUnrolledCamelByRank()))
				m_NbRound++;

			CasesLandedOn = (int[])aInitialBoard.CasesLandedOn.Clone();

			int caseLanded = GetCamelPos(aRolledCamel);
			if(caseLanded < CasesLandedOn.Length)
				CasesLandedOn[caseLanded]++;

			PopulateNeighbouring(); 

			if (IsCamelReachEnd)
				Program.PopulateFinishBoard(this);
			else if (m_NbRound < GameRules.MAX_ROUND_ANALYSE)
				PopulateSubBoard();
			else Program.PopulateUnfinishBoardbyMaxRound(this);
		}

		private int GetNbCamelInPattern(string aPattern)
		{
			int nbOfCamelInPattern = 0;
			foreach (char pattern in aPattern)
				if (GameRules.IsCharPatternCamel(pattern))
					nbOfCamelInPattern++;
			return nbOfCamelInPattern;
		}

		private int GetCamelPos(char aRolledCamel)
		{
			if (!m_Position.ContainsKey(aRolledCamel))
			{
				int retval = 0;
				foreach (char token in BoardState)
				{
					if (Char.ToLower(token) == Char.ToLower(aRolledCamel))
					{
						m_Position.Add(aRolledCamel, retval);
					}
					else if (token == GameRules.CASE_SEPARATOR)
					{
						retval++;
					}
				}
			}
			return m_Position[aRolledCamel];
		}

		private void SetAllCamelUnroll()
		{
			StringBuilder tempBoard = new StringBuilder(BoardState);
			for(int i = 0; i < tempBoard.Length; i++)
			{
				if (GameRules.IsCharIdentityCamel(tempBoard[i]))
					tempBoard[i] = Char.ToUpper(tempBoard[i]);
			}

			BoardState = tempBoard.ToString();
		}

		private void PopulateSubBoard()
		{       
			if (IsCamelReachEnd)
				return;

			string unRollCamel = GetUnrolledCamelByRank();
			//TODO hardcoder pour short term seulement soit quand tous les dés sont lancées. (Pas de reroll)

			if (String.IsNullOrEmpty(unRollCamel))
			{        
				SetAllCamelUnroll();
				unRollCamel = GetUnrolledCamelByRank();
			}

			List<Pattern> patterns = ToPattern();

			// i = pattern
			// j = movingCamel in pattern
			// k = result
			foreach (Pattern pattern in patterns)
			{
				string unrolledCamels = string.Empty;
				for (int j = 0; j < pattern.NbCamel && j < unRollCamel.Length; j++)
				{
					char unrollCamel = unRollCamel[j];
					if (!pattern.CamelsIdentity.ToUpper().Contains(unrollCamel.ToString().ToUpper()))
					{
						//GameRules.Log("TEST" + unrollCamel + "\n");
						continue;
					}

					List<string> results = pattern.GetResultsForDice(unrollCamel);
					unrolledCamels += unRollCamel[j];
					for (int k = 0; k < results.Count; k++)
					{
						//string dicesHistory = DicesHistory + unrollCamel + (k + 1); // Ca faut bugger TestMethod
						string dicesHistory = DicesHistory + unrollCamel;
						Board subBoard = new Board(this, results[k], unrollCamel, dicesHistory, m_NbRound);
						m_SubBoard.Add(subBoard);
					}
				}

				foreach (char unrollCamel in unrolledCamels)
					unRollCamel = Regex.Replace(unRollCamel, unrollCamel.ToString(), string.Empty);

				unrolledCamels = string.Empty;
			}
		}

		private List<Pattern> ToPattern()
		{
			string board = string.Empty;
			int camelIndex = 0;
			foreach (char token in BoardState)
			{
				if (GameRules.IsCharIdentityCamel(token))
					board += GameRules.PATTERN_CAMEL_NAME[camelIndex++];
				else
					board += token;
			}

			List<string> patterns = GameRules.PatternResultToPattern(board);
			List<Pattern> retval = new List<Pattern>();
			string camels = GetRank();
			int camelsIndex = 0;

			foreach (string pattern in patterns)
			{
				if (!PatternGenerator.Instance.Patterns.ContainsKey(pattern))
				{
					PatternGenerator.Instance.StartGeneratePattern(pattern);
				}

				string camelIdentity = camels.Substring(camelsIndex, PatternGenerator.Instance.Patterns[pattern].NbCamel);
				Pattern newPattern = new Pattern(PatternGenerator.Instance.Patterns[pattern], camelIdentity);
				camelsIndex += newPattern.NbCamel;
				retval.Add(newPattern);
			}

			return retval;
		}

		public string GetUnrolledCamelByRank()
		{
			string retval = string.Empty;
			foreach (char token in BoardState)
			{
				if (GameRules.IsCharIdentityCamel(token) 
				    && Char.IsUpper(token))
					retval += token;
			}

			return retval;
		}

		private char CamelToPattern(char aCamel)
		{
			int camelIndex = 0;
			foreach (char token in BoardState)
			{
				char upperChar = char.ToUpper(token);

				if (upperChar == char.ToUpper(aCamel))
					break;
				else if (GameRules.IsCharIdentityCamel(upperChar))
					camelIndex++;
			}

			return GameRules.PATTERN_CAMEL_NAME[camelIndex];
		}

		public string GetRank()
		{
			if (string.IsNullOrWhiteSpace(m_Rank))
			{
				foreach (char token in BoardState)
				{
					if (GameRules.IsCharIdentityCamel(token))
						m_Rank += token;
				}
			}
			return m_Rank;
		}

		public bool IsCamelRolled(char aCamel)
		{
			return BoardState.Contains((char.ToLower(aCamel).ToString()));
		}

		public override string ToString()
		{
			string retval = string.Empty;

			if(GetUnrolledCamelByRank().Length == 0)
				retval += BoardState + "->" + DicesHistory + " " + HighestCaseLandedOn + "\n";

			foreach (Board subBoard in m_SubBoard)
			{
				retval += subBoard.ToString() ;
			}		

			return retval;
		}

		public string ToStringOldCamelUpFormat()
		{
			//White2->Y  Yellow2 Blue3->O Orange3 Green4
			//;;wy;bo;g
			string retval = string.Empty;

			if (GetUnrolledCamelByRank().Length == 0)
			{
				int caseIndex = 0;
				for(int i = 0; i < BoardState.Length; i++)
				{
					if(BoardState[i] == GameRules.CASE_SEPARATOR)
						caseIndex++;
					else if (GameRules.IsCharIdentityCamel(BoardState[i]))
					{
						retval += GameRules.FullNameCamel(BoardState[i]) + caseIndex;

						if(BoardState.Length - 1 > i && GameRules.IsCharIdentityCamel(BoardState[i + 1]))
						{
							retval += "->" + Char.ToUpper(BoardState[i + 1]) + " ";
						}

						retval += " ";
					}
				}
			}

			foreach (Board suBoard in m_SubBoard)
			{
				retval += suBoard.ToStringOldCamelUpFormat();
			}
			return retval + "\t";
		}

		public void PopulateNeighbouring()
		{
			string camels = GetRank();
			foreach (char camel in camels)
				GetCamelsNeighbouring(camel);
		}

		public string GetCamelsNeighbouring(char aCamel)
		{
			if (!m_Neighbouring.ContainsKey(aCamel))
			{
				string camels = string.Empty;
				string[] tempSplittedBoard = BoardState.Split(GameRules.CASE_SEPARATOR);
				List<string> splittedBoard = new List<string>();

				foreach (string token in tempSplittedBoard)
					if (!string.IsNullOrEmpty(token))
						splittedBoard.Add(token);

				for (int i = 0; i < splittedBoard.Count; i++)
				{
					if (string.IsNullOrEmpty(splittedBoard[i]) || !GameRules.IsCharIdentityCamel(splittedBoard[i][0]))
						continue;

					if (splittedBoard[i].Contains(aCamel.ToString()))
					{
						camels += splittedBoard[i];
						continue;
					}
					int rolledCamelPos = GetCamelPos(aCamel);
					int nbCaseBetweenSplittedAndRolledCamel = rolledCamelPos - GetCamelPos(splittedBoard[i][0]);
                
					if (nbCaseBetweenSplittedAndRolledCamel > 0)
					{
						if (!GameRules.IsCamelsAreTooFar(splittedBoard[i].Length, nbCaseBetweenSplittedAndRolledCamel))
						{
							camels += splittedBoard[i];
						}
						else
						{
							camels = string.Empty;
						}
					}
					else
					{
						int lastSplitPos = Math.Abs(GetCamelPos(splittedBoard[i - 1][0]));
						int currentSplitPos = Math.Abs(GetCamelPos(splittedBoard[i][0]));

						if (!GameRules.IsCamelsAreTooFar(splittedBoard[i - 1].Length, currentSplitPos - lastSplitPos))
						{
							camels += splittedBoard[i];
						}
						else
						{
							break;
						}
					}
				}

				foreach (char camel in camels)
					m_Neighbouring.Add(camel, camels);

				//GameRules.Log(camels + "\n");
			}

			return m_Neighbouring[aCamel];
		}    
	}
}
