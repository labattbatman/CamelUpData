using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


//TODO MAIN
//Test les +
//Faire les - et tester

//Tester avec CamelUpUnityTest les traps: Va falloir faire 2e facon pour les minus traps
//Pour les positions des traps...repenser le calcul et attendre les traps. fait des tests.
//Long terme decision. Tester sur un bon ordi le temps
//Merge avec CamelUpUnity pour le visuel


namespace CamelUpData
{
	[TestClass]
    public class Program
    {
	    private static string UnityCamelUpResultFolder = "/UnityResult";
		private static string TextFileName = "/test.txt";
        private static DateTime m_StartingTime;
        private static Dictionary<string, List<Board>> m_BoardsByDiceOrder = new Dictionary<string, List<Board>>();
        private static List<Board> m_FinishBoard = new List<Board>();

        static void Main(string[] args)
        {
            EraseTextFile();

            m_StartingTime = DateTime.Now;
            GameRules.Log("this is a test \n");

			CustomTest(";O;B;W;Y;G;+;", TextFileName);
			//TestAnalyseBoard(test);

			string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
            GameRules.Log(log);
            GameRules.Log(m_FinishBoard.Count + "\n");

            GameRules.Log("test is over \n");
            Console.ReadKey();
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

        private static void PopulateFinishBoard(Board aBoard)
        {
            if (aBoard.IsCamelReachEnd || aBoard.IsAllCamelRolled)
                m_FinishBoard.Add(aBoard);

            for (int i = 0; i < aBoard.m_SubBoard.Count; i++)
                PopulateFinishBoard(aBoard.m_SubBoard[i]);
        }

        private static void CustomTest(string aBoard, string aFileName)
        {
	        m_BoardsByDiceOrder.Clear();
			Board board = new Board(aBoard);
            PopulateBoardByDiceOrder(board);
            
            var list = m_BoardsByDiceOrder.Keys.ToList();
            list.Sort();
            int diceNumberCount = m_BoardsByDiceOrder[list[0]].Count;
            List<string> log = new List<string>();
            //i = dice number 1-1-1-1-1
            //j = dice order  W-O-B-Y-G
            for (int i = 0; i < diceNumberCount; i++)
            {
                string newLog = string.Empty;
                for (int j = 0; j < list.Count; j++)
                {
                    if (m_BoardsByDiceOrder[list[j]].Count <= i)
                    {
                        continue;
                    }
                    newLog += m_BoardsByDiceOrder[list[j]][i].ToStringOldCamelUpFormat() + "";
                }

                log.Add(newLog);
            }

            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + aFileName, true);

	        for (int i = 0; i < log.Count; i++)
	        {
					tw.WriteLine(log[i].Remove(log[i].Length - 2));
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

            for (int i = 0; i < aBoard.m_SubBoard.Count; i++)
            {
                PopulateBoardByDiceOrder(aBoard.m_SubBoard[i]);
            }
        }

        private static void EraseTextFile()
        {
            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + TextFileName, false);
            tw.Write(String.Empty);
            tw.Close();
        }

        private static void TestAnalyseBoard(Board aBoard)
        {
            PopulateFinishBoard(aBoard);
            BoardAnalyzer boardAnal = new BoardAnalyzer(m_FinishBoard);
            boardAnal.SetCamelCard("B0O0W0Y0G0");
            GameRules.Log(boardAnal + "\n");
        }

	    [TestMethod]
	    public void TestBoards()
	    {
		    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + UnityCamelUpResultFolder);

			//PopulatePattern(";A;B;C;D;E;");

		    foreach (var file in files)
		    {
			    if (file.Contains("+"))
				    continue;

			    EraseTextFile();
				CustomTest(Path.GetFileNameWithoutExtension(file), TextFileName);

				string[] compared = File.ReadAllLines(file);
				string[] result = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + TextFileName);

				Assert.AreEqual(compared.Length, result.Length);
				
				for(int i = 0; i < compared.Length; i++)
					Assert.AreEqual(compared[i], result[i]);
			}	    
		}

	    [TestMethod]
	    public void TestBoardHardcoder()
	    {
		    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + UnityCamelUpResultFolder);

		    foreach (var file in files)
		    {
			    if (Path.GetFileNameWithoutExtension(file) != ";O;B;W;YG;+")
				    continue;

			    EraseTextFile();
			    CustomTest(Path.GetFileNameWithoutExtension(file), TextFileName);

			    string[] compared = File.ReadAllLines(file);
			    string[] result = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + TextFileName);

			    Assert.AreEqual(compared.Length, result.Length);

			    for (int i = 0; i < compared.Length; i++)
				    Assert.AreEqual(compared[i], result[i]);
		    }
	    }
	}
}
