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

			if(aBoard.m_SubBoard.Count == 0 && aBoard.NbRound < 1)
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
