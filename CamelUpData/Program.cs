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
//Faire Roll dice decision
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
        private static Dictionary<string, List<Board>> m_BoardsByDiceOrder = new Dictionary<string, List<Board>>();
        private static List<Board> m_FinishBoard = new List<Board>();
	    private static List<Board> m_UnfinishBoardByMaxRound = new List<Board>();

		static void Main(string[] args)
        {
            EraseTextFile();
            m_StartingTime = DateTime.Now;

	        if (args.Length == 0)
	        {
		        string testBoard = ";Y;G;B;w;o;;";
				BoardManager.Instance.CreateBoard(testBoard);


				//ComparaisonPourBoardManager();
				//TestAnalyseBoard(new Board(";ygwBO;;"), "B0O0W0Y0G0");
				//UNITY_CallCamelUpExe(";YGWBO;;","B0O0W0Y0G0");
				string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
		        
				GameRules.Log(log);
				Console.ReadLine();
			}
	        else
	        {
				TestAnalyseBoard(new Board(args[0]), args[1]);
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

        public static void PopulateFinishBoard(Board aBoard)
        {
	        //if (m_FinishBoard.Count % 7000 == 0)
		    //    GameRules.Log(m_FinishBoard.Count.ToString());

			if (aBoard.IsCamelReachEnd || aBoard.IsAllCamelRolled)
                m_FinishBoard.Add(aBoard);

	        foreach (Board board in aBoard.m_SubBoard)
		        PopulateFinishBoard(board);
        }

	    public static void PopulateUnfinishBoardbyMaxRound(Board aBoard)
	    {
			m_UnfinishBoardByMaxRound.Add(aBoard);

		}

        private static void CustomTest(string aBoard, string aFileName)
        {
	        m_BoardsByDiceOrder.Clear();
			Board board = new Board(aBoard);
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

        private static void PopulateBoardByDiceOrder(Board aBoard)
        {
	        if (aBoard.GetUnrolledCamelByRank().Length == 0)
            {
                if(!m_BoardsByDiceOrder.ContainsKey(aBoard.DicesHistory))
                {
                    m_BoardsByDiceOrder.Add(aBoard.DicesHistory, new List<Board>());
                }

                m_BoardsByDiceOrder[aBoard.DicesHistory].Add(aBoard);
            }

	        foreach (Board sub in aBoard.m_SubBoard)
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

        private static void TestAnalyseBoard(Board aBoard, string aCards)
        {
            BoardAnalyzer boardAnal = new BoardAnalyzer(aBoard, aCards);
            GameRules.Log(boardAnal + "\n");

	        /*TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + BOARD_ANALYZER_FILE_NAME, false);
	        tw.Write(boardAnal.ToStringLong());
	        tw.Close();*/
		}

	    public static void HardPopulateFinishBoard(Board aBoard)
        {
            //if (aBoard.IsCamelReachEnd)
            {		
                m_FinishBoard.Add(aBoard);

	                if (m_FinishBoard.Count % 100000 == 0)
	                   GameRules.Log(m_FinishBoard.Count + "\n");
            }
        }

	    private static void ComparaisonPourBoardManager()
	    {
		    string testBoard = ";YGBWO;;";

			GameRules.USE_DICE_NB_IN_DICE_HSITORY = false;
		    TestAnalyseBoard(new Board(testBoard), "B0O0W0Y0G0");

		    GameRules.USE_DICE_NB_IN_DICE_HSITORY = true;
		    BoardManager.Instance.CreateBoard(testBoard);

		    var groupBy = m_UnfinishBoardByMaxRound.GroupBy(c => c.BoardState).ToList();
		    var dict = new Dictionary<string, int>();
		    var total = m_UnfinishBoardByMaxRound.Count;
		    var dict2 = new Dictionary<string, int>();
		    var total2 = 0;

		    var diceHistoryForCustom = new Dictionary<string, List<string>>();

		    foreach (var all in groupBy)
		    {
			    var diceHistory = new List<string>();
			    dict.Add(all.Key, all.Count());

			    foreach (var board in all)
				    diceHistory.Add(board.DicesHistory);

			    diceHistoryForCustom.Add(all.Key, diceHistory);
		    }

		    foreach (var board in BoardManager.Instance.m_UnfinishBoardByMaxRound)
		    {
			    dict2.Add(board.Key, board.Value.Weight);
			    total2 += board.Value.Weight;
		    }
		}

		[TestMethod]
	    public void TestBoards()
		{
			GameRules.USE_DICE_NB_IN_DICE_HSITORY = false;

			string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + UNITY_CAMELUP_RESULT_FOLDER);

			//PopulatePattern(";A;B;C;D;E;");

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

	    [TestMethod]
	    public void TestBoardHardcoder()
	    {
		    GameRules.USE_DICE_NB_IN_DICE_HSITORY = false;

			string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + UNITY_CAMELUP_RESULT_FOLDER);

		    foreach (string file in files)
		    {
			    if (Path.GetFileNameWithoutExtension(file) != ";O;B;W;YG;+")
				    continue;

			    EraseTextFile();
			    CustomTest(Path.GetFileNameWithoutExtension(file), TEXT_FILE_NAME);

			    string[] compared = File.ReadAllLines(file);
			    string[] result = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + TEXT_FILE_NAME);

			    Assert.AreEqual(compared.Length, result.Length);

			    for (int i = 0; i < compared.Length; i++)
				    Assert.AreEqual(compared[i], result[i]);
		    }
	    }
	}
}
