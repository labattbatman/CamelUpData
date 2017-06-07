using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


//TODO MAIN
//faire des test avec camelUp Unity pour les +. (il y a un bug avec camelupunity avec les +)
//Faire les - et tester
//Peut etre faire des tests automatic ca bug et je ne sias pas pkoi
//AutoPopulate si ya l'a pas
//Tester avec CamelUpUnityTest les traps: Va falloir faire 2e facon pour les minus traps
//Pour les positions des traps...repenser le calcul et attendre les traps. fait des tests.
//Long terme decision. Tester sur un bon ordi le temps
//Merge avec CamelUpUnity pour le visuel


namespace CamelUpData
{
    class Program
    {
	    private static string UnityCamelUpResultFolder = "/UnityResult";
		private static string TextFileName = "/test.txt";
        private static DateTime m_StartingTime;
        private static Dictionary<string, List<Board>> m_BoardsByDiceOrder = new Dictionary<string, List<Board>>();
        private static List<Board> m_FinishBoard = new List<Board>();

        private static int m_ToDelete = 0;

        static void Main(string[] args)
        {
            EraseTextFile();

            m_StartingTime = DateTime.Now;
            GameRules.Log("this is a test \n");

			PopulatePattern(";A;B;C;D;E;");

			//Board test = new Board(";OBW;;");
			//Board test = new Board(";GW");
			//Board test = new Board(";OBWGY");

			//TestMultipleBoard();
			CustomTest(";O;B;W;YG;", TextFileName);	                      
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

        private static void PopulatePattern(string aBoard)
        {
           /* if(SaveManager.Instance.IsPatternSaved)
            {
                GameRules.Patterns = SaveManager.Instance.Load();
                return;
            }*/

            TestPopulatePattern(aBoard);
        }

        private static void TestPopulatePattern(string aBoard)
        {
            PatternGenerator m_PatternGenerator = new PatternGenerator();
            m_PatternGenerator.Init(aBoard);
            bool isGeneratingPattern = true;
            while (isGeneratingPattern)
            {
                if (m_PatternGenerator.RemainingPatternsToDiscover > 0)
                {
                    m_PatternGenerator.Update();
                }
                else
                {
                    isGeneratingPattern = false;
                    m_PatternGenerator.SaveLastPatterns();
                    GameRules.Patterns = m_PatternGenerator.m_Patterns;
                }
            }
        }

        private static void TestMultipleBoard()
        {
            List<string> test = new List<string>();
          
            test.Add(";YGWBO");
            /*test.Add(";;;;;;GWOBY");
            test.Add(";G;W;O;B;Y");
            
            test.Add(";GWOB;;;;;;;;;;;;;;Y");
            test.Add(";GWO;;;;;;;;;;;;;;BY");
            test.Add(";GW;;;;;;;;;;;;;;OBY");
            test.Add(";G;;;;;;;;;;;;;;WOBY");
            
            
            test.Add(";GWO;;;;;;;;;;B;;;;Y");
            test.Add(";GW;;;;;;;;;;BO;;;;;;;Y");
            test.Add(";GW;;;;;;;O;;;;BY");
            test.Add(";G;;;;WOB;;;;;;;;;;;;;;;;;;;;;;Y");
            test.Add(";G;;;;WO;;;;;;;;;;;;;;;;;;;;;;BY");
            test.Add(";G;;;;W;;;;;;;;;;;OBY");
            
            test.Add(";GW;;;;;;;O;;;;B;;;;Y");
            test.Add(";G;;;;;;WO;;;;;;;B;;;;Y");
            test.Add(";G;;;;;;O;;;;;;WB;;;;;;;;Y");
            test.Add(";G;;;;;;O;;;;B;;;;;;WY");

            test.Add(";G;;;;W;;;;O;;;;B;;;;Y");
            /**/
            
            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + TextFileName, true);
            for (int i = 0; i < test.Count; i++)
            {
                Board aBoard = new Board(test[i]);
                tw.WriteLine(i + "\n" + aBoard.ToStringOldCamelUpFormat() + "\n" + "-------------------");
                //GameRules.Log(i + "\n" + aBoard.ToString() + "\n" + "-------------------");
            }
            tw.Close();
        }

        private static void CustomTest(string aBoard, string aName)
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

            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + aName, true);

	        for (int i = 0; i < log.Count; i++)
	        {
					tw.WriteLine(log[i] + "\t");
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

                m_ToDelete++;


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
            GameRules.Log(boardAnal.ToString() + "\n");
        }
	
	    private static void TestBoardWithUnityCamelUp()
	    {
		    //Ne Marche pas ???? je ne sias pas pkoi
			string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + UnityCamelUpResultFolder);
		    string filePostFix = "MYRESULT";
			
			for (int i = 0; i < 1/*files.Length*/; i++)
			{
				if (files[i].Contains(filePostFix) || files[i].Contains(TextFileName))
					continue;
				m_BoardsByDiceOrder.Clear();

				string[] tempString = files[i].Split('/');
			    string board = tempString[tempString.Length - 1];
			    board = board.Remove(0,UnityCamelUpResultFolder.Length);
			    board = board.Substring(0, board.Length - 4);

			    CustomTest(board, UnityCamelUpResultFolder + "/" + board + filePostFix + ".txt");
		    }			
	    }
    }
}
