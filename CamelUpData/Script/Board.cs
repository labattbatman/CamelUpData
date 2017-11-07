using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CamelUpData.Script
{
	public class Board : IBoard
	{
		public string BoardState { get; private set; }

		public string BoardStateString { get { return BoardState; } }

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

		private Dictionary<char, int> m_Position = new Dictionary<char, int>();
		private Dictionary<char, string> m_Neighbouring = new Dictionary<char, string>();
		public List<IBoard> m_SubBoard { get; set; }
		
		private string m_Rank = string.Empty;
		public int NbRound{ get; private set; }

		public int Weight { get; private set; }

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
			m_SubBoard = new List<IBoard>();
			BoardState = aBoardId;
			CasesLandedOn = new int[GameRules.CASE_NUMBER];
			Weight = 1;

			PopulateNeighbouring();

			if(!GameRules.USE_DICE_NB_IN_DICE_HSITORY)
				PopulateSubBoard();
		} 
    
		protected Board(Board aInitialBoard, string aPattern, char aRolledCamel)
		{
			m_SubBoard = new List<IBoard>();
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

			NbRound = aInitialBoard.NbRound;
			Weight = aInitialBoard.Weight;

			if (String.IsNullOrEmpty(GetUnrolledCamelByRank()))
				NbRound++;

			CasesLandedOn = (int[])aInitialBoard.CasesLandedOn.Clone();

			int caseLanded = GetCamelPos(aRolledCamel);
			if(caseLanded < CasesLandedOn.Length)
				CasesLandedOn[caseLanded]++;

			PopulateNeighbouring();
		}

		/*private bool IsPosHasTwoOrMoreCamel(int aPos)
		{
			bool isCamelOnPos = false;
			string camels = GetRank();

			foreach (var camel in camels)
			{
				if (GetCamelPos(camel) == aPos)
				{
					if (isCamelOnPos)
						return true;
					isCamelOnPos = true;
				}
			}

			return false;
		}*/

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

		public void PopulateSubBoard()
		{       
			if (IsCamelReachEnd)
				return;

			string unRollCamel = GetUnrolledCamelByRank();

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
						CreateSubboard(results[k], unrollCamel, k + 1);
					}
				}

				foreach (char unrollCamel in unrolledCamels)
					unRollCamel = Regex.Replace(unRollCamel, unrollCamel.ToString(), string.Empty);

				unrolledCamels = string.Empty;
			}
		}

		protected virtual void CreateSubboard(string aResult, char aUnrollCamel, int aDiceNb)
		{
			Board subBoard = new Board(this, aResult, aUnrollCamel);
			m_SubBoard.Add(subBoard);
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

		public string GetRankString()
		{
			return GetRank();
		}

		private bool IsCamelRolled(char aCamel)
		{
			return BoardState.Contains((char.ToLower(aCamel).ToString()));
		}

		private void PopulateNeighbouring()
		{
			string camels = GetRank();
			foreach (char camel in camels)
				GetCamelsNeighbouring(camel);
		}

		private string GetCamelsNeighbouring(char aCamel)
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

		public void AddWeight(IBoard aBoard)
		{
			Weight += aBoard.Weight;

			//for (int i = 0; i < CasesLandedOn.Length; i++)
			//	CasesLandedOn[i] += aBoard.CasesLandedOn[i];
		}
	}
}
