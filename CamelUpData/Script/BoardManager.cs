﻿using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class BoardManager
	{
		private readonly Dictionary<string, IBoard> m_UnfinishBoardByMaxRound = new Dictionary<string, IBoard>();
		private readonly Dictionary<string, IBoard> m_FinishBoard = new Dictionary<string, IBoard>();//to private
		private Dictionary<string, IBoard> m_UncompleteBoards= new Dictionary<string, IBoard>();

		private int m_MaxDicesRoll;

		public BoardManager(int aMaxDicesRoll)
		{
			//*2 car on comparer avec le DiceHistory qui contient le chiffre roulé
			m_MaxDicesRoll = aMaxDicesRoll * 2;
		}

		public long TotalWeigh
		{
			get
			{
				long retval = 0;
				foreach (var board in m_UnfinishBoardByMaxRound)
					retval += board.Value.Weight;
				foreach (var board in m_FinishBoard)
					retval += board.Value.Weight;
				return retval;
			}
		}

		public void CreateBoard(string aBoard, bool aAddWeightByReachEnd)
		{
			CreateBoards(new Board(aBoard), aAddWeightByReachEnd);
		 }

		public void CreateBoardDebug(string aBoard, bool aAddWeightByReachEnd)
		{
			CreateBoards(new BoardDebug(aBoard), aAddWeightByReachEnd);
		}

		public void CreateBoardByte(string aBoard, bool aAddWeightByReachEnd)
		{
			CreateBoards(new BoardByte(aBoard), aAddWeightByReachEnd);
		}

		public List<IBoard> GetAllBoards()
		{
			List<IBoard> allBoards = new List<IBoard>();
			allBoards.AddRange(m_UnfinishBoardByMaxRound.Values);
			allBoards.AddRange(m_FinishBoard.Values);

			return allBoards;
		}

		private void CreateBoards(IBoard aBoard, bool aAddWeightByReachEnd)
		{
			m_UncompleteBoards.Add(aBoard.BoardStateString, aBoard);

			while (m_UncompleteBoards.Count > 0)
			{
				Dictionary<string, IBoard> newUncompleteBoards = new Dictionary<string, IBoard>();

				foreach (var unCompleted in m_UncompleteBoards)
				{
					unCompleted.Value.PopulateSubBoard();

					foreach (var subBoard in unCompleted.Value.m_SubBoard)
					{
						if (subBoard.IsCamelReachEnd)
						{
							AddBoardIntoDict(subBoard, m_FinishBoard);
						}
						else if (subBoard.DicesHistories[0].Length < m_MaxDicesRoll)
							AddBoardIntoDict(subBoard, newUncompleteBoards);
						else AddBoardIntoDict(subBoard, m_UnfinishBoardByMaxRound);
					}
				}
				m_UncompleteBoards = newUncompleteBoards;
			}

			if (aAddWeightByReachEnd)
			{
				int maxDiceHistory = m_FinishBoard.Values.SelectMany(fb => fb.DicesHistories).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
				foreach (var board in m_FinishBoard.Values)
				{
					if (board.IsCamelReachEnd)
					{
						board.AddWeightByReachEnd(maxDiceHistory);
					}
				}
			}
		}

		private void AddBoardIntoDict(IBoard aBoard, Dictionary<string, IBoard> aDict)
		{
			if (aDict.ContainsKey(aBoard.BoardStateString))
			{
				aDict[aBoard.BoardStateString].AddWeight(aBoard);
			}
			else
			{
				aDict.Add(aBoard.BoardStateString, aBoard);
			}
		}
	}
}
