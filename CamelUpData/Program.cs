using System;
using System.Diagnostics;
using System.IO;
using CamelUpData.Script;

//TODO fait à moitié
//Test les + (ya un bug avec CamelUpUnity)
//Tester les -
//Pour les positions des traps...repenser le calcul et attendre les traps. fait des tests.

//TODO MAIN
//Long terme decision. Tester sur un bon ordi le temps //Rajouter aux testes
//Merge avec CamelUpUnity pour le visuel
//GameRules.IS_SHUTTLE_WHEN_HITTING_MINUS_TRAP. Je le fais????

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
		        //GameRules.USE_DICE_NB_IN_DICE_HSITORY = false;
				//string testBoard = ";y;g;r;W;o;;";
				string testBoard = ";OBWYG;";
		        //string testBoard = ";O;;B;;W;;Y;;G;";

				BoardManager.Instance.CreateBoardDebug(testBoard);
		        BoardManager.Instance.AnalyseBoards("B0O0W0Y0G0");

				//UNITY_CallCamelUpExe(";YGWBO;;","B0O0W0Y0G0");
				string log = string.Format("{0}\n", (DateTime.Now - m_StartingTime).TotalSeconds);
		        
				GameRules.Log(log);
				Console.ReadLine();
			}
	        else
	        {
		        BoardManager.Instance.CreateBoard(args[0]);
		        BoardManager.Instance.AnalyseBoards(args[1]);
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
	}
}
