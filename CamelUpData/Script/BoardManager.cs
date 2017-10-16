using System.Collections.Generic;

namespace CamelUpData.Script
{
	public class BoardManager : MonoSingleton<BoardManager>
	{
		public readonly Dictionary<string, Board> m_UnfinishBoardByMaxRound = new Dictionary<string, Board>();
		private readonly Dictionary<string, Board> m_FinishBoard = new Dictionary<string, Board>();
		private Dictionary<string, Board> m_UncompleteBoards= new Dictionary<string, Board>();

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

		private void CreateBoard(Board aBoard)
		{
			m_UncompleteBoards.Add(aBoard.BoardState, aBoard);

			while (m_UncompleteBoards.Count > 0)
			{
				Dictionary<string, Board> newUncompleteBoards = new Dictionary<string, Board>();

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

		private void AddBoardIntoDict(Board aBoard, Dictionary<string, Board> aDict)
		{
			if (aDict.ContainsKey(aBoard.BoardState))
			{
				aDict[aBoard.BoardState].AddWeight(aBoard);
			}
			else
			{
				aDict.Add(aBoard.BoardState, aBoard);
			}
		}
	}
}
