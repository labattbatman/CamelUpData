using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class Board
{
    public readonly bool POPULATE_SUBBOARD = true;
    public readonly bool POPULATE_TILL_FINISH = false;

    public string BoardState { get; private set; }
    public int[] CasesLandedOn { get; private set; }
    public string DicesHistory { get; private set; } //Start from initial board. Used for debugging

    private Dictionary<char, string> m_Neighbouring = new Dictionary<char, string>();
    public List<Board> m_SubBoard = new List<Board>();   

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
    
    private Board(Board aInitialBoard, string aPattern, char aRolledCamel, string aDicesHistory)
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
        CasesLandedOn = (int[])aInitialBoard.CasesLandedOn.Clone();

        int camelIndex = BoardState.IndexOf(Char.ToLower(aRolledCamel));
        if (BoardState[camelIndex - 1] == GameRules.CASE_SEPARATOR)
        {
            int caseLanded = GetCamelPos(aRolledCamel);
            if(caseLanded < CasesLandedOn.Length)
                CasesLandedOn[caseLanded]++;
        }

        PopulateNeighbouring();

        if (POPULATE_SUBBOARD && !IsCamelReachEnd)
            PopulateSubBoard();	    
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
        int retval = 0;
        for (int i = 0; i < BoardState.Length; i++)
        {
            if (Char.ToLower(BoardState[i]) == Char.ToLower(aRolledCamel))
            {
                return retval;
            }
            else if(BoardState[i] == GameRules.CASE_SEPARATOR)
            {
                retval++;
            }
        }

        return -1;
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

    private void PopulateSubBoard(bool aPopulateStillRaceEnd = false)
    {       
        if (IsCamelReachEnd)
            return;

        string unRollCamel = GetUnrolledCamelByRank();
        //TODO hardcoder pour short term seulement soit quand tous les dés sont lancées. (Pas de reroll)
        aPopulateStillRaceEnd = POPULATE_TILL_FINISH;
        if (String.IsNullOrEmpty(unRollCamel) && aPopulateStillRaceEnd)
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
                if (!pattern[i].CamelsIdentity.Contains(unrollCamel.ToString()))
                {
                    //GameRules.Log("TEST" + unrollCamel + "\n");
                    continue;
                }

                List<string> results = pattern[i].GetResultsForDice(unrollCamel);
                rolledCamel += unRollCamel[j];
                for (int k = 0; k < results.Count; k++)
                {
                    string dicesHistory = DicesHistory + unrollCamel;
                    Board subBoard = new Board(this, results[k], unrollCamel, dicesHistory);
                    m_SubBoard.Add(subBoard);
                    CamelUpData.Program.HardPopulateFinishBoard(subBoard);
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
        string retval = string.Empty;
        for (int i = 0; i < BoardState.Length; i++)
        {
            if (GameRules.IsCharIdentityCamel(BoardState[i]))
                retval += BoardState[i];
        }
        return retval;
    }

    public bool IsCamelRolled(char aCamel)
    {
        return BoardState.Contains((char.ToLower(aCamel).ToString()));
    }

    public override string ToString()
    {
        string retval = string.Empty;

        if(GetUnrolledCamelByRank().Length == 0)
            retval += BoardState + "->" + DicesHistory + "\n";

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
