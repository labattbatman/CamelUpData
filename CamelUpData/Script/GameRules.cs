using System;
using System.Text;
using System.Collections.Generic;
#if UsingUnity
using UnityEngine;
#endif

public class GameRules
{
	//Info:
	// Minuscule si dés du camel est sorti.

	public enum PlayerAction
	{
		RollDice,
		PickShortTermCard,
		PickLongTermCard,
		PutTrap,
	}

	public static readonly bool POPULATE_SUBBOARD = true;
	public static readonly bool POPULATE_TILL_FINISH = false;

	#region Game Rules
	public static readonly int DICE_NB_FACES = 3;
    public static readonly int CASE_NUMBER = 7;

    public static readonly int[] SHORT_TERM_FIRST_PRICE = new int[] { 5, 3, 2 };
    public static readonly int SHORT_TERM_SECOND_PRICE = 1;
    public static readonly int SHORT_TERM_LAST_PRICE = -1;

    public static readonly int TRAP_REWARD = 1;
    public static readonly int TRAP_PLUS_MODIFIER = 1;
    public static readonly int TRAP_MINUS_MODIFIER = -1;
    public static readonly bool IS_SHUTTLE_WHEN_HITTING_MINUS_TRAP = false;

    public static int GetRankPrice(int aRank, int aCardNb)
    {
        switch(aRank)
        {
            case 0: return SHORT_TERM_FIRST_PRICE[aCardNb];
            case 1: return SHORT_TERM_SECOND_PRICE;
            default: return SHORT_TERM_LAST_PRICE;
        }
    }
    #endregion

    #region Board Layout
    public static readonly char PATTERN_SAVE_ID_RESULT_SEPERATOR = '?';
    public static readonly char PATTERN_RESULT_SEPARATOR = '&';
    public static readonly char PATTERN_RESULT_NAME_SEPARATOR = ':';
    public static readonly char CASE_SEPARATOR = ';';
    public static readonly char TRAP_PLUS = '+';
    public static readonly char TRAP_MINUS = '-';
    public static readonly char[] PATTERN_CAMEL_NAME = new char[] { 'A', 'B', 'C', 'D', 'E' };
    public static readonly char[] IDENTITY_CAMEL_NAME = new char[] { 'O', 'B', 'W', 'G', 'Y' };   

    private static string m_PatternCamelNames = string.Empty;
    public static string PATTERN_CAMEL_NAME_IN_STRING
    {
        get
        {
            if (String.IsNullOrEmpty(m_PatternCamelNames))
            {
                string retval = string.Empty;
                for (int i = 0; i < PATTERN_CAMEL_NAME.Length; i++)
                {
                    m_PatternCamelNames += PATTERN_CAMEL_NAME[i];
                    m_PatternCamelNames += Char.ToLower(PATTERN_CAMEL_NAME[i]);
                }
            }
            return m_PatternCamelNames;
        }
    }

    public static bool IsCharPatternCamel(string aChar)
    {
        if(aChar.Length == 1)
            return IsCharPatternCamel(aChar[0]);

        Log("Error: aChar is too long: " + aChar);
        return false;
    }
    public static bool IsCharPatternCamel(char aChar)
    {
        return PATTERN_CAMEL_NAME_IN_STRING.Contains(aChar.ToString());
    }

    private static string m_IdentityCamelNames = string.Empty;
    public static string IDENTITY_CAMEL_NAME_IN_STRING
    {
        get
        {
            if (String.IsNullOrEmpty(m_IdentityCamelNames))
            {
                string retval = string.Empty;
                for (int i = 0; i < IDENTITY_CAMEL_NAME.Length; i++)
                {
                    m_IdentityCamelNames += IDENTITY_CAMEL_NAME[i];
                    m_IdentityCamelNames += Char.ToLower(IDENTITY_CAMEL_NAME[i]);
                }
            }
            return m_IdentityCamelNames;
        }
    }

    public static bool IsCharIdentityCamel(char aChar)
    {
        return IDENTITY_CAMEL_NAME_IN_STRING.Contains(aChar.ToString());
    }

    public static int PATTER_NAME_NUMBER(char aPatternName)
    {
        for(int i = 0; i < PATTERN_CAMEL_NAME.Length; i++)
            if (PATTERN_CAMEL_NAME[i] == aPatternName)
                return i;

        return -1;
    }

    public static string FullNameCamel(char aChar)
    {
        switch(aChar)
        {
            case 'o':
            case 'O': return "Orange";

            case 'w':
            case 'W': return "White";

            case 'b':
            case 'B': return "Blue";

            case 'g':
            case 'G': return "Green";

            case 'y':
            case 'Y': return "Yellow";


            default: return "Null ->" + aChar;
        }
    }
    #endregion

    //todo find another place
    public static List<string> PatternResultToPattern(string result)
    {
        List<string> retval = new List<string>();
        CamelsMovement camelsMovement = new CamelsMovement(result);
        string holePattern = camelsMovement.StartingCamelsInBoard;

        //Before Camel A
        while (!GameRules.IsCharPatternCamel(holePattern.Substring(1, 1)))
            holePattern = holePattern.Substring(1, holePattern.Length - 1);

        //BetweenCamel
        int camelNb = 0;
        int caseSinceLastCamel = 0;
        int nbCamelsOnLastPile = 1;
        bool isCamelSameCase = false;

        for (int i = 0; i < holePattern.Length; i++)
        {        
            if (holePattern[i] == GameRules.CASE_SEPARATOR)
            {
                caseSinceLastCamel++;
                isCamelSameCase = false;
            }
            else if (holePattern[i] != GameRules.TRAP_MINUS && holePattern[i] != GameRules.TRAP_PLUS)
            {                          
                if (IsCamelsAreTooFar(nbCamelsOnLastPile, caseSinceLastCamel))
                {
                    retval.Add(holePattern.Substring(0, i - caseSinceLastCamel));
                    holePattern = holePattern.Substring(i - 1, holePattern.Length - i + 1);
                    i = 1;
                    camelNb = 0;
                }

                if (!isCamelSameCase)
                {
                    isCamelSameCase = true;
                    nbCamelsOnLastPile = 0;
                }

                nbCamelsOnLastPile++;

                //Override Camel     
                StringBuilder sb = new StringBuilder(holePattern);
                sb[i] = GameRules.PATTERN_CAMEL_NAME[camelNb++];
                holePattern = sb.ToString();

                caseSinceLastCamel = 0;
            }
            else
            {
	            //TODO BUG ICI mauvais pattern ;A;+;+ -> ;A; pas sur si cest la bonne solution
				caseSinceLastCamel = 0;
            }

            //Debug.Log(string.Format(" {4} -> camelNb: {0}, caseSinceLastCamel: {1}, nbCamelsOnLastPile: {2}, isCamelSameCase: {3}",
            //    camelNb, caseSinceLastCamel, nbCamelsOnLastPile, isCamelSameCase, holePattern[i]));
        }
        retval.Add(holePattern);
        return retval;
    }

    public static bool IsCamelsAreTooFar(int aNbOnSlowestPileOfCamel, int aNbOfCaseBetweenCamels)
    {      
        return aNbOnSlowestPileOfCamel * GameRules.DICE_NB_FACES < aNbOfCaseBetweenCamels;
    }

    public static void Log(string aLog)
    {
#if UsingUnity
        UnityEngine.Debug.Log(aLog);
#else
        Console.Write(aLog);
#endif
    }
}