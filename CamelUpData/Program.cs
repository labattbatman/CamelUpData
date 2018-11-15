using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CamelUpData.Script;

namespace CamelUpData
{
	public class Program
	{
		//LOOP infinie dans GenerateLongTermCardEv() BoardAnalyzer.cs L97

		private static DateTime m_StartingTime;

		static void Main(string[] args)
		{
			m_StartingTime = DateTime.Now;
			args = GetBoardStatus();
			GameRules.Log(string.Format("Boards: {0} \nCards: {1} \n\n", args[0], args[1]));

			BoardManager bm = new BoardManager(5);
			bm.CreateBoard(args[0]);

			BoardAnalyzer ba = new BoardAnalyzer(args[0], bm.GetAllBoards(), args[1]);
			GameRules.Log(ba.ToString());

			ShowExecutionTimeLog();
			//TestLTGuesser();
			//TestLT();
			//TestBoardManager();
			//UNITY_CallCamelUpExe(";YGWBO;;","B0O0W0Y0G0");
		}

		private static string[] GetBoardStatus()
		{
			var status = new string[2];
			var dict = ReadBoardFile();

			status[0] = GetRaceStatus(dict["Race"]);
			status[1] = GetCardsStatus(dict["Cards"]);

			return status;
		}

		private static string GetRaceStatus(string arg)
		{
			string race = string.Empty;
			var cases = arg.Split(' ');
			int lastCase = 0;

			foreach (var ca in cases)
			{
				var caseSeparatorNeeded = Convert.ToInt32(Regex.Match(ca, @"\d+").Value);
				for (int i = lastCase; i < caseSeparatorNeeded; i++)
					race += GameRules.CASE_SEPARATOR;

				lastCase = caseSeparatorNeeded;
				race += Regex.Replace(ca, @"[\d]", string.Empty);
			}
			return race;
		}

		private static string GetCardsStatus(string arg)
		{
			var status = string.Empty;
			var camels = arg.Split(' ');

			foreach (var camel in camels)
			{
				if (string.IsNullOrEmpty(camel))
					continue;

				var converted = string.Empty;
				switch (camel[1])
				{
					case '5': converted = "0"; break;
					case '3': converted = "1"; break;
					case '2': converted = "2"; break;
					case '0': converted = "3"; break;
					default: throw new Exception("Cards Status in arguments is invalid: " + camel[1]);
				}

				status += camel[0] + converted;
			}

			return status;
		}

		private static Dictionary<string, string> ReadBoardFile()
		{
			var patch = Directory.GetCurrentDirectory() + "/BoardStatusArgs.txt";
			var dict = new Dictionary<string, string>();

			string line;
			StreamReader file = new StreamReader(patch);
			while ((line = file.ReadLine()) != null)
			{
				if (!string.IsNullOrEmpty(line))
				{
					var splitted = line.Split(':');
					dict.Add(splitted[0], splitted[1]);
				}
			}
			file.Close();

			return dict;
		}

		private static void ShowExecutionTimeLog()
		{
			string log = string.Format("\nTemps d'exexution: {0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
			GameRules.Log(log);
			Console.ReadLine();
		}

		//Tests
		private static void TestLTGuesser()
		{
			var guesser = new LongTermCardGuesser();
			guesser.AddFirstCamelCard(";;;;;OB;;;W;;;Y;;G;");
		}

		private static void TestLT()
		{
			string testBoard = ";;;;;;;;;;;;;;;O;B;WY;;G;";
			//ring testBoard = ";;;;;;;;;;;;;;O;B;WY;;G;";

			var ltbm = new LongTermBoardAnalyser(new Board(testBoard), null);
			var actual = ltbm.GetEv();

		}

		private static void TestBoardManager()
		{
			//string testBoard = ";y;g;r;W;o;;";
			//string testBoard = ";OBWYG;";
			string testBoard = ";;O;B;WY;;;;G;";
			BoardManager bm = new BoardManager(1);
			bm.CreateBoardDebug(testBoard);

			BoardAnalyzer ba = new BoardAnalyzer(testBoard, bm.GetAllBoards(), "B0O0W0Y0G0");
			GameRules.Log(ba.ToString());
		}

		private static void UNITY_CallCamelUpExe(string aBoard, string aCards)
		{
			Process process = new Process();
			try
			{
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.EnableRaisingEvents = true;
				process.StartInfo.RedirectStandardOutput = true;

				process.StartInfo.FileName = Directory.GetCurrentDirectory() + "/CamelUpData.exe";
				process.StartInfo.Arguments = aBoard + " " + aCards;

				process.Start();

				string result = process.StandardOutput.ReadToEnd();

				GameRules.Log(string.Format("CamelEXE Result: {0}", result));
				process.Close();
			}
			catch (Exception ex)
			{
				GameRules.Log(string.Format("CamelEXE Exception: {0}", ex.Message));
				process.Close();
			}
		}
	}
}
