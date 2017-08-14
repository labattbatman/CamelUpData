using System;
using System.Collections.Generic;

public class BoardAnalyzer
{
	public Ev[] m_Evs = new Ev[Enum.GetNames(typeof(GameRules.PlayerAction)).Length];

    private Dictionary<char, CamelRank> m_CamelRanks = new Dictionary<char, CamelRank>();
	private Dictionary<char, int> m_CamelCard = new Dictionary<char, int>();
	private Board m_initialBoard;
	private string m_InitialCard;
	private List<Board> m_FinishBoards = new List<Board>();

	List<BoardAnalyzer> m_SubBoardAnalyzer = new List<BoardAnalyzer>();

	private int[] m_CasesLandedOn { get; set; }
	private int m_TotalCasesLandedOn { get; set; }

    public BoardAnalyzer(Board aBoard, string aCards)
    {
	    m_initialBoard = aBoard;
	    m_InitialCard = aCards;

		CreateCamelRanks();
	    PopulateFinishBoard(m_initialBoard);
		m_CasesLandedOn = new int[GameRules.CASE_NUMBER];
	    SetCamelCard();
	    PopulateCamelsRank();
	    GenerateEvs();
	    int ttt = 0;
    }

    private void SetCamelCard()
    {
        //Format: B0O1W2Y0G0
        for(int i = 0; i < m_InitialCard.Length; i += 2)
        {
            char camel = char.ToUpper(m_InitialCard[i]);

            if (!m_CamelCard.ContainsKey(camel))
                m_CamelCard.Add(camel, -1);

            m_CamelCard[camel] = (int)char.GetNumericValue(m_InitialCard[i + 1]);
        }
    }

	private void PopulateFinishBoard(Board aBoard)
	{
		if (aBoard.IsCamelReachEnd || aBoard.IsAllCamelRolled)
			m_FinishBoards.Add(aBoard);

		for (int i = 0; i < aBoard.m_SubBoard.Count; i++)
			PopulateFinishBoard(aBoard.m_SubBoard[i]);
	}

	private void CreateCamelRanks()
    {
	    string camels = m_initialBoard.GetRank().ToLower();

		for (int i = 0; i < camels.Length; i++)
        {
            m_CamelRanks.Add(camels[i], new CamelRank(camels[i]));
        }
    }

	private void PopulateCamelsRank()
	{
		for (int i = 0; i < m_FinishBoards.Count; i++)
		{
			string rank = m_FinishBoards[i].GetRank().ToLower();

			for (int j = 0; j < rank.Length; j++)
			{
				int currentRank = rank.Length - j - 1;
				m_CamelRanks[rank[j]].UpdateFinish(currentRank);
			}

			if (m_FinishBoards[i].m_SubBoard.Count == 0)
			{
				m_TotalCasesLandedOn++;
				for (int j = 0; j < m_FinishBoards[i].CasesLandedOn.Length; j++)
				{
					m_CasesLandedOn[j] += m_FinishBoards[i].CasesLandedOn[j];
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
			float current = camelRank.Value.EVShortTerm(m_CamelCard[char.ToUpper(camelRank.Key)]);
			if (current > highestEv)
			{
				highestCard = camelRank.Key;
				highestEv = current;
			}
		}

		Ev retval = new Ev();
		retval.m_PlayerAction = GameRules.PlayerAction.PickShortTermCard;
		retval.m_Ev = highestEv;
		retval.m_Info = highestCard;
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

		Ev retval = new Ev();
		retval.m_PlayerAction = GameRules.PlayerAction.PutTrap;
		retval.m_Ev = (float)m_CasesLandedOn[highestCaseLandedOn] / (float)m_TotalCasesLandedOn * GameRules.TRAP_REWARD; ;
		retval.m_Info = highestCaseLandedOn;
		return retval;
	}

	private Ev GenerateLongTermCardEv()
	{
		Ev retval = new Ev();
		retval.m_PlayerAction = GameRules.PlayerAction.PickLongTermCard;
		retval.m_Ev = -100;
		retval.m_Info = "TODO!!!!!!";
		return retval;
	}

	private Ev GenerateRollDiceEv()
	{
		foreach (var board in m_initialBoard.m_SubBoard)
		{
			m_SubBoardAnalyzer.Add(new BoardAnalyzer(board, m_InitialCard));
		}

		Ev retval = new Ev();
		retval.m_PlayerAction = GameRules.PlayerAction.RollDice;
		retval.m_Ev = GameRules.TRAP_REWARD;
		retval.m_Info = "TODO!!!!!!";
		return retval;
	}

    public override string ToString()
    {
        string retval = m_initialBoard.BoardState + "\n";

        foreach (var camelRank in m_CamelRanks)
        {
            char camel = char.ToUpper(camelRank.Key);
            int cardNb = m_CamelCard.ContainsKey(camel) ? m_CamelCard[camel] : 0;
            retval += camelRank.Value.EVShortTerm(cardNb).ToString("N2") + " " + camelRank.ToString() + "\n";
        }

	    retval += string.Format("\nHighest card short term is: {0} avec ev: {1}\n", GameRules.FullNameCamel((char)m_Evs[0].m_Info), m_Evs[0].m_Ev.ToString("N2"));
        retval += string.Format("HighestCase = {0}: EV: {1} TotalLanded: {2} \n", m_Evs[1].m_Info, m_Evs[1].m_Ev.ToString("N2"), m_TotalCasesLandedOn);
		retval += string.Format("Highest card long term is: {0} avec ev: {1}\n", m_Evs[2].m_Info, m_Evs[2].m_Ev.ToString("N2"));
	    retval += string.Format("RollDice ev: {0}. {1}\n", m_Evs[3].m_Ev.ToString("N2"), m_Evs[3].m_Info);

	    Ev biggestEv = GetBiggestEv();
		retval += string.Format("\nBiggestEv: {0} avec {1}. {2} \n", biggestEv.m_PlayerAction, biggestEv.m_Ev, biggestEv.m_Info);

		return retval;
    }

	public string ToStringLong()
	{
		string retval = ToString();

		retval += "\nSUB BOARD ANALYZER \n";

		foreach (var boardAnalyzer in m_SubBoardAnalyzer)
		{
			retval += boardAnalyzer + "\n";
		}

		return retval;
	}

	public Ev GetBiggestEv()
	{
		Ev retval = new Ev();

		foreach (var ev in m_Evs)
		{
			if (ev.m_Ev > retval.m_Ev)
				retval = ev;
		}

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
        m_TimeFinish = new int[GameRules.IDENTITY_CAMEL_NAME.Length];
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