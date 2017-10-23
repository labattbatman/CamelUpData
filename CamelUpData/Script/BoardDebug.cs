using System;
using System.Collections.Generic;

namespace CamelUpData.Script
{
	public class BoardDebug : Board
	{
		private BoardDebug m_ParentBoard { get; set; }

		public string DicesHistory { get; private set; }
		public List<string> DicesHistories = new List<string>();

		private List<BoardDebug> m_SubBoard = new List<BoardDebug>();

		public BoardDebug(string aBoardId) : base (aBoardId)
		{
			
		}

		public BoardDebug(Board aInitialBoard, string aPattern, char aRolledCamel, string aDicesHistory) : base (aInitialBoard, aPattern, aRolledCamel)
		{
			m_ParentBoard = (BoardDebug)aInitialBoard;
			DicesHistory = aDicesHistory;

			if (IsCamelReachEnd)
				Program.PopulateFinishBoard(this);
			else if (NbRound < GameRules.MAX_ROUND_ANALYSE)
				PopulateSubBoard();
			else Program.PopulateUnfinishBoardbyMaxRound(this);
		}

		public override string ToString()
		{
			string retval = string.Empty;

			if (GetUnrolledCamelByRank().Length == 0)
				retval += BoardState + "->" + DicesHistory + " " + HighestCaseLandedOn + "\n";

			foreach (BoardDebug subBoard in m_SubBoard)
			{
				retval += subBoard.ToString();
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
				for (int i = 0; i < BoardState.Length; i++)
				{
					if (BoardState[i] == GameRules.CASE_SEPARATOR)
						caseIndex++;
					else if (GameRules.IsCharIdentityCamel(BoardState[i]))
					{
						retval += GameRules.FullNameCamel(BoardState[i]) + caseIndex;

						if (BoardState.Length - 1 > i && GameRules.IsCharIdentityCamel(BoardState[i + 1]))
						{
							retval += "->" + Char.ToUpper(BoardState[i + 1]) + " ";
						}

						retval += " ";
					}
				}
			}

			foreach (BoardDebug suBoard in m_SubBoard)
			{
				retval += suBoard.ToStringOldCamelUpFormat();
			}
			return retval + "\t";
		}

		protected override Board CreateSubboard(string aResult, char aUnrollCamel, int aDiceNb)
		{
			string dicesHistory = DicesHistory + aUnrollCamel;

			if (GameRules.USE_DICE_NB_IN_DICE_HSITORY)
				dicesHistory += aDiceNb;

			return new BoardDebug(this, aResult, aUnrollCamel, dicesHistory);
		}
	}
}
