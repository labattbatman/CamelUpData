using System;
using System.Collections.Generic;

namespace CamelUpData.Script
{
	public class Pattern
	{
		public string Id { get; private set; }
		public string CamelsIdentity { get; private set; }

		private Dictionary<char, Dictionary<int, string>> m_Results { get; set; }
		private List<string> m_ResultInString;
		private int m_NbCamelInPattern = 0;
    
		public List<string> ResultsInList
		{
			get
			{
				if (m_ResultInString == null || m_ResultInString.Count == 0)
				{
					m_ResultInString = new List<string>();
					foreach (var subDict in m_Results)
					{
						foreach (var result in subDict.Value)
						{
							m_ResultInString.Add(result.Value);
						}
					}
				}
				return m_ResultInString;
			}
		}

		public int NbCamel
		{
			get
			{           
				if(m_NbCamelInPattern <= 0)
				{
					foreach (char t in Id)
						if (GameRules.IsCharPatternCamel(t))
							m_NbCamelInPattern++;
				}

				return m_NbCamelInPattern;
			}
		}

		public Pattern(string aPatternData)
		{
			string[] data = aPatternData.Split(GameRules.PATTERN_SAVE_ID_RESULT_SEPERATOR);
			string[] results = data[1].Split(GameRules.PATTERN_RESULT_SEPARATOR);
			List<string> resultsInList = new List<string>();

			foreach (string t in results)
				resultsInList.Add(t);

			Id = data[0];
			PopulateDict(resultsInList);
		}

		public Pattern(string aPattern, List<string> aResult)
		{
			Id = aPattern;
			PopulateDict(aResult);
		}

		public Pattern(Pattern aPattern, string aCamelIdentity)
		{
			Id = aPattern.Id;
			CamelsIdentity = aCamelIdentity;

			m_Results = aPattern.m_Results;
			m_ResultInString = aPattern.m_ResultInString;
			m_NbCamelInPattern = aPattern.m_NbCamelInPattern;
		}

		public Pattern(Pattern aPattern, byte[] aCamelIdentity) : this(aPattern, GameRules.ByteToString(aCamelIdentity)){ }

		private void PopulateDict(List<string> aResult)
		{
			m_Results = new Dictionary<char, Dictionary<int, string>>();

			for (int i = 0; i < aResult.Count; i++)
			{
				if (string.IsNullOrEmpty(aResult[i]))
					continue;
				string[] subResult = aResult[i].Split(GameRules.PATTERN_RESULT_NAME_SEPARATOR);
				//TODO info hardcoder suite
				if (!m_Results.ContainsKey(subResult[0][0]))
				{
					Dictionary<int, string> subDict = new Dictionary<int, string>();
					subDict.Add((int)Char.GetNumericValue(subResult[0][1]), subResult[1]);
					m_Results.Add(subResult[0][0], subDict);
				}
				else
				{
					m_Results[subResult[0][0]].Add((int)Char.GetNumericValue(subResult[0][1]), subResult[1]);
				}
			}
		}

		private char CamelIdentityToPattern(char aIdentity)
		{
			int idIndex = CamelsIdentity.ToUpper().IndexOf(char.ToUpper(aIdentity));
			int patternIndex = 0;

			foreach (char t in Id)
			{
				if(GameRules.IsCharPatternCamel(t))
				{
					if(idIndex == patternIndex)
						return t;

					patternIndex++;
				}
			}

			return '0';
		}

		public override string ToString()
		{
			string retval = Id + GameRules.PATTERN_SAVE_ID_RESULT_SEPERATOR;

			foreach (var result in m_Results)
			{
				foreach (var detail in result.Value)
				{
					retval += result.Key + detail.Key + GameRules.PATTERN_RESULT_NAME_SEPARATOR + detail.Value + GameRules.PATTERN_RESULT_SEPARATOR;
				}
			}
			return retval;
		}  

		public List<string> GetResultsForDice(char aDice)
		{
			List<string> retval = new List<string>();
			char patternCamel = CamelIdentityToPattern(aDice);

			foreach (var result in m_Results[patternCamel])
			{
				retval.Add(result.Value);
			}

			return retval;
		}
	}
}
