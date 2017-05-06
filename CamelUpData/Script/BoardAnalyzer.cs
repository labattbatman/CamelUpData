using System.Collections.Generic;

public class BoardAnalyzer
{
    public Dictionary<char, CamelRank> m_CamelRanks = new Dictionary<char, CamelRank>();

    private Dictionary<char, int> m_CamelCard = new Dictionary<char, int>();
    private int[] m_CasesLandedOn { get; set; }
    private int m_TotalCasesLandedOn { get; set; }

    public BoardAnalyzer(List<Board> aFinishBoard)
    {
        CreateCamelRanks(aFinishBoard[0].GetRank().ToLower());
        m_CasesLandedOn = new int[GameRules.CASE_NUMBER];

        for (int i = 0; i < aFinishBoard.Count; i++)
        {
            string rank = aFinishBoard[i].GetRank().ToLower();

            for (int j = 0; j < rank.Length; j++)
            {
                int currentRank = rank.Length - j - 1;
                m_CamelRanks[rank[j]].UpdateFinish(currentRank);
            }

            for (int j = 0; j < aFinishBoard[i].CasesLandedOn.Length; j++)
                m_CasesLandedOn[j] += aFinishBoard[i].CasesLandedOn[j];
        }

        m_TotalCasesLandedOn = 0;

        for (int i = 0; i < m_CasesLandedOn.Length; i++)
            m_TotalCasesLandedOn += m_CasesLandedOn[i];
    }

    public void SetCamelCard(string aCards)
    {
        //Format: B0O1W2Y0G0
        for(int i = 0; i < aCards.Length; i += 2)
        {
            char camel = char.ToUpper(aCards[i]);

            if (!m_CamelCard.ContainsKey(camel))
                m_CamelCard.Add(camel, -1);

            m_CamelCard[camel] = (int)char.GetNumericValue(aCards[i + 1]);
        }
    }

    private void CreateCamelRanks(string aCamels)
    {
        for(int i = 0; i < aCamels.Length; i++)
        {
            m_CamelRanks.Add(aCamels[i], new CamelRank(aCamels[i]));
        }
    }

    private int GetHighLandedCase()
    {
        int retval = 0;

        for(int i = 0; i < m_CasesLandedOn.Length; i++)
        {
            if (m_CasesLandedOn[i] > m_CasesLandedOn[retval])
                retval = i;
        }

        return retval;
    }

    private float GetEvForCase(int aCase)
    {      
        return (float)m_CasesLandedOn[aCase] / (float)m_TotalCasesLandedOn * GameRules.TRAP_REWARD;
    }

    public override string ToString()
    {
        string retval = string.Empty;

        foreach (var camelRank in m_CamelRanks)
        {
            char camel = char.ToUpper(camelRank.Key);
            int cardNb = m_CamelCard.ContainsKey(camel) ? m_CamelCard[camel] : 0;
            retval += camelRank.Value.EVShortTerm(cardNb) + " " + camelRank.ToString() + "\n";
        }
        int highestCase = GetHighLandedCase();
        retval += string.Format("$$Case = {0}: EV: {1} TotalLanded: {2} \n", highestCase, GetEvForCase(highestCase), m_TotalCasesLandedOn);

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