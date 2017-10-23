using System.Collections.Generic;

namespace CamelUpData.Script
{
	public class BoardManager : MonoSingleton<BoardManager>
	{
		public readonly Dictionary<string, SmallBoard> m_UnfinishBoardByMaxRound = new Dictionary<string, SmallBoard>();
		private readonly Dictionary<string, SmallBoard> m_FinishBoard = new Dictionary<string, SmallBoard>();
		private Dictionary<string, SmallBoard> m_UncompleteBoards= new Dictionary<string, SmallBoard>();

		public int TotalWeigh
		{
			get
			{
				int retval = 0;
				//foreach (var board in m_UnfinishBoardByMaxRound)
					//retval += board.Value.Weight;
				return retval;
			}
		}

		public void CreateBoard(string aBoard)
		{
			CreateBoard(new SmallBoard(aBoard));
			
		}

		public void CreateBoardDebug(string aBoard)
		{
			CreateBoard(new SmallBoard(aBoard));
		}

		public void CreateBoardByte(string aBoard)
		{
			CreateBoard(new SmallBoard(aBoard));
		}

		private void CreateBoard(SmallBoard aBoard)
		{
			m_UncompleteBoards.Add(aBoard.BoardState, aBoard);

			while (m_UncompleteBoards.Count > 0)
			{
				Dictionary<string, SmallBoard> newUncompleteBoards = new Dictionary<string, SmallBoard>();

				foreach (var unCompleted in m_UncompleteBoards)
				{
					List<IBoard> m_SubBoard = unCompleted.Value.PopulateSubBoard();
					foreach (var subBoard in m_SubBoard)
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

		private void AddBoardIntoDict(IBoard aBoard, Dictionary<string, SmallBoard> aDict)
		{
			if (aDict.ContainsKey(aBoard.BoardStateString))
			{
				//aDict[aBoard.BoardStateString].AddWeight(aBoard);
			}
			else
			{
				aDict.Add(aBoard.BoardStateString, aBoard.GetSmallBoard());
			}
		}
	}
}
