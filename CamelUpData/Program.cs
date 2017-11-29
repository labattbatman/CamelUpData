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
				//TestBoardManager();
				//UNITY_CallCamelUpExe(";YGWBO;;","B0O0W0Y0G0");
				string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
				GameRules.Log(log);
				Console.ReadLine();
			}
	        else
	        {
				BoardManager bm = new BoardManager(5);
		        bm.CreateBoard(";OBWYG;", true);

		        BoardAnalyzer ba = new BoardAnalyzer(bm.GetAllBoards(), "B0O0W0Y0G0");
			}
        }

		private static void TestBoardManager()
	    {
			//string testBoard = ";y;g;r;W;o;;";
		    string testBoard = ";OBWYG;";
		    //string testBoard = ";O;;B;;W;;Y;;G;";
		    BoardManager bm = new BoardManager(5);
		    bm.CreateBoard(testBoard, true);

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
