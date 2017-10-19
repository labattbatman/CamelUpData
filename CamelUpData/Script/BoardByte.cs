using System;
using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class BoardByte : IBoard
	{
		public byte[] BoardState { get; private set; }

		private string m_BoardStateString;

		public string BoardStateString
		{
			get
			{
				if (string.IsNullOrEmpty(m_BoardStateString))
					m_BoardStateString = GameRules.ByteToString(BoardState);
				return m_BoardStateString;
			}
		}

		public int[] CasesLandedOn { get; private set; }

		public string HighestCaseLandedOn
		{
			get
			{
				int highestCase = 0;

				for (int i = 1; i < CasesLandedOn.Length; i++)
					if (CasesLandedOn[i] > CasesLandedOn[highestCase])
						highestCase = i;

				return string.Format("Case: {0}. Time: {1}", highestCase, CasesLandedOn[highestCase]);
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

		private Dictionary<byte, int> m_Position = new Dictionary<byte, int>();
		private Dictionary<byte, byte[]> m_Neighbouring = new Dictionary<byte, byte[]>();

		public List<IBoard> m_SubBoard { get; set; }

		private byte[] m_Rank;
		public int NbRound { get; private set; }

		public int Weight { get; private set; }

		public bool IsCamelReachEnd { get { return FirstCamelPos >= GameRules.CASE_NUMBER; } }

		public bool IsAllCamelRolled
		{
			get
			{
				byte[] camels = GetRank();
				foreach (byte camel in camels)
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
				byte[] rank = GetRank();
				return GetCamelPos(rank[rank.Length - 1]);
			}
		}

		public BoardByte(string aBoardId)
		{
			m_SubBoard = new List<IBoard>();
			BoardState = GameRules.StringToByte(aBoardId);
			CasesLandedOn = new int[GameRules.CASE_NUMBER];
			Weight = 1;

			PopulateNeighbouring();

			if (!GameRules.USE_DICE_NB_IN_DICE_HSITORY)
				PopulateSubBoard();
		}

		protected BoardByte(BoardByte aInitialBoard, string aPattern, byte aRolledCamel)
		{
			m_SubBoard = new List<IBoard>();
			byte[] pattern = GameRules.StringToByte(aPattern);
			byte[] camels = aInitialBoard.GetCamelsNeighbouring(aRolledCamel);
			
			for (int i = 0; i < pattern.Length; i++)
			{
				if (GameRules.IsBytePatternCamel(pattern[i]))
				{
					byte camel = camels[GameRules.PATTER_NAME_NUMBER_BYTE(pattern[i])];
					pattern[i] = camel == aRolledCamel ? GameRules.ByteUnrollToRoll(camel) : camel;
				}
			}

			int startingPos = GetBytePositionInArray(aInitialBoard.BoardState, camels[0]) - 1;
			byte[] newBoardState = (byte[])aInitialBoard.BoardState.Clone();

			newBoardState = RemoveByteArray(newBoardState, startingPos, Math.Min(pattern.Length, newBoardState.Length - startingPos));
			newBoardState = InsertBytesIntoBytes(newBoardState, startingPos, pattern);

			BoardState = newBoardState;

			NbRound = aInitialBoard.NbRound;
			Weight = aInitialBoard.Weight;

			if (GetUnrolledCamelByRank().Length == 0)
				NbRound++;

			CasesLandedOn = (int[])aInitialBoard.CasesLandedOn.Clone();

			int caseLanded = GetCamelPos(aRolledCamel);
			if (caseLanded < CasesLandedOn.Length)
				CasesLandedOn[caseLanded]++;

			PopulateNeighbouring();
		}

		private int GetNbCamelInPattern(string aPattern)
		{
			int nbOfCamelInPattern = 0;
			foreach (char pattern in aPattern)
				if (GameRules.IsCharPatternCamel(pattern))
					nbOfCamelInPattern++;
			return nbOfCamelInPattern;
		}

		private int GetCamelPos(byte aRolledCamel)
		{
			if (!m_Position.ContainsKey(aRolledCamel))
			{
				int retval = 0;
				foreach (byte token in BoardState)
				{
					if (GameRules.IsByteIdentityCamel(token) && GameRules.ByteRollToUnroll(token) == GameRules.ByteRollToUnroll(aRolledCamel))
					{
						m_Position.Add(aRolledCamel, retval);
					}
					else if (token == GameRules.CASE_SEPARATOR_BYTE)
					{
						retval++;
					}
				}
			}
			return m_Position[aRolledCamel];
		}

		private void SetAllCamelUnroll()
		{
			byte[] tempBoard = new byte[BoardState.Length];
			for (int i = 0; i < tempBoard.Length; i++)
			{
				if (GameRules.IsByteIdentityCamel(BoardState[i]))
					tempBoard[i] = GameRules.ByteRollToUnroll(BoardState[i]);
				else tempBoard[i] = BoardState[i];
			}

			BoardState = tempBoard;
		}

		public void PopulateSubBoard()
		{
			if (IsCamelReachEnd)
				return;

			byte[] unRollCamel = GetUnrolledCamelByRank();

			if (unRollCamel.Length == 0)
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
					byte unrollCamel = unRollCamel[j];
					if (!pattern.CamelsIdentity.ToUpper().Contains(GameRules.ByteToString(unrollCamel).ToString().ToUpper()))
					{
						//GameRules.Log("TEST" + unrollCamel + "\n");
						continue;
					}

					List<string> results = pattern.GetResultsForDice(GameRules.ByteToString(unrollCamel));
					unrolledCamels += GameRules.ByteToString(unRollCamel[j]);
					for (int k = 0; k < results.Count; k++)
					{
						CreateSubboard(results[k], unrollCamel, k + 1);
					}
				}

				foreach (char unrollCamel in unrolledCamels)
					unRollCamel = RemoveByteFromBytesArray(unRollCamel, GameRules.StringToByte(unrollCamel));

				unrolledCamels = string.Empty;
			}
		}

		protected virtual void CreateSubboard(string aResult, byte aUnrollCamel, int aDiceNb)
		{
			BoardByte subBoard = new BoardByte(this, aResult, aUnrollCamel);
			m_SubBoard.Add(subBoard);
		}

		private List<Pattern> ToPattern()
		{
			string board = string.Empty;
			int camelIndex = 0;
			foreach (byte token in BoardState)
			{
				if (GameRules.IsByteIdentityCamel(token))
					board += GameRules.PATTERN_CAMEL_NAME[camelIndex++];
				else
					board += GameRules.ByteToString(token);
			}

			List<string> patterns = GameRules.PatternResultToPattern(board);
			List<Pattern> retval = new List<Pattern>();
			byte[] camels = GetRank();
			int camelsIndex = 0;

			foreach (string pattern in patterns)
			{
				if (!PatternGenerator.Instance.Patterns.ContainsKey(pattern))
				{
					PatternGenerator.Instance.StartGeneratePattern(pattern);
				}

				byte[] camelIdentity = SubByteArray(camels, camelsIndex, PatternGenerator.Instance.Patterns[pattern].NbCamel);
				Pattern newPattern = new Pattern(PatternGenerator.Instance.Patterns[pattern], camelIdentity);
				camelsIndex += newPattern.NbCamel;
				retval.Add(newPattern);
			}

			return retval;
		}

		public byte[] GetUnrolledCamelByRank()
		{
			List<byte> retvalList = new List<byte>();
			foreach (byte token in BoardState)
			{
				if (GameRules.IsByteIdentityCamelUnrolled(token))
					retvalList.Add(token);
			}

			return retvalList.ToArray();
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

		public byte[] GetRank()
		{
			if (m_Rank == null)
			{
				List<byte> tempByte = new List<byte>();
				foreach (byte token in BoardState)
				{
					if (GameRules.IsByteIdentityCamel(token))
						tempByte.Add(token);
				}

				m_Rank = new byte[tempByte.Count];

				for (int i = 0; i < m_Rank.Length; i++)
					m_Rank[i] = tempByte[i];

			}
			return m_Rank;
		}

		public string GetRankString()
		{
			byte[] rankByte = GetRank();
			string retval = string.Empty;

			foreach (var rank in rankByte)
				retval += GameRules.ByteToString(rank);

			return retval;
		}

		private bool IsCamelRolled(byte aCamel)
		{
			byte UnrollCamel = GameRules.ByteUnrollToRoll(aCamel);
			foreach (var aByte in BoardState)
			{
				if (aByte == UnrollCamel)
					return true;
			}
			return false;
		}

		private void PopulateNeighbouring()
		{
			byte[] camels = GetRank();
			foreach (byte camel in camels)
				GetCamelsNeighbouring(camel);
		}

		private byte[] GetCamelsNeighbouring(byte aCamel)
		{
			if (!m_Neighbouring.ContainsKey(aCamel))
			{
				List<byte> camels = new List<byte>();
				List<byte[]> splittedBoard = SplitBoardByCase();

				/*foreach (Byte token in tempSplittedBoard)
					if (!string.IsNullOrEmpty(token)) TODO DELETE
						splittedBoard.Add(token);*/

				for (int i = 0; i < splittedBoard.Count; i++)
				{
					if (!GameRules.IsByteIdentityCamel(splittedBoard[i][0]))
						continue;

					if (IsByteArrayContainByte(splittedBoard[i], aCamel))
					{
						
						camels.AddRange(splittedBoard[i]);
						continue;
					}
					int rolledCamelPos = GetCamelPos(aCamel);
					int nbCaseBetweenSplittedAndRolledCamel = rolledCamelPos - GetCamelPos(splittedBoard[i][0]);

					if (nbCaseBetweenSplittedAndRolledCamel > 0)
					{
						if (!GameRules.IsCamelsAreTooFar(splittedBoard[i].Length, nbCaseBetweenSplittedAndRolledCamel))
						{
							camels.AddRange(splittedBoard[i]);
						}
						else
						{
							camels = new List<byte>();
						}
					}
					else
					{
						int lastSplitPos = Math.Abs(GetCamelPos(splittedBoard[i - 1][0]));
						int currentSplitPos = Math.Abs(GetCamelPos(splittedBoard[i][0]));

						if (!GameRules.IsCamelsAreTooFar(splittedBoard[i - 1].Length, currentSplitPos - lastSplitPos))
						{
							camels.AddRange(splittedBoard[i]);
						}
						else
						{
							break;
						}
					}
				}

				foreach (byte camel in camels)
						m_Neighbouring.Add(camel, camels.ToArray());

				//GameRules.Log(camels + "\n");
			}

			return m_Neighbouring[aCamel];
		}

		public void AddWeight(IBoard aBoard)
		{
			Weight += aBoard.Weight;
		}

		#region Byte Logic

		private List<byte[]> SplitBoardByCase()
		{
			List<byte[]> retval = new List<byte[]>();
			int lastCasePos = 0;

			for (int i = 0; i < BoardState.Length; i++)
			{
				if (BoardState[i] == GameRules.CASE_SEPARATOR_BYTE)
				{
					if (lastCasePos != i)
					{
						byte[] splitted = new byte[i - lastCasePos];
						for (int j = lastCasePos; j < i; j++)
							splitted[j - lastCasePos] = BoardState[j];

						retval.Add(splitted);

						lastCasePos = i;
					}

					lastCasePos++;
				}
				else if (i == BoardState.Length - 1)
				{
					int index = i + 1;
					byte[] splitted = new byte[index - lastCasePos];
					for (int j = lastCasePos; j < index; j++)
						splitted[j - lastCasePos] = BoardState[j];

					retval.Add(splitted);
				}
			}

			return retval;
		}

		private bool IsByteArrayContainByte(byte[] aByteArray, byte aByte)
		{
			foreach (var byteArray in aByteArray)
			{
				if (byteArray == aByte)
					return true;
			}

			return false;
		}

		private byte[] SubByteArray(byte[] aBytes, int aStart, int aLength)
		{
			byte[] retval = new byte[aLength];

			for (int i = aStart; i < aStart + aLength; i++)
				retval[i - aStart] = aBytes[i];

			return retval;
		}

		private int GetBytePositionInArray(byte[] aBytesArray, byte aByte)
		{
			for(int i = 0; i < aBytesArray.Length; i++)
				if (aBytesArray[i] == aByte)
					return i;

			return -1;
		}

		private byte[] RemoveByteArray(byte[] aBytes, int aStartingPos, int aCount)
		{
			byte[] retval = new byte[aBytes.Length - aCount];

			for (int i = 0; i < aBytes.Length; i++)
			{
				if (i < aStartingPos)
					retval[i] = aBytes[i];
				else if(i >= aStartingPos + aCount)
					retval[i - aCount] = aBytes[i];
			}

			return retval;
		}

		private byte[] RemoveByteFromBytesArray(byte[] aBytes, byte aBytesToRemove)
		{
			return RemoveByteArray(aBytes, GetBytePositionInArray(aBytes, aBytesToRemove), 1);
		}

		private byte[] InsertBytesIntoBytes(byte[] aInitialBytes, int aStartingPos, byte[] aInsertBytes)
		{
			byte[] retval = new byte[aInitialBytes.Length + aInsertBytes.Length];

			for (int i = 0; i < retval.Length; i++)
			{
				if (i < aStartingPos)
					retval[i] = aInitialBytes[i];
				else if (i >= aStartingPos && i < aStartingPos + aInsertBytes.Length)
					retval[i] = aInsertBytes[i - aStartingPos];
				else
					retval[i] = aInitialBytes[i - aInsertBytes.Length];
			}

			return retval;
		}

		#endregion
	}
}
