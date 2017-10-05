using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CamelUpData;

public class Board
{
    public string BoardState { get; private set; }
    public int[] CasesLandedOn { get; private set; }

	public string HighestCaseLandedOn
	{
		get
		{
			int highestCase = 0;

			for(int i = 1; i < CasesLandedOn.Length; i++)
				if (CasesLandedOn[i] > CasesLandedOn[highestCase])
					highestCase = i;

			return string.Format("Case: {0}. Time: {1}" ,highestCase, CasesLandedOn[highestCase]);
		}
	}

	public int HighestTimeLandedOnSameCase
	{
		get
		{
			int highest = 0;

			for (int i = 1; i < CasesLandedOn.Length; i++)
				if (CasesLandedOn[i] > highest)
					highest = CasesLandedOn[i];

			return highest;
		}
	}

	public string DicesHistory { get; private set; } //Start from initial board. Used for debugging

	private Dictionary<char, int> m_Position = new Dictionary<char, int>();
	private Dictionary<char, string> m_Neighbouring = new Dictionary<char, string>();
    public List<Board> m_SubBoard = new List<Board>();
	private string m_Rank = string.Empty;
	private int m_NbRound;

	private Board m_ParentBoard { get; set; } //Used for debugging
    
    public bool IsCamelReachEnd { get { return FirstCamelPos >= GameRules.CASE_NUMBER; } }  

    public bool IsAllCamelRolled
    {
        get
        {
            string camels = GetRank();
            for (int i = 0; i < camels.Length; i++)
            {
                if (!IsCamelRolled(camels[i]))
                    return false;
            }

            return true;
        }
    }

    public int FirstCamelPos
    {
        get
        {
            string rank = GetRank();
            return GetCamelPos(rank[rank.Length - 1]);
        }
    }

    public Board(string aBoardId)
    {
        BoardState = aBoardId;
        CasesLandedOn = new int[GameRules.CASE_NUMBER];

        PopulateNeighbouring();
        PopulateSubBoard();
    } 
    
    private Board(Board aInitialBoard, string aPattern, char aRolledCamel, string aDicesHistory, int aRoundNb)
    {
	    m_ParentBoard = aInitialBoard;

		StringBuilder pattern = new StringBuilder(aPattern);     
        string camels = aInitialBoard.GetCamelsNeighbouring(aRolledCamel);

        for (int i = 0; i < pattern.Length; i++)
        {
            if (GameRules.IsCharPatternCamel(pattern[i]))
            {
                char camel = camels[GameRules.PATTER_NAME_NUMBER(pattern[i])];
                pattern[i] = camel == aRolledCamel ? Char.ToLower(camel) : camel;
            }
        }

        int startingPos = aInitialBoard.BoardState.IndexOf(camels[0]) - 1;
        StringBuilder newBoardState = new StringBuilder(aInitialBoard.BoardState);

        newBoardState.Remove(startingPos, Math.Min(pattern.Length, newBoardState.Length - startingPos));
        newBoardState.Insert(startingPos, pattern);

        BoardState = newBoardState.ToString();
        DicesHistory = aDicesHistory;

	    m_NbRound = aRoundNb;
		if (String.IsNullOrEmpty(GetUnrolledCamelByRank()))
		    m_NbRound++;

        CasesLandedOn = (int[])aInitialBoard.CasesLandedOn.Clone();

        int caseLanded = GetCamelPos(aRolledCamel);
        if(caseLanded < CasesLandedOn.Length)
            CasesLandedOn[caseLanded]++;

        PopulateNeighbouring(); 

		if (IsCamelReachEnd)
			Program.PopulateFinishBoard(this);
        else if (m_NbRound < GameRules.MAX_ROUND_ANALYSE)
            PopulateSubBoard();
		else Program.PopulateUnfinishBoardbyMaxRound(this);
    }

    private int GetNbCamelInPattern(string aPattern)
    {
        int nbOfCamelInPattern = 0;
        for (int i = 0; i < aPattern.Length; i++)
            if (GameRules.IsCharPatternCamel(aPattern[i]))
                nbOfCamelInPattern++;
        return nbOfCamelInPattern;
    }

    private int GetCamelPos(char aRolledCamel)
    {
	    if (!m_Position.ContainsKey(aRolledCamel))
	    {
		    int retval = 0;
		    for (int i = 0; i < BoardState.Length; i++)
		    {
			    if (Char.ToLower(BoardState[i]) == Char.ToLower(aRolledCamel))
			    {
					m_Position.Add(aRolledCamel, retval);
				}
			    else if (BoardState[i] == GameRules.CASE_SEPARATOR)
			    {
				    retval++;
			    }
		    }
	    }
	    return m_Position[aRolledCamel];
    }

    private void SetAllCamelUnroll()
    {
        StringBuilder tempBoard = new StringBuilder(BoardState);
        for(int i = 0; i < tempBoard.Length; i++)
        {
            if (GameRules.IsCharIdentityCamel(tempBoard[i]))
                tempBoard[i] = Char.ToUpper(tempBoard[i]);
        }

        BoardState = tempBoard.ToString();
    }

    private void PopulateSubBoard()
    {       
        if (IsCamelReachEnd)
            return;

        string unRollCamel = GetUnrolledCamelByRank();
        //TODO hardcoder pour short term seulement soit quand tous les dés sont lancées. (Pas de reroll)

        if (String.IsNullOrEmpty(unRollCamel))
        {        
            SetAllCamelUnroll();
            unRollCamel = GetUnrolledCamelByRank();
		}

		List<Pattern> pattern = ToPattern();

        // i = pattern
        // j = movingCamel in pattern
        // k = result
        for (int i = 0; i < pattern.Count; i++)
        {
            string rolledCamel = string.Empty;
            for (int j = 0; j < pattern[i].NbCamel && j < unRollCamel.Length; j++)
            {
                char unrollCamel = unRollCamel[j];
                if (!pattern[i].CamelsIdentity.ToUpper().Contains(unrollCamel.ToString().ToUpper()))
                {
                    //GameRules.Log("TEST" + unrollCamel + "\n");
                    continue;
                }

                List<string> results = pattern[i].GetResultsForDice(unrollCamel);
                rolledCamel += unRollCamel[j];
                for (int k = 0; k < results.Count; k++)
                {
	                string dicesHistory = DicesHistory + unrollCamel + (k + 1); // Ca faut bugger TestMethod
					//string dicesHistory = DicesHistory + unrollCamel;
                    Board subBoard = new Board(this, results[k], unrollCamel, dicesHistory, m_NbRound);
                    m_SubBoard.Add(subBoard);
				}
            }

            for (int j = 0; j < rolledCamel.Length; j++)
                unRollCamel = Regex.Replace(unRollCamel, rolledCamel[j].ToString(), string.Empty);

            rolledCamel = string.Empty;
        }
    }

    private List<Pattern> ToPattern()
    {
        string board = string.Empty;
        int camelIndex = 0;
        for (int i = 0; i < BoardState.Length; i++)
        {
            if (GameRules.IsCharIdentityCamel(BoardState[i]))
                board += GameRules.PATTERN_CAMEL_NAME[camelIndex++];
            else
                board += BoardState[i];
        }

        List<string> patterns = GameRules.PatternResultToPattern(board);
        List<Pattern> retval = new List<Pattern>();
        string camels = GetRank();
        int camelsIndex = 0;

        for (int i = 0; i < patterns.Count; i++)
        {
	        if (!PatternGenerator.Instance.Patterns.ContainsKey(patterns[i]))
	        {
				PatternGenerator.Instance.StartGeneratePattern(patterns[i]);
			}

			string camelIdentity = camels.Substring(camelsIndex, PatternGenerator.Instance.Patterns[patterns[i]].NbCamel);
            Pattern newPattern = new Pattern(PatternGenerator.Instance.Patterns[patterns[i]], camelIdentity);
            camelsIndex += newPattern.NbCamel;
            retval.Add(newPattern);
        }

        return retval;
    }

    public string GetUnrolledCamelByRank()
    {
        string retval = string.Empty;
        for(int i = 0; i < BoardState.Length; i++)
        {
            if (GameRules.IsCharIdentityCamel(BoardState[i]) 
                && Char.IsUpper(BoardState[i]))
                retval += BoardState[i];
        }

        return retval;
    }

    private char CamelToPattern(char aCamel)
    {
        int camelIndex = 0;
        for (int i = 0; i < BoardState.Length; i++)
        {
            char upperChar = char.ToUpper(BoardState[i]);

            if (upperChar == char.ToUpper(aCamel))
                break;
            else if (GameRules.IsCharIdentityCamel(upperChar))
                camelIndex++;
        }

        return GameRules.PATTERN_CAMEL_NAME[camelIndex];
    }

	public string GetRank()
	{
		if (string.IsNullOrWhiteSpace(m_Rank))
		{
			for (int i = 0; i < BoardState.Length; i++)
			{
				if (GameRules.IsCharIdentityCamel(BoardState[i]))
					m_Rank += BoardState[i];
			}
		}
		return m_Rank;
    }

    public bool IsCamelRolled(char aCamel)
    {
        return BoardState.Contains((char.ToLower(aCamel).ToString()));
    }

    public override string ToString()
    {
        string retval = string.Empty;

        if(GetUnrolledCamelByRank().Length == 0)
            retval += BoardState + "->" + DicesHistory + " " + HighestCaseLandedOn + "\n";

        for(int i = 0; i < m_SubBoard.Count; i++)
        {
            retval += m_SubBoard[i].ToString() ;
        }		

        return retval;
    }

    public string ToStringOldCamelUpFormat()
    {
        //White2->Y  Yellow2 Blue3->O Orange3 Green4
        //;;wy;bo;g
        string retval = string.Empty;

        if (GetUnrolledCamelByRank().Length == 0)
        {
            int caseIndex = 0;
            for(int i = 0; i < BoardState.Length; i++)
            {
                if(BoardState[i] == GameRules.CASE_SEPARATOR)
                    caseIndex++;
                else if (GameRules.IsCharIdentityCamel(BoardState[i]))
                {
                    retval += GameRules.FullNameCamel(BoardState[i]) + caseIndex;

                    if(BoardState.Length - 1 > i && GameRules.IsCharIdentityCamel(BoardState[i + 1]))
                    {
                        retval += "->" + Char.ToUpper(BoardState[i + 1]) + " ";
                    }

                    retval += " ";
                }
            }
        }

        for (int i = 0; i < m_SubBoard.Count; i++)
        {
            retval += m_SubBoard[i].ToStringOldCamelUpFormat();
        }
        return retval + "\t";
    }

    public void PopulateNeighbouring()
    {
        string camels = GetRank();
        for (int i = 0; i < camels.Length; i++)
            GetCamelsNeighbouring(camels[i]);
    }

    public string GetCamelsNeighbouring(char aCamel)
    {
        if (!m_Neighbouring.ContainsKey(aCamel))
        {
            string camels = string.Empty;
            string[] tempSplittedBoard = BoardState.Split(GameRules.CASE_SEPARATOR);
            List<string> splittedBoard = new List<string>();

            for (int i = 0; i < tempSplittedBoard.Length; i++)
                if (!string.IsNullOrEmpty(tempSplittedBoard[i]))
                    splittedBoard.Add(tempSplittedBoard[i]);

            for (int i = 0; i < splittedBoard.Count; i++)
            {
                if (string.IsNullOrEmpty(splittedBoard[i]) || !GameRules.IsCharIdentityCamel(splittedBoard[i][0]))
                    continue;

                if (splittedBoard[i].Contains(aCamel.ToString()))
                {
                    camels += splittedBoard[i];
                    continue;
                }
                int rolledCamelPos = GetCamelPos(aCamel);
                int nbCaseBetweenSplittedAndRolledCamel = rolledCamelPos - GetCamelPos(splittedBoard[i][0]);
                
                if (nbCaseBetweenSplittedAndRolledCamel > 0)
                {
                    if (!GameRules.IsCamelsAreTooFar(splittedBoard[i].Length, nbCaseBetweenSplittedAndRolledCamel))
                    {
                        camels += splittedBoard[i];
                    }
                    else
                    {
                        camels = string.Empty;
                    }
                }
                else
                {
                    int lastSplitPos = Math.Abs(GetCamelPos(splittedBoard[i - 1][0]));
                    int currentSplitPos = Math.Abs(GetCamelPos(splittedBoard[i][0]));

                    if (!GameRules.IsCamelsAreTooFar(splittedBoard[i - 1].Length, currentSplitPos - lastSplitPos))
                    {
                        camels += splittedBoard[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < camels.Length; i++)
                m_Neighbouring.Add(camels[i], camels);

            //GameRules.Log(camels + "\n");
        }

        return m_Neighbouring[aCamel];
    }    
}
