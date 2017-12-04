using System;
using System.Diagnostics;
using System.IO;
using CamelUpData.Script;

namespace CamelUpData
{
    public class Program
    {
		private static DateTime m_StartingTime;
        
		static void Main(string[] args)
        {
            m_StartingTime = DateTime.Now;

	        if (args.Length == 0)
	        {
				TestLTGuesser();
		        //TestLT();
				//TestBoardManager();
				//UNITY_CallCamelUpExe(";YGWBO;;","B0O0W0Y0G0");
				string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
				GameRules.Log(log);
				Console.ReadLine();
			}
	        else
	        {
				BoardManager bm = new BoardManager(5);
		        bm.CreateBoard(";OBWYG;");

		        BoardAnalyzer ba = new BoardAnalyzer(bm.GetAllBoards(), "B0O0W0Y0G0");
			}
        }

	    private static void TestLTGuesser()
	    {
		    var guesser = new LongTermCardGuesser();
		    guesser.AddFirstCamelCard(";;;;;OB;;;W;;;Y;;G;");
	    }


		private static void TestLT()
	    {
			string testBoard = ";;;;;;;;;;;;;O;B;WY;;G;";
		    //ring testBoard = ";;;;;;;;;;;;;;O;B;WY;;G;";

			var ltbm = new LongTermBoardAnalyser(new Board(testBoard), null);
		    var actual = ltbm.GetAverageCamelRankInfo();

		}

		private static void TestBoardManager()
	    {
			//string testBoard = ";y;g;r;W;o;;";
		    string testBoard = ";OBWYG;";
		    //string testBoard = ";O;;B;;W;;Y;;G;";
		    BoardManager bm = new BoardManager(5);
		    bm.CreateBoardByte(testBoard);

		    BoardAnalyzer ba = new BoardAnalyzer(bm.GetAllBoards(), "B0O0W0Y0G0");
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
