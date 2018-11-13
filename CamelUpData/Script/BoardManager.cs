using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class BoardManager
	{
		private readonly Dictionary<string, IBoard> m_UnfinishBoardByMaxRound = new Dictionary<string, IBoard>();
		private readonly Dictionary<string, IBoard> m_FinishBoard = new Dictionary<string, IBoard>();//to private
		private Dictionary<string, IBoard> m_UncompleteBoards = new Dictionary<string, IBoard>();

		private int m_MaxDicesRoll;

		public BoardManager(int aMaxDicesRoll)
		{
			m_MaxDicesRoll = aMaxDicesRoll;
		}

		public long TotalWeigh
		{
			get
			{
				long retval = 0;
				foreach (var board in m_UnfinishBoardByMaxRound)
					retval += board.Value.Weight;
				foreach (var board in m_FinishBoard)
					retval += board.Value.Weight;
				return retval;
			}
		}

		public void CreateBoard(string aBoard)
		{
			CreateBoards(new Board(aBoard));
		}

		public void CreateBoardDebug(string aBoard)
		{
			var t = new BoardDebug(aBoard);
			CreateBoards(new BoardDebug(aBoard));
		}

		public void CreateBoardByte(string aBoard)
		{
			CreateBoards(new BoardByte(aBoard));
		}

		public List<IBoard> GetAllBoards()
		{
			List<IBoard> allBoards = new List<IBoard>();
			allBoards.AddRange(m_UnfinishBoardByMaxRound.Values);
			allBoards.AddRange(m_FinishBoard.Values);

			return allBoards;
		}

		private void CreateBoards(IBoard aBoard)
		{
			m_UncompleteBoards.Add(aBoard.BoardStateString, aBoard);

			while (m_UncompleteBoards.Count > 0)
			{
				Dictionary<string, IBoard> newUncompleteBoards = new Dictionary<string, IBoard>();

				foreach (var unCompleted in m_UncompleteBoards)
				{
					unCompleted.Value.PopulateSubBoard();

					foreach (var subBoard in unCompleted.Value.m_SubBoard)
					{
						if (subBoard.IsCamelReachEnd)
						{
							AddBoardIntoDict(subBoard, m_FinishBoard);
						}
						else if (subBoard.DicesHistories[0].Length < m_MaxDicesRoll)
							AddBoardIntoDict(subBoard, newUncompleteBoards);
						else AddBoardIntoDict(subBoard, m_UnfinishBoardByMaxRound);
					}
				}
				m_UncompleteBoards = newUncompleteBoards;
			}
			//P-e ajouter un bool pour le faire
			int maxDiceHistory = m_FinishBoard.Values.SelectMany(fb => fb.DicesHistories).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
			foreach (var board in m_FinishBoard.Values)
			{
				if (board.IsCamelReachEnd)
				{
					board.AddWeightByReachEnd(maxDiceHistory);
				}
			}
		}

		private void AddBoardIntoDict(IBoard aBoard, Dictionary<string, IBoard> aDict)
		{
			if (aDict.ContainsKey(aBoard.BoardStateString))
			{
				aDict[aBoard.BoardStateString].AddWeight(aBoard);
			}
			else
			{
				aDict.Add(aBoard.BoardStateString, aBoard);
			}
		}
	}
}
