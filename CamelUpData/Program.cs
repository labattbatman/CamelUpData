using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CamelUpData.Script;
using Microsoft.VisualStudio.TestTools.UnitTesting;


//TODO fait à moitié
//Test les + (ya un bug avec CamelUpUnity)
//Tester les -
//Pour les positions des traps...repenser le calcul et attendre les traps. fait des tests.

//TODO MAIN
//Long terme decision. Tester sur un bon ordi le temps
//Merge avec CamelUpUnity pour le visuel
//GameRules.IS_SHUTTLE_WHEN_HITTING_MINUS_TRAP. Je le fais????

namespace CamelUpData
{
	[TestClass]
    public class Program
    {
	    private static string UNITY_CAMELUP_RESULT_FOLDER = "/UnityResult";
		private static string TEXT_FILE_NAME = "/test.txt";
	    private static string BOARD_ANALYZER_FILE_NAME = "/analyzer.txt";
		private static DateTime m_StartingTime;
        private static Dictionary<string, List<BoardDebug>> m_BoardsByDiceOrder = new Dictionary<string, List<BoardDebug>>();
        private static List<BoardDebug> m_FinishBoard = new List<BoardDebug>();
	    private static List<BoardDebug> m_UnfinishBoardByMaxRound = new List<BoardDebug>();

		static void Main(string[] args)
        {
            EraseTextFile();
            m_StartingTime = DateTime.Now;

	        if (args.Length == 0)
	        {
		        //string testBoard = ";y;g;r;W;o;;";
				string testBoard = ";ORWYG;";
		        BoardManager.Instance.CreateBoard(testBoard);

				//UNITY_CallCamelUpExe(";YGWBO;;","B0O0W0Y0G0");
				string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
		        
				GameRules.Log(log);
				Console.ReadLine();
			}
	        else
	        {
				//Caller par commandLine
	        }
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

        public static void PopulateFinishBoard(BoardDebug aBoard)
        {
			if (aBoard.IsCamelReachEnd || aBoard.IsAllCamelRolled)
                m_FinishBoard.Add(aBoard);

	        foreach (BoardDebug board in aBoard.m_SubBoard)
		        PopulateFinishBoard(board);
        }

	    public static void PopulateUnfinishBoardbyMaxRound(BoardDebug aBoard)
	    {
			m_UnfinishBoardByMaxRound.Add(aBoard);

		}

        private static void CustomTest(string aBoard, string aFileName)
        {
	        m_BoardsByDiceOrder.Clear();
			BoardDebug board = new BoardDebug(aBoard);
	        PopulateFinishBoard(board);
			
			PopulateBoardByDiceOrder(board);

	        List<string> list = m_BoardsByDiceOrder.Keys.ToList();
            list.Sort();
            int diceNumberCount = m_BoardsByDiceOrder[list[0]].Count;
            List<string> log = new List<string>();
            //i = dice number 1-1-1-1-1
            //j = dice order  W-O-B-Y-G
            for (int i = 0; i < diceNumberCount; i++)
            {
                string newLog = string.Empty;
                foreach (string b in list)
                {
	                if (m_BoardsByDiceOrder[b].Count <= i)
	                {
		                continue;
	                }
	                newLog += m_BoardsByDiceOrder[b][i].ToStringOldCamelUpFormat() + "";
                }

                log.Add(newLog);
            }

            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + aFileName, true);

	        foreach (string l in log)
	        {
		        tw.WriteLine(l.Remove(l.Length - 2));
	        }

            tw.Close();
        }

        private static void PopulateBoardByDiceOrder(BoardDebug aBoard)
        {
	        if (aBoard.GetUnrolledCamelByRank().Length == 0)
            {
                if(!m_BoardsByDiceOrder.ContainsKey(aBoard.DicesHistory))
                {
                    m_BoardsByDiceOrder.Add(aBoard.DicesHistory, new List<BoardDebug>());
                }

                m_BoardsByDiceOrder[aBoard.DicesHistory].Add(aBoard);
            }

	        foreach (BoardDebug sub in aBoard.m_SubBoard)
	        {
		        PopulateBoardByDiceOrder(sub);
	        }
        }

        private static void EraseTextFile()
        {
            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + TEXT_FILE_NAME, false);
            tw.Write(String.Empty);
            tw.Close();
        }

		[TestMethod]
	    public void TestBoards()
		{
			GameRules.USE_DICE_NB_IN_DICE_HSITORY = false;

			string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + UNITY_CAMELUP_RESULT_FOLDER);

		    foreach (string file in files)
		    {
			    if (file.Contains("+"))
				    continue;

			    EraseTextFile();
				CustomTest(Path.GetFileNameWithoutExtension(file), TEXT_FILE_NAME);

				string[] compared = File.ReadAllLines(file);
				string[] result = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + TEXT_FILE_NAME);

				Assert.AreEqual(compared.Length, result.Length, string.Format("Fail at {0}", Path.GetFileNameWithoutExtension(file)));
				
				for(int i = 0; i < compared.Length; i++)
					Assert.AreEqual(compared[i], result[i], string.Format("Fail at {0}", Path.GetFileNameWithoutExtension(file)));
			}	    
		}
	}
}
