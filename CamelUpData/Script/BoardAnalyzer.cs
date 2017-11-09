using System;
using System.Collections.Generic;

namespace CamelUpData.Script
{
	public class BoardAnalyzer
	{
		public Ev[] m_Evs = new Ev[Enum.GetNames(typeof(GameRules.PlayerAction)).Length];

		private Dictionary<char, CamelRank> m_CamelRanks = new Dictionary<char, CamelRank>();
		private Dictionary<char, int> m_CamelCard = new Dictionary<char, int>();
		private List<IBoard> m_Boards;

		private int[] m_CasesLandedOn { get; set; }
		private int m_TotalSubBoard { get; set; }

		public BoardAnalyzer(List<IBoard> aBoards, string aCards)
		{
			m_Boards = new List<IBoard>(aBoards);

			CreateCamelRanks();
			m_CasesLandedOn = new int[GameRules.CASE_NUMBER];
			SetCamelCard(aCards);
			PopulateCamelsRank();
			GenerateEvs();
		}

		private void SetCamelCard(string aCards)
		{
			//Format: R0O1W2Y0G0
			for (int i = 0; i < aCards.Length; i += 2)
			{
				char camel = char.ToUpper(aCards[i]);

				if (!m_CamelCard.ContainsKey(camel))
					m_CamelCard.Add(camel, -1);

				m_CamelCard[camel] = (int)char.GetNumericValue(aCards[i + 1]);
			}
		}

		private void CreateCamelRanks()
		{
			string camels = m_Boards[0].GetRankString().ToUpper();

			foreach (char camel in camels)
			{
				m_CamelRanks.Add(camel, new CamelRank(camel));
			}
		}

		private void PopulateCamelsRank()
		{
			foreach (IBoard board in m_Boards)
			{
				string rank = board.GetRankString().ToUpper();

				for (int j = 0; j < rank.Length; j++)
				{
					int currentRank = rank.Length - j - 1;
					m_CamelRanks[rank[j]].UpdateFinish(currentRank);
				}

				if (board.m_SubBoard.Count == 0)
				{
					m_TotalSubBoard+= board.Weight;
					for (int j = 0; j < board.CasesLandedOn.Length; j++)
					{
						m_CasesLandedOn[j] += board.CasesLandedOn[j] * board.Weight;
					}
				}
			}
		}

		private void GenerateEvs()
		{
			//Hardcoder??? meilleur facon?
			m_Evs[0] = GenerateShortTermCardEv();
			m_Evs[1] = GeneratePutTrapEv();
			m_Evs[2] = GenerateLongTermCardEv();
			m_Evs[3] = GenerateRollDiceEv();
		}

		private Ev GenerateShortTermCardEv()
		{	
			char highestCard = 'Z';
			float highestEv = -10;
			foreach (var camelRank in m_CamelRanks)
			{
				float current = camelRank.Value.EVShortTerm(m_CamelCard[camelRank.Key]);
				if (current > highestEv)
				{
					highestCard = camelRank.Key;
					highestEv = current;
				}
			}

			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PickShortTermCard,
				m_Ev = highestEv,
				m_Info = highestCard
			};
			return retval;
		}

		private Ev GeneratePutTrapEv()
		{
			int highestCaseLandedOn = 0;

			for (int i = 0; i < m_CasesLandedOn.Length; i++)
			{
				if (m_CasesLandedOn[i] > m_CasesLandedOn[highestCaseLandedOn])
					highestCaseLandedOn = i;
			}

			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PutTrap,
				m_Ev = (float) m_CasesLandedOn[highestCaseLandedOn] / (float) m_TotalSubBoard * GameRules.TRAP_REWARD,
				m_Info = highestCaseLandedOn + " Minus Trap NOTE: Ce n'est pas la ev Exacte. Ne prend pas en compte l'effet de la trap"
			};
			return retval;
		}

		private Ev GenerateLongTermCardEv()
		{
			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PickLongTermCard,
				m_Ev = -100,
				m_Info = "TODO!!!!!!"
			};
			return retval;
		}

		private Ev GenerateRollDiceEv()
		{
			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.RollDice,
				m_Ev = GameRules.TRAP_REWARD,
				m_Info = "NOTE: Pas le bon ev. Il faut prendre compte de l'info qu'on donne au prochain joueur"
			};

			return retval;
		}

		public override string ToString()
		{
			string retval = string.Format("Analyze of {0} boards\n", m_Boards.Count);

			foreach (var camelRank in m_CamelRanks)
			{
				char camel = char.ToUpper(camelRank.Key);
				int cardNb = m_CamelCard.ContainsKey(camel) ? m_CamelCard[camel] : 0;
				retval += camelRank.Value.EVShortTerm(cardNb).ToString("N2") + " " + camelRank + "\n";
			}

			retval += string.Format("\nHighest card short term is: {0} avec ev: {1}\n", GameRules.FullNameCamel((char)m_Evs[0].m_Info), m_Evs[0].m_Ev.ToString("N2"));
			retval += string.Format("HighestCase = {0}: EV: {1} TotalLanded: {2} \n", m_Evs[1].m_Info, m_Evs[1].m_Ev.ToString("N2"), m_TotalSubBoard);
			retval += string.Format("Highest card long term is: {0} avec ev: {1}\n", m_Evs[2].m_Info, m_Evs[2].m_Ev.ToString("N2"));
			retval += string.Format("RollDice ev: {0}. {1}\n", m_Evs[3].m_Ev.ToString("N2"), m_Evs[3].m_Info);

			Ev biggestEv = GetSortedtEvs()[0];
			retval += string.Format("\nBiggestEv: {0} avec {1}. {2} \n", biggestEv.m_PlayerAction, biggestEv.m_Ev.ToString("N2"), biggestEv.m_Info);

			return retval;
		}

		public List<Ev> GetSortedtEvs()
		{
			List<Ev> retval = new List<Ev>();

			foreach (var ev in m_Evs)
				retval.Add(ev);

			retval.Sort(delegate(Ev x, Ev y)
			{
				return y.m_Ev.CompareTo(x.m_Ev);
			});

			return retval;
		}
	}

	public class CamelRank
	{
		public char CamelName { private get; set; }

		private int m_TotalFinish;
		private int[] m_TimeFinish;
		public int TimeFinish(int aPos) { return m_TimeFinish[aPos]; }  

		public CamelRank(char aCamelName)
		{
			CamelName = aCamelName;
			m_TimeFinish = new int[GameRules.IDENTITY_CAMEL_NAME_ROLLED.Length];
		}

		public void UpdateFinish(int aRank)
		{
			m_TimeFinish[aRank]++;
			m_TotalFinish++;
		}

		public override string ToString()
		{
			string retval = string.Format("{0}: " ,CamelName.ToString());

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

	public struct Ev
	{
		public GameRules.PlayerAction m_PlayerAction;
		public float m_Ev;
		public object m_Info;
	}
}