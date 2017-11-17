using System;
using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class BoardAnalyzer
	{
		public Ev[] m_Evs = new Ev[Enum.GetNames(typeof(GameRules.PlayerAction)).Length];

		private readonly Dictionary<char, CamelRank> m_CamelRanks = new Dictionary<char, CamelRank>();
		private readonly Dictionary<char, int> m_CamelCard = new Dictionary<char, int>();
		private readonly List<IBoard> m_Boards = new List<IBoard>();

		private int[] m_CasesLandedOn { get; set; }
		public int m_TotalSubBoardWithWeight { get; private set; }

        private string m_CamelCardString;

		public BoardAnalyzer(List<IBoard> aBoards, string aCards)
		{
			m_Boards = new List<IBoard>(aBoards);
            Setup(aCards, true);
		}

		private BoardAnalyzer(List<IBoard> aBoards, string aCards, string aRules)
		{
			foreach (var aBoard in aBoards)
			{
				int newWeight = 0;
				foreach (var hist in aBoard.DicesHistories)
				{
					if (hist.StartsWith(aRules))
						newWeight++;
				}
				if (newWeight > 0)
				{
					aBoard.RemoveWeight(newWeight);
					m_Boards.Add(aBoard);
				}
			}

            Setup(aCards, false);
        }

        private void Setup(string aCards, bool aIsCalculatingRollDice)
        {
            CreateCamelRanks();
            m_CasesLandedOn = new int[GameRules.CASE_NUMBER];
            SetCamelCard(aCards);
            PopulateCamelsRank();
            GenerateEvs(aIsCalculatingRollDice);
        }

		private void SetCamelCard(string aCards)
		{
            //Format: B0O1W2Y0G0
            m_CamelCardString = aCards;
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
					m_CamelRanks[rank[j]].UpdateFinish(currentRank, board.Weight);
				}

				if (board.m_SubBoard.Count == 0)
				{
					m_TotalSubBoardWithWeight+= board.Weight;
					for (int j = 0; j < board.CasesLandedOn.Length; j++)
					{
						m_CasesLandedOn[j] += board.CasesLandedOn[j] * board.Weight;
					}
				}
			}
		}

		private void GenerateEvs(bool aIsCalculatingRollDice)
		{
			m_Evs[0] = GenerateShortTermCardEv();
			m_Evs[1] = GeneratePutTrapEv();
			m_Evs[2] = GenerateLongTermCardEv();
            if(aIsCalculatingRollDice)
			    m_Evs[3] = GenerateRollDiceEv();
		}

		private Ev GenerateShortTermCardEv()
		{	
			char highestCard = '*';
			float highestEv = float.MinValue;

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
				m_Info = GameRules.FullNameCamel(highestCard)
			};
			return retval;
		}

		private Ev GeneratePutTrapEv()
		{
			List<int> highestCases = new List<int> {0};

			for (int i = 0; i < m_CasesLandedOn.Length; i++)
			{
				if (m_CasesLandedOn[i] >= m_CasesLandedOn[highestCases[0]])
				{
					if(m_CasesLandedOn[i] > m_CasesLandedOn[highestCases[0]])
						highestCases.Clear();

					highestCases.Add(i);
				}
			}

			string casesRank = string.Empty;
			foreach (var highestCase in highestCases)
				casesRank += highestCase + ", ";

			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PutTrap,
				m_Ev = (float) m_CasesLandedOn[highestCases[0]] / (float) m_TotalSubBoardWithWeight * GameRules.TRAP_REWARD,
				m_Info = string.Format("Case(s): {0}. Minus Trap. Pas EV exacte.", casesRank)
			};
			return retval;
		}

		private Ev GenerateLongTermCardEv()
		{
			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PickLongTermCard,
				m_Ev = -9,
				m_Info = "TODO!!!!!!"
			};
			return retval;
		}

		private Ev GenerateRollDiceEv()
		{
            float evNextTurn = 0;

            foreach (var camel in GameRules.IDENTITY_CAMEL_NAME_UNROLLED)
                evNextTurn += new BoardAnalyzer(m_Boards, m_CamelCardString, camel.ToString()).GetSortedtEvs()[0].m_Ev;

            evNextTurn = evNextTurn / GameRules.IDENTITY_CAMEL_NAME_UNROLLED.Length;
            float secondEv = GetSortedtEvs()[1].m_Ev;

            Ev retval = new Ev
            {
                m_PlayerAction = GameRules.PlayerAction.RollDice,
                m_Ev = GameRules.TRAP_REWARD - (evNextTurn - secondEv)
            };

            return retval;
		}

		public override string ToString()
		{
			string retval = string.Format("Analyze of {0} boards with a weight of {1}\n", m_Boards.Count, m_TotalSubBoardWithWeight);
			retval += "---------------------------------\n";

			foreach (var camelRank in m_CamelRanks)
			{
				char camel = char.ToUpper(camelRank.Key);
				int cardNb = m_CamelCard.ContainsKey(camel) ? m_CamelCard[camel] : 0;
				retval += string.Format("{0} {1} \tCarteST: {2} \n", camelRank.Value.EVShortTerm(cardNb).ToString("N2"), camelRank,
					GameRules.GetRankPrice(0, cardNb));
			}

			retval += "---------------------------------\n";

			retval += string.Format("Highest card short term is\nEV: {1} {0}\n\n", m_Evs[0].m_Info, m_Evs[0].m_Ev.ToString("N2"));
			retval += string.Format("HighestCase               \nEV: {1} {0}\n\n", m_Evs[1].m_Info, m_Evs[1].m_Ev.ToString("N2"));
			retval += string.Format("Highest card long term is \nEV: {1} {0}\n\n", m_Evs[2].m_Info, m_Evs[2].m_Ev.ToString("N2"));
			retval += string.Format("RollDice                  \nEV: {0} {1}\n\n", m_Evs[3].m_Ev.ToString("N2"), m_Evs[3].m_Info);

			retval += "---------------------------------\n";

			Ev biggestEv = GetSortedtEvs()[0];
			retval += string.Format("BestAction: {0} EV: {1}. {2} \n", biggestEv.m_PlayerAction, biggestEv.m_Ev.ToString("N2"), biggestEv.m_Info);

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

		public void UpdateFinish(int aRank, int aWeight)
		{
			m_TimeFinish[aRank]+= aWeight;
			m_TotalFinish+= aWeight;
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

	public struct Ev
	{
		public GameRules.PlayerAction m_PlayerAction;
		public float m_Ev;
		public object m_Info;
	}
}