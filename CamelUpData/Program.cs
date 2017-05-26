using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


//TODO MAIN
//Tester avec CamelUpUnityTest les traps: Va falloir faire 2e facon pour les minus traps
//Pour les positions des traps...repenser le calcul et attendre les traps. fait des tests.
//Long terme decision. Tester sur un bon ordi le temps
//Merge avec CamelUpUnity pour le visuel


namespace CamelUpData
{
    class Program
    {
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

            PopulatePattern();           

            Board test = new Board(";OBWG");
            //Board test = new Board(";GW");

            //TestMultipleBoard();
            //CustomTest();
            //TestAnalyseBoard(test);


            string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
            GameRules.Log(log);
            GameRules.Log(m_FinishBoard.Count + "\n");

            GameRules.Log("test is over \n");
            Console.ReadKey();
        }

        public static void HardPopulateFinishBoard(Board aBoard)
        {
            if (aBoard.IsCamelReachEnd)
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

        private static void PopulatePattern()
        {
            if(SaveManager.Instance.IsPatternSaved)
            {
                GameRules.Patterns = SaveManager.Instance.Load();
                return;
            }

            TestPopulatePattern();
        }

        private static void TestPopulatePattern()
        {
            PatternGenerator m_PatternGenerator = new PatternGenerator();
            m_PatternGenerator.Init(";ABCDE");
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

        private static void CustomTest()
        {
            //TODO ";YBGW;;;;;O;" manque des boards 28674 => 29160
            // ";YBG;w;;;;O;" a jusque 9 subboard => 12
            //string test = ";YBG;w;;;;O;";
            string test = ";YBGWO;;;;;";

            Board board = new Board(test);
            PopulateBoardByDiceOrder(board);

            var list = m_BoardsByDiceOrder.Keys.ToList();
            list.Sort();
            int diceNumberCount = m_BoardsByDiceOrder[list[0]].Count;
            List<string> log = new List<string>();
            //i = dice number 1-1-1-1-1
            //j = dice order  W-O-B-Y-G
            HashSet<string> wrongDiceOrder = new HashSet<string>();
            for (int i = 0; i < diceNumberCount; i++)
            {
                string newLog = string.Empty;
                for (int j = 0; j < list.Count; j++)
                {
                    if (m_BoardsByDiceOrder[list[j]].Count <= i)
                    {
                        List<Board> ttest = m_BoardsByDiceOrder[list[j]];
                        wrongDiceOrder.Add(list[j]);
                        continue;
                    }
                    newLog += m_BoardsByDiceOrder[list[j]][i].ToStringOldCamelUpFormat() + "";
                }

                log.Add(newLog);
            }

            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + TextFileName, true);

            for(int i = 0; i < log.Count; i++)
                tw.WriteLine(log[i] + "\t");

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
    }
}
