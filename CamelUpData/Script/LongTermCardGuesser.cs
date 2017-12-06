using System.Collections.Generic;
using CamelUpData.Script.ReUse;

namespace CamelUpData.Script
{
	public class LongTermCardGuesser : MonoSingleton<LongTermCardGuesser>
	{
		private readonly List<Dictionary<char, float>> m_FirstCards = new List<Dictionary<char, float>>();
		private readonly List<Dictionary<char, float>> m_LastCards = new List<Dictionary<char, float>>();

		public void AddFirstCamelCard(string aBoard)
		{
			//TODO On peut simplifier pour ressembler plus à un humain
			var props = GetFutureCamelRankManager(aBoard).GetAllProportionByPosition(0);
			m_FirstCards.Add(props);
		}

		public void AddLastCamelCard(string aBoard)
		{
			//TODO On peut simplifier pour ressembler plus à un humain
			var props = GetFutureCamelRankManager(aBoard).GetAllProportionByPosition(GameRules.IDENTITY_CAMEL_NAME_ROLLED.Length - 1);
			m_LastCards.Add(props);
		}

		private CamelRankManager GetFutureCamelRankManager(string aBoard)
		{
			//TODO Plein de facon de faire ca.
			BoardManager bm = new BoardManager(5);
			bm.CreateBoard(aBoard);

			return new CamelRankManager(bm.GetAllBoards());
		}

		public float GetPriceForFirst(char aCamel)
		{
			return GetPrice(aCamel, m_FirstCards);
		}

		public float GetPriceForLast(char aCamel)
		{
			return GetPrice(aCamel, m_LastCards);
		}

		private float GetPrice(char aCamel, List<Dictionary<char, float>> aList)
		{
			if (aList.Count == 0)
				return GameRules.LONG_TERM_PRICE[0];

			float retval = 0f;
			var comb = MathFunc.AllCombinationsBooleans(aList.Count);

			for (int i = 0; i < comb.Count; i++)
			{
				float combTotal = 1f;
				int trueTotal = 0;
				for (int boolIndex = 0; boolIndex < comb[i].Length; boolIndex++)
				{
					if (comb[i][boolIndex])
					{
						trueTotal++;
						combTotal *= aList[boolIndex][aCamel];
					}
					else
					{
						combTotal *= 1 - aList[boolIndex][aCamel];
					}
				}

				retval += combTotal * GameRules.LONG_TERM_PRICE[trueTotal];
			}

			return retval;
		}
	}
}
