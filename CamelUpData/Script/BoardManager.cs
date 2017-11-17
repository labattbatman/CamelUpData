using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class BoardManager : MonoSingleton<BoardManager>
	{
		public readonly Dictionary<string, IBoard> m_UnfinishBoardByMaxRound = new Dictionary<string, IBoard>();
		private readonly Dictionary<string, IBoard> m_FinishBoard = new Dictionary<string, IBoard>();
		private Dictionary<string, IBoard> m_UncompleteBoards= new Dictionary<string, IBoard>();

		public int TotalWeigh
		{
			get
			{
				int retval = 0;
				foreach (var board in m_UnfinishBoardByMaxRound)
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
			CreateBoards(new BoardDebug(aBoard));
		}

		public void CreateBoardByte(string aBoard)
		{
			CreateBoards(new BoardByte(aBoard));
		}

		public BoardAnalyzer AnalyseBoards(string aShortTermCardRemaining)
		{
			BoardAnalyzer anal = new BoardAnalyzer(m_UnfinishBoardByMaxRound.Values.ToList(), aShortTermCardRemaining);

			GameRules.Log(anal.ToString());

			return anal;
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
							AddBoardIntoDict(subBoard, m_FinishBoard);
						else if (subBoard.DicesHistories[0].Length < GameRules.GetMaxDicesHistoryLenght)
							AddBoardIntoDict(subBoard, newUncompleteBoards);
						else AddBoardIntoDict(subBoard, m_UnfinishBoardByMaxRound);
					}
				}

				m_UncompleteBoards = newUncompleteBoards;
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
