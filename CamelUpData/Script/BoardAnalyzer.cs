using System;
using System.Collections.Generic;

namespace CamelUpData.Script
{
	public class BoardAnalyzer
	{
		public Ev[] m_Evs = new Ev[Enum.GetNames(typeof(GameRules.PlayerAction)).Length];
		public int m_TotalSubBoardWithWeight { get; private set; }

		private int[] m_CasesLandedOn { get; set; }
		private readonly Dictionary<char, int> m_CamelCards = new Dictionary<char, int>();
		private string m_CamelCardString; //Format: B0O1W2Y0G0
		private readonly List<IBoard> m_Boards = new List<IBoard>();
		private CamelRankManager m_CamelRankManager;

		private string m_OriginBoard;

		public BoardAnalyzer(string aOriginBoard, List<IBoard> aBoards, string aCards)
		{
			m_Boards = new List<IBoard>(aBoards);
			Setup(aOriginBoard, aCards, true);
		}

		private BoardAnalyzer(string aOriginBoard, List<IBoard> aBoards, string aCards, string aRules)
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

			Setup(aOriginBoard, aCards, false);
		}

		private void Setup(string aOriginBoard, string aCards, bool aIsCalculatingRollDice)
		{
			m_OriginBoard = aOriginBoard;

			m_CamelRankManager = new CamelRankManager(m_Boards);
			m_CasesLandedOn = new int[GameRules.CASE_NUMBER];

			SetCamelCard(aCards);
			CalculateWeightAndCasesLandedOn();
			GenerateEvs(aIsCalculatingRollDice);
		}

		private void SetCamelCard(string aCards)
		{
			m_CamelCardString = aCards;
			for (int i = 0; i < m_CamelCardString.Length; i += 2)
			{
				char camel = char.ToUpper(m_CamelCardString[i]);

				if (!m_CamelCards.ContainsKey(camel))
					m_CamelCards.Add(camel, -1);

				m_CamelCards[camel] = (int)char.GetNumericValue(m_CamelCardString[i + 1]);
			}
		}

		private void CalculateWeightAndCasesLandedOn()
		{
			foreach (var board in m_Boards)
			{
				if (board.m_SubBoard.Count == 0)
				{
					m_TotalSubBoardWithWeight += board.Weight;
					for (int j = 0; j < board.CasesLandedOn.Length; j++)
					{
						m_CasesLandedOn[j] += board.CasesLandedOn[j] * board.Weight;
					}
				}
				else
				{
					GameRules.Log("");
				}
			}
		}

		private void GenerateEvs(bool aIsCalculatingRollDice)
		{
			m_Evs[0] = GenerateShortTermCardEv();
			m_Evs[1] = GeneratePutTrapEv();
			if (aIsCalculatingRollDice)
			{
				m_Evs[3] = GenerateRollDiceEv();
				//m_Evs[2] = GenerateLongTermCardEv(); //TODO inclure ou ne pas inclure
			}
		}

		private Ev GenerateShortTermCardEv()
		{
			var highestCamelEv = m_CamelRankManager.GetHighestEv(m_CamelCards);
			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PickShortTermCard,
				m_Ev = highestCamelEv.Item2,
				m_Info = GameRules.FullNameCamel(highestCamelEv.Item1)
			};
			return retval;
		}

		private Ev GeneratePutTrapEv()
		{
			List<int> highestCases = new List<int> { 0 };

			for (int i = 0; i < m_CasesLandedOn.Length; i++)
			{
				if (m_CasesLandedOn[i] >= m_CasesLandedOn[highestCases[0]])
				{
					if (m_CasesLandedOn[i] > m_CasesLandedOn[highestCases[0]])
						highestCases.Clear();

					highestCases.Add(i);
				}
			}

			string casesRank = string.Empty;
			for (int i = 0; i < highestCases.Count; i++)
			{
				if (i != 0) casesRank += ", ";
				casesRank += highestCases[i];
			}

			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PutTrap,
				m_Ev = (float)m_CasesLandedOn[highestCases[0]] / (float)m_TotalSubBoardWithWeight * GameRules.TRAP_REWARD,
				m_Info = string.Format("Case(s): {0}. Minus Trap. Pas EV exacte.", casesRank)
			};
			return retval;
		}

		private Ev GenerateLongTermCardEv()
		{
			LongTermBoardAnalyser ltba = new LongTermBoardAnalyser(new Board(m_OriginBoard), ClearDictionaries);
			return ltba.GetEv();
		}

		private Ev GenerateRollDiceEv()
		{
			float evNextTurn = 0;

			foreach (var camel in GameRules.IDENTITY_CAMEL_NAME_UNROLLED)
            {
                if(m_OriginBoard.Contains(camel.ToString()))
                    evNextTurn += new BoardAnalyzer(m_OriginBoard, m_Boards, m_CamelCardString, camel.ToString()).GetSortedtEvs()[0].m_Ev;
            }
				

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
			string retval = string.Format("\n\nAnalyze of {0} boards with a weight of {1}\n", m_Boards.Count, m_TotalSubBoardWithWeight);
			retval += "---------------------------------\n";

			//retval += m_CamelRankManager.ToString(m_CamelCards);

			retval += "---------------------------------\n";

			retval += string.Format("Highest card short term is\nEV: {1} {0}\n\n", m_Evs[0].m_Info, m_Evs[0].m_Ev.ToString("N2"));
			retval += string.Format("HighestCase               \nEV: {1} {0}\n\n", m_Evs[1].m_Info, m_Evs[1].m_Ev.ToString("N2"));
			retval += string.Format("Highest card long term is \nEV: {1} {0}\n\n", m_Evs[2].m_Info, m_Evs[2].m_Ev.ToString("N2"));
			retval += string.Format("RollDice                  \nEV: {0} {1}\n\n", m_Evs[3].m_Ev.ToString("N2"), m_Evs[3].m_Info);

			retval += "---------------------------------\n";
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

			retval.Sort(delegate (Ev x, Ev y)
			{
				return y.m_Ev.CompareTo(x.m_Ev);
			});

			return retval;
		}

		private void ClearDictionaries()
		{
			m_CamelRankManager = null;
			m_CamelCards.Clear();
			m_Boards.Clear();
		}
	}

	public struct Ev
	{
		public GameRules.PlayerAction m_PlayerAction;
		public float m_Ev;
		public object m_Info;
	}
}