using System.Collections.Generic;

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
			CreateBoard(new Board(aBoard));
			
		}

		public void CreateBoardDebug(string aBoard)
		{
			CreateBoard(new BoardDebug(aBoard));
		}

		public void CreateBoardByte(string aBoard)
		{
			CreateBoard(new BoardByte(aBoard));
		}

		private void CreateBoard(IBoard aBoard)
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
						else if (subBoard.NbRound < GameRules.MAX_ROUND_ANALYSE)
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
