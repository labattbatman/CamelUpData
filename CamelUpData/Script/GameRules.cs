using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if UsingUnity
using UnityEngine;
#endif

namespace CamelUpData.Script
{
	public class GameRules
	{
		//Info: Minuscule si dés du camel est sorti.

		public enum PlayerAction
		{
			RollDice,
			PickShortTermCard,
			PickLongTermCard,
			PutTrap,
		}

		public static bool USE_DICE_NB_IN_DICE_HSITORY = true;

		private static readonly int MAX_DICES_ROLLED_ANALYSE = 1;
		public static int GetMaxDicesHistoryLenght
		{
			get
			{
				if (MaxDicesHistoryLenght < 0)
					MaxDicesHistoryLenght = MAX_DICES_ROLLED_ANALYSE * 2;

				return MaxDicesHistoryLenght;
			}
		}

		private static int MaxDicesHistoryLenght = -1;

		#region Game Rules
		public static readonly int DICE_NB_FACES = 3;
		public static readonly int CASE_NUMBER = 20;

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
		#region Board Layout String
		public const char PATTERN_SAVE_ID_RESULT_SEPERATOR = '?';
		public const char PATTERN_RESULT_SEPARATOR = '&';
		public const char PATTERN_RESULT_NAME_SEPARATOR = ':';
		public const char CASE_SEPARATOR = ';';
		public const char TRAP_PLUS = '+';
		public const char TRAP_MINUS = '-';

		private const char PATTERN_CAMEL_NAME_1 = 'A';
		private const char PATTERN_CAMEL_NAME_2 = 'C';
		private const char PATTERN_CAMEL_NAME_3 = 'D';
		private const char PATTERN_CAMEL_NAME_4 = 'E';
		private const char PATTERN_CAMEL_NAME_5 = 'F';
		public static readonly char[] PATTERN_CAMEL_NAME =  { PATTERN_CAMEL_NAME_1, PATTERN_CAMEL_NAME_2, PATTERN_CAMEL_NAME_3, PATTERN_CAMEL_NAME_4, PATTERN_CAMEL_NAME_5 };

		private const char IDENTITY_CAMEL_NAME_ROLLED_1 = 'o';
		private const char IDENTITY_CAMEL_NAME_ROLLED_2 = 'b';
		private const char IDENTITY_CAMEL_NAME_ROLLED_3 = 'w';
		private const char IDENTITY_CAMEL_NAME_ROLLED_4 = 'g';
		private const char IDENTITY_CAMEL_NAME_ROLLED_5 = 'y';
		public static readonly char[] IDENTITY_CAMEL_NAME_ROLLED = { IDENTITY_CAMEL_NAME_ROLLED_1, IDENTITY_CAMEL_NAME_ROLLED_2, IDENTITY_CAMEL_NAME_ROLLED_3, IDENTITY_CAMEL_NAME_ROLLED_4, IDENTITY_CAMEL_NAME_ROLLED_5 };

		private const char IDENTITY_CAMEL_NAME_UNROLLED_1 = 'O';
		private const char IDENTITY_CAMEL_NAME_UNROLLED_2 = 'B';
		private const char IDENTITY_CAMEL_NAME_UNROLLED_3 = 'W';
		private const char IDENTITY_CAMEL_NAME_UNROLLED_4 = 'G';
		private const char IDENTITY_CAMEL_NAME_UNROLLED_5 = 'Y';
		public static readonly char[] IDENTITY_CAMEL_NAME_UNROLLED = { IDENTITY_CAMEL_NAME_UNROLLED_1, IDENTITY_CAMEL_NAME_UNROLLED_2, IDENTITY_CAMEL_NAME_UNROLLED_3, IDENTITY_CAMEL_NAME_UNROLLED_4, IDENTITY_CAMEL_NAME_UNROLLED_5 };

		private static string m_PatternCamelNames = string.Empty;
		public static string PATTERN_CAMEL_NAME_IN_STRING
		{
			get
			{
				if (String.IsNullOrEmpty(m_PatternCamelNames))
				{
					foreach (char c in PATTERN_CAMEL_NAME)
					{
						m_PatternCamelNames += c;
						m_PatternCamelNames += Char.ToLower(c);
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
					foreach (char c in IDENTITY_CAMEL_NAME_ROLLED)
						m_IdentityCamelNames += c;

					foreach (char c in IDENTITY_CAMEL_NAME_UNROLLED)
						m_IdentityCamelNames += c;
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

				case 'r':
				case 'R':
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
		#region Board Layout Byte
		public const byte PATTERN_SAVE_ID_RESULT_SEPERATOR_BYTE = 0;
		public const byte PATTERN_RESULT_SEPARATOR_BYTE = 1;
		public const byte PATTERN_RESULT_NAME_SEPARATOR_BYTE = 2;
		public const byte CASE_SEPARATOR_BYTE = 3;
		public const byte TRAP_PLUS_BYTE = 4;
		public const byte TRAP_MINUS_BYTE = 5;

		private const byte PATTERN_CAMEL_NAME_1_BYTE = 51;
		private const byte PATTERN_CAMEL_NAME_2_BYTE = 52;
		private const byte PATTERN_CAMEL_NAME_3_BYTE = 53;
		private const byte PATTERN_CAMEL_NAME_4_BYTE = 54;
		private const byte PATTERN_CAMEL_NAME_5_BYTE = 55;
		public static readonly byte[] PATTERN_CAMEL_NAME_BYTE = { PATTERN_CAMEL_NAME_1_BYTE, PATTERN_CAMEL_NAME_2_BYTE, PATTERN_CAMEL_NAME_3_BYTE, PATTERN_CAMEL_NAME_4_BYTE, PATTERN_CAMEL_NAME_5_BYTE };

		private const byte IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE = 101;
		private const byte IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE = 102;
		private const byte IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE = 103;
		private const byte IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE = 104;
		private const byte IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE = 105;
		public static readonly byte[] IDENTITY_CAMEL_NAME_UNROLLED_BYTE = { IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE, IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE, IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE, IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE, IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE };

		private const byte IDENTITY_CAMEL_NAME_ROLLED_1_BYTE = 201;
		private const byte IDENTITY_CAMEL_NAME_ROLLED_2_BYTE = 202;
		private const byte IDENTITY_CAMEL_NAME_ROLLED_3_BYTE = 203;
		private const byte IDENTITY_CAMEL_NAME_ROLLED_4_BYTE = 204;
		private const byte IDENTITY_CAMEL_NAME_ROLLED_5_BYTE = 205;
		public static readonly byte[] IDENTITY_CAMEL_NAME_ROLLED_BYTE = { IDENTITY_CAMEL_NAME_ROLLED_1_BYTE, IDENTITY_CAMEL_NAME_ROLLED_2_BYTE, IDENTITY_CAMEL_NAME_ROLLED_3_BYTE, IDENTITY_CAMEL_NAME_ROLLED_4_BYTE, IDENTITY_CAMEL_NAME_ROLLED_5_BYTE };

		public static bool IsBytePatternCamel(byte aByte)
		{
			foreach (var patternByte in PATTERN_CAMEL_NAME_BYTE)
			{
				if (patternByte == aByte)
					return true;
			}

			return false;
		}

		public static bool IsByteIdentityCamel(byte aByte)
		{
			return IsByteIdentityCamelUnrolled(aByte) || IsByteIdentityCamelRolled(aByte);
		}

		public static bool IsByteIdentityCamelUnrolled(byte aByte)
		{
			foreach (var patternByte in IDENTITY_CAMEL_NAME_UNROLLED_BYTE)
			{
				if (patternByte == aByte)
					return true;
			}

			return false;
		}

		public static bool IsByteIdentityCamelRolled(byte aByte)
		{
			foreach (var patternByte in IDENTITY_CAMEL_NAME_ROLLED_BYTE)
			{
				if (patternByte == aByte)
					return true;
			}

			return false;
		}

		public static int PATTER_NAME_NUMBER_BYTE(byte aPatternName)
		{
			for (int i = 0; i < PATTERN_CAMEL_NAME_BYTE.Length; i++)
				if (PATTERN_CAMEL_NAME_BYTE[i] == aPatternName)
					return i;

			return -1;
		}
		#endregion
		#endregion

		public static byte[] StringToByte(string aString)
		{
			byte[] retval = new byte[aString.Length];

			for (int i = 0; i < aString.Length; i++)
				retval[i] = StringToByte(aString[i]);

			return retval;
		}

		public static byte StringToByte(char aChar)
		{
			switch (aChar)
			{
				case CASE_SEPARATOR:
					return CASE_SEPARATOR_BYTE;

				case TRAP_PLUS:
					return TRAP_PLUS_BYTE;

				case TRAP_MINUS:
					return TRAP_MINUS_BYTE;

				case IDENTITY_CAMEL_NAME_ROLLED_1:
					return IDENTITY_CAMEL_NAME_ROLLED_BYTE[0];

				case IDENTITY_CAMEL_NAME_ROLLED_2:
					return IDENTITY_CAMEL_NAME_ROLLED_BYTE[1];

				case IDENTITY_CAMEL_NAME_ROLLED_3:
					return IDENTITY_CAMEL_NAME_ROLLED_BYTE[2];

				case IDENTITY_CAMEL_NAME_ROLLED_4:
					return IDENTITY_CAMEL_NAME_ROLLED_BYTE[3];

				case IDENTITY_CAMEL_NAME_ROLLED_5:
					return IDENTITY_CAMEL_NAME_ROLLED_BYTE[4];

				case IDENTITY_CAMEL_NAME_UNROLLED_1:
					return IDENTITY_CAMEL_NAME_UNROLLED_BYTE[0];

				case IDENTITY_CAMEL_NAME_UNROLLED_2:
					return IDENTITY_CAMEL_NAME_UNROLLED_BYTE[1];

				case IDENTITY_CAMEL_NAME_UNROLLED_3:
					return IDENTITY_CAMEL_NAME_UNROLLED_BYTE[2];

				case IDENTITY_CAMEL_NAME_UNROLLED_4:
					return IDENTITY_CAMEL_NAME_UNROLLED_BYTE[3];

				case IDENTITY_CAMEL_NAME_UNROLLED_5:
					return IDENTITY_CAMEL_NAME_UNROLLED_BYTE[4];

				case PATTERN_CAMEL_NAME_1:
					return PATTERN_CAMEL_NAME_1_BYTE;

				case PATTERN_CAMEL_NAME_2:
					return PATTERN_CAMEL_NAME_2_BYTE;

				case PATTERN_CAMEL_NAME_3:
					return PATTERN_CAMEL_NAME_3_BYTE;

				case PATTERN_CAMEL_NAME_4:
					return PATTERN_CAMEL_NAME_4_BYTE;

				case PATTERN_CAMEL_NAME_5:
					return PATTERN_CAMEL_NAME_5_BYTE;
				default:
					Assert.Fail("{0} n'a pas de byte", aChar);
					break;
			}

			return new byte();
		}

		public static string ByteToString(byte[] aBytes)
		{
			string retval = String.Empty;

			for (int i = 0; i < aBytes.Length; i++)
				retval += ByteToString(aBytes[i]);

			return retval;
		}

		public static char ByteToString(byte aByte)
		{
			switch (aByte)
			{
				case CASE_SEPARATOR_BYTE:
					return CASE_SEPARATOR;

				case TRAP_PLUS_BYTE:
					return TRAP_PLUS;

				case TRAP_MINUS_BYTE:
					return TRAP_MINUS;

				case IDENTITY_CAMEL_NAME_ROLLED_1_BYTE:
					return IDENTITY_CAMEL_NAME_ROLLED[0];

				case IDENTITY_CAMEL_NAME_ROLLED_2_BYTE:
					return IDENTITY_CAMEL_NAME_ROLLED[1];

				case IDENTITY_CAMEL_NAME_ROLLED_3_BYTE:
					return IDENTITY_CAMEL_NAME_ROLLED[2];

				case IDENTITY_CAMEL_NAME_ROLLED_4_BYTE:
					return IDENTITY_CAMEL_NAME_ROLLED[3];

				case IDENTITY_CAMEL_NAME_ROLLED_5_BYTE:
					return IDENTITY_CAMEL_NAME_ROLLED[4];

				case IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE:
					return IDENTITY_CAMEL_NAME_UNROLLED[0];

				case IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE:
					return IDENTITY_CAMEL_NAME_UNROLLED[1];

				case IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE:
					return IDENTITY_CAMEL_NAME_UNROLLED[2];

				case IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE:
					return IDENTITY_CAMEL_NAME_UNROLLED[3];

				case IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE:
					return IDENTITY_CAMEL_NAME_UNROLLED[4];

				default:
					Assert.Fail("{0} n'a pas de byte", aByte);
					break;
			}

			return new char();
		}

		public static byte ByteRollToUnroll(byte aByte)
		{
			switch (aByte)
			{
				case IDENTITY_CAMEL_NAME_ROLLED_1_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE;
				case IDENTITY_CAMEL_NAME_ROLLED_2_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE;
				case IDENTITY_CAMEL_NAME_ROLLED_3_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE;
				case IDENTITY_CAMEL_NAME_ROLLED_4_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE;
				case IDENTITY_CAMEL_NAME_ROLLED_5_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE: return IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE;
				default: Assert.Fail("{0} n'a pas de byte", aByte); break;
			}

			return new byte();
		}

		public static byte ByteUnrollToRoll(byte aByte)
		{
			switch (aByte)
			{
				case IDENTITY_CAMEL_NAME_UNROLLED_1_BYTE: return IDENTITY_CAMEL_NAME_ROLLED_1_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_2_BYTE: return IDENTITY_CAMEL_NAME_ROLLED_2_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_3_BYTE: return IDENTITY_CAMEL_NAME_ROLLED_3_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_4_BYTE: return IDENTITY_CAMEL_NAME_ROLLED_4_BYTE;
				case IDENTITY_CAMEL_NAME_UNROLLED_5_BYTE: return IDENTITY_CAMEL_NAME_ROLLED_5_BYTE;
				default: Assert.Fail("{0} n'a pas de byte", aByte); break;
			}

			return new byte();
		}

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
			if (string.IsNullOrWhiteSpace(aLog))
				return;
#if UsingUnity
        UnityEngine.Debug.Log(aLog);
#else
			Console.Write(aLog);
#endif
		}
	}
}