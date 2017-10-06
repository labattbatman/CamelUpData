using System.Collections.Generic;
using System.IO;

namespace CamelUpData.Script
{
	public class SaveManager : MonoSingleton<SaveManager>
	{
		private string CompletePath
		{
			get
			{
				string m_FileName = "/PatternDict.txt";
#if UsingUnity
            return Application.dataPath + m_FileName;
#else
				return Directory.GetCurrentDirectory() + m_FileName;
#endif
			}
		}

		public bool IsPatternSaved { get { return File.Exists(CompletePath); } }

		public void Save(Dictionary<string, Pattern> aPatterns, bool aOverrideSave)
		{
			Save(new List<Pattern>(aPatterns.Values), aOverrideSave);
		}

		public void Save(List<Pattern> aPatterns, bool aAppend)
		{
			string save = string.Empty;
			foreach (var pattern in aPatterns)
			{
				save += pattern.ToString() + '\n';
			}

			string path = CompletePath;
			GameRules.Log(path);
			TextWriter tw = new StreamWriter(path, aAppend);
			tw.WriteLine(save);
			tw.Close();
		}

		public Dictionary<string, Pattern> Load()
		{
			Dictionary<string, Pattern> retval = new Dictionary<string, Pattern>();

			string line;
			StreamReader file = new StreamReader(CompletePath);
			while ((line = file.ReadLine()) != null)
			{
				if (!string.IsNullOrEmpty(line))
				{
					Pattern newPattern = new Pattern(line);
					retval.Add(newPattern.Id, newPattern);
				}
			}
			file.Close();
			return retval;
		}
	}
}
