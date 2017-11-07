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

		int[] CasesLandedOn { get; }

		string BoardStateString { get;}

		void AddWeight(IBoard aBoard);

		string GetRankString();

	}
}
