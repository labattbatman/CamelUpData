using System;
using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class CamelRankManager
	{
		private readonly Dictionary<char, CamelRank> m_CamelRanks = new Dictionary<char, CamelRank>();

		public List<CamelRank> GetCamelRanks => m_CamelRanks.Values.ToList();

		public CamelRankManager(List<IBoard> aBoards)
		{
			CreateCamelRanks(aBoards[0]);
			PopulateCamelsRank(aBoards);
		}

		public Tuple<char, float> GetHighestEv(Dictionary<char, int> aCamelCards)
		{
			char highestCard = '*';
			float highestEv = float.MinValue;

			foreach (var camelRank in m_CamelRanks)
			{
				float current = camelRank.Value.EVShortTerm(aCamelCards[camelRank.Key]);

				if (current > highestEv)
				{
					highestCard = camelRank.Key;
					highestEv = current;
				}
			}

			return new Tuple<char, float>(highestCard, highestEv);
		}

		public string ToString(Dictionary<char, int> aCamelCards)
		{
			string retval = String.Empty;
			foreach (var camelRank in m_CamelRanks)
			{
				char camel = char.ToUpper(camelRank.Key);
				int cardNb = aCamelCards.ContainsKey(camel) ? aCamelCards[camel] : 0;
				retval += string.Format("{0} {1} \tCarteST: {2} \n", camelRank.Value.EVShortTerm(cardNb).ToString("N2"), camelRank,
					GameRules.GetRankPrice(0, cardNb));
			}

			return retval;
		}

		private void PopulateCamelsRank(List<IBoard> aBoards)
		{
			foreach (IBoard board in aBoards)
			{
				string rank = board.GetRankString().ToUpper();

				for (int j = 0; j < rank.Length; j++)
				{
					int currentRank = rank.Length - j - 1;
					m_CamelRanks[rank[j]].UpdateFinish(currentRank, board.Weight);
				}
			}
		}

		private void CreateCamelRanks(IBoard aBoard)
		{
			string camels = aBoard.GetRankString().ToUpper();

			foreach (char camel in camels)
			{
				m_CamelRanks.Add(camel, new CamelRank(camel));
			}
		}
	}

	public class CamelRank
	{
		private int[] m_TimeFinish;

		public char CamelName { get; private set; }
		public int m_TotalFinish { get; private set; }
		public int TimeFinish(int aPos) { return m_TimeFinish[aPos]; }

		public CamelRank(char aCamelName)
		{
			CamelName = aCamelName;
			m_TimeFinish = new int[GameRules.IDENTITY_CAMEL_NAME_ROLLED.Length];
		}

		public void UpdateFinish(int aRank, int aWeight)
		{
			m_TimeFinish[aRank] += aWeight;
			m_TotalFinish += aWeight;
		}

		public override string ToString()
		{
			string retval = string.Format("{0}: ", CamelName.ToString());

			for (int i = 0; i < m_TimeFinish.Length; i++)
			{
				retval += string.Format("{0}->{1} ", i + 1, m_TimeFinish[i]);
			}

			return retval;
		}

		public float EVShortTerm(int aCardNb)
		{
			float retval = 0;

			for (int i = 0; i < m_TimeFinish.Length; i++)
			{
				float rankPercentFinish = (float)m_TimeFinish[i] / (float)m_TotalFinish;
				retval += rankPercentFinish * GameRules.GetRankPrice(i, aCardNb);
			}

			return retval;
		}
	}
}
