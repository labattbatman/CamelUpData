using System;
using System.Collections.Generic;

namespace CamelUpData.Script
{
	public interface IBoard
	{
		void PopulateSubBoard();

		List<IBoard> m_SubBoard { get; set; }

		bool IsCamelReachEnd { get; }

		int NbRound { get; }

		int Weight { get; }

		string BoardStateString { get;}

		void AddWeight(IBoard aBoard);
	}
}
