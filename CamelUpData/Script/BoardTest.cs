using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CamelUpData.Script
{
	[TestClass]
	public class BoardTest
	{
		private readonly string UNITY_CAMELUP_RESULT_FOLDER = "/FichiersValidateur";
		private readonly string TEXT_FILE_NAME = "/test.txt";
		private readonly string BOARD_ANALYZER_FILE_NAME = "/analyzer.txt";

		private readonly Dictionary<string, List<BoardDebug>> m_BoardsByDiceOrder = new Dictionary<string, List<BoardDebug>>();
		private readonly List<BoardDebug> m_FinishBoard = new List<BoardDebug>();

		private readonly int m_DiceHistoryLenght = 10;

		[TestMethod]
		public void TestGameRules()
		{
			Assert.AreEqual(GameRules.DICE_NB_FACES, 3);
			Assert.AreEqual(GameRules.CASE_NUMBER, 20);

			Assert.AreEqual(GameRules.SHORT_TERM_FIRST_PRICE[0], 5);
			Assert.AreEqual(GameRules.SHORT_TERM_FIRST_PRICE[1], 3);
			Assert.AreEqual(GameRules.SHORT_TERM_FIRST_PRICE[2], 2);

			Assert.AreEqual(GameRules.SHORT_TERM_SECOND_PRICE, 1);
			Assert.AreEqual(GameRules.SHORT_TERM_LAST_PRICE, -1);
			Assert.AreEqual(GameRules.TRAP_REWARD, 1);
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

				string[] expected = File.ReadAllLines(file);
				string[] result = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + TEXT_FILE_NAME);

				Assert.AreEqual(expected.Length, result.Length, string.Format("Fail at {0}. {1}", Path.GetFileNameWithoutExtension(file), TEXT_FILE_NAME));

				for (int i = 0; i < expected.Length; i++)
					Assert.AreEqual(expected[i], result[i], string.Format("Fail at {0}", Path.GetFileNameWithoutExtension(file)));
			}
		}

		[TestMethod]
		public void TestBoardAnalyser()
		{
			BoardManager bm = new BoardManager(5);
			bm.CreateBoardDebug(";OBWYG;", true);
			AssertBoardAnalyzer(bm.GetAllBoards());

			bm = new BoardManager(5);
			bm.CreateBoard(";OBWYG;", true);
			AssertBoardAnalyzer(bm.GetAllBoards());

			//TODO extremement long(jamais fini :S) car BoardDebug.DiceHistory ne contient pas le chffre roulé
			/*
			bm = new BoardManager();
			bm.CreateBoardByte(";OBWYG;");
			AssertBoardAnalyzer(bm.GetAllBoards());
			*/
		}

		[TestMethod]
		public void TestLongTermBoardAnalyzerRank()
		{
			string testBoard = ";;;;;;;;;;;;;;O;B;WY;;G;";

			var ltbm = new LongTermBoardAnalyser(new Board(testBoard), null);
			var actual = ltbm.GetAverageCamelRankInfo();

			var bmm = new BoardManager(10);
			bmm.CreateBoard(testBoard, true);
			var rankCamels = new CamelRankManager(bmm.GetAllBoards()).GetCamelRanks;

			var expected = new Dictionary<char, double[]>();

			foreach (var rankCamel in rankCamels)
			{
				var rank = new double[5];

				for (int i = 0; i < 5; i++)
				{
					rank[i] = (double)rankCamel.TimeFinish(i) / rankCamel.m_TotalFinish;
				}

				expected.Add(rankCamel.CamelName, rank);
			}

			for(int i = 0; i < 5; i++)
			{
				var total = 0.0;

				foreach (var exp in expected)
					total += exp.Value[i];

				Assert.IsTrue(Math.Abs(1 - total) < 0.01, i.ToString());
			}

			foreach (var exp in expected)
			{
				for (int i = 0; i < exp.Value.Length; i++)
					Assert.IsTrue(Math.Abs(exp.Value[i] - actual[exp.Key][i]) < 0.001, String.Format("Camel {0}, position {1}", exp.Key, i));
			}
		}

		private void AssertBoardAnalyzer(List<IBoard> aBoards)
		{
			BoardAnalyzer actual = new BoardAnalyzer(aBoards, "B0O0W0Y0G0");
			List<Ev> actualEvs = actual.GetSortedtEvs();

			Assert.AreEqual(actual.m_TotalSubBoardWithWeight, 29160);
			AssertEv(actualEvs[0], GameRules.PlayerAction.PickShortTermCard, 1.37f, "Green");
			AssertEv(actualEvs[1], GameRules.PlayerAction.PutTrap, 0.81f, "Case(s): 4, . Minus Trap. Pas EV exacte.");
			AssertEv(actualEvs[2], GameRules.PlayerAction.RollDice, -0.19f, null);
			//AssertEv(actualEvs[3], GameRules.PlayerAction.PickLongTermCard, 1.37f, "Green");
		}

		private void AssertEv(Ev aEv, GameRules.PlayerAction aAction, float a2DecimalEv, object aInfo)
		{
			var evDiff = Math.Abs(Math.Round(aEv.m_Ev, 2) - a2DecimalEv);

			Assert.AreEqual(aEv.m_PlayerAction, aAction);
			Assert.IsTrue(evDiff < 0.01);
			Assert.AreEqual(aEv.m_Info, aInfo);
		}

		private void CustomTest(string aBoard, string aFileName)
		{
			m_BoardsByDiceOrder.Clear();
			BoardDebug board = new BoardDebug(aBoard);
			PopulateFinishBoard(board);

			PopulateBoardByDiceOrder(board);

			List<string> list = m_BoardsByDiceOrder.Keys.ToList();
			list.Sort();
			int diceNumberCount = m_BoardsByDiceOrder[list[0]].Count;
			List<string> logs = new List<string>();
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
					newLog += m_BoardsByDiceOrder[b][i].ToStringOldCamelUpFormat();
				}

				logs.Add(newLog);
			}

			TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + aFileName, true);

			foreach (string log in logs)
			{
				
				tw.WriteLine(log.Remove(log.Length - 2));
			}

			tw.Close();
		}
		private void EraseTextFile()
		{
			TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + TEXT_FILE_NAME, false);
			tw.Write(String.Empty);
			tw.Close();
		}
		private void PopulateFinishBoard(BoardDebug aBoard)
		{
			if (aBoard.IsCamelReachEnd || aBoard.IsAllCamelRolled)
				m_FinishBoard.Add(aBoard);

			if(aBoard.m_SubBoard.Count == 0 && aBoard.DicesHistories[0].Length < m_DiceHistoryLenght)
				aBoard.PopulateSubBoard();

			foreach (BoardDebug board in aBoard.m_SubBoard)
				PopulateFinishBoard(board);
		}

		private void PopulateBoardByDiceOrder(BoardDebug aBoard)
		{
			if (aBoard.GetUnrolledCamelByRank().Length == 0)
			{
				string histWihtoutDice = new string(aBoard.DicesHistory.Where(c => !char.IsDigit(c)).ToArray());

				if (!m_BoardsByDiceOrder.ContainsKey(histWihtoutDice))
				{
					m_BoardsByDiceOrder.Add(histWihtoutDice, new List<BoardDebug>());
				}
				
				m_BoardsByDiceOrder[histWihtoutDice].Add(aBoard);
			}

			foreach (BoardDebug sub in aBoard.m_SubBoard)
			{
				PopulateBoardByDiceOrder(sub);
			}
		}
	}
}
