using System.Collections.Generic;

namespace CamelUpData.Script
{
	public interface IBoard
	{
		List<IBoard> PopulateSubBoard();

		bool IsCamelReachEnd { get; }

		int NbRound { get; }

		int Weight { get; }

		string BoardStateString { get;}

		void AddWeight(IBoard aBoard);

		SmallBoard GetSmallBoard();
	}

	public class SmallBoard
	{
		public string BoardState { get; private set; }

		public SmallBoard(IBoard aBoard)
		{
			BoardState = aBoard.BoardStateString;
		}

		public SmallBoard(string aBoard)
		{
			BoardState = aBoard;
		}

		public List<IBoard> PopulateSubBoard()
		{
			Board newBoard = new Board(BoardState);
			return newBoard.PopulateSubBoard();
		}
	}
}
