using System;
using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script
{
	public class LongTermBoardAnalyser
	{
		class CamelRankInfo
		{
			public CamelRankManager m_CamelRankManager;
			public double m_Proportion;

			public override string ToString()
			{
				return m_Proportion.ToString();
			}
		}

		private readonly Dictionary<string, IBoard> m_FinishBoard = new Dictionary<string, IBoard>();
		private readonly Dictionary<string, IBoard> m_UnfinishBoard = new Dictionary<string, IBoard>();
		private Dictionary<string, IBoard> m_UncompleteBoards = new Dictionary<string, IBoard>();

		//CamelRankManager avec sa proportion. Toutes les proportions <= 1
		private List<CamelRankInfo> m_CamelRankInfos = new List<CamelRankInfo>();

		private int m_MaxDicesRoll; //TODO pas sur encore quoi faire avec lui

		private readonly bool m_IsMemoryFail;

		private double TotalPropotionRankInfos => m_CamelRankInfos.Select(cr => cr.m_Proportion).Sum();

		private long TotalWeight => GetWeight(m_FinishBoard.Values.ToList()) + GetWeight(m_UnfinishBoard.Values.ToList()) + GetWeight(m_UncompleteBoards.Values.ToList());

		private double GetTotalProportionAllRankManagers
		{
			get
			{
				double total = 0;

				foreach (var info in m_CamelRankInfos)
					total += info.m_Proportion;

				return total;
			}
		}

		public LongTermBoardAnalyser(IBoard aBoard, Action aActionAfterManageBoard)
		{
			//TODO supporter plusieurs board...il y a un bug quand on ajoute boards avec 2 dés roulés
			m_MaxDicesRoll = m_FinishBoard.Values.SelectMany(fb => fb.DicesHistories).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;

			ManageBoards(new List<IBoard>{aBoard});
			if(aActionAfterManageBoard != null)
				aActionAfterManageBoard.Invoke();

			if (m_FinishBoard.Values.Any())
			{
				AddCamelRankManager(m_FinishBoard.Values.ToList());
				m_FinishBoard.Clear();
			}

			while (m_UncompleteBoards.Any() && IsContinueAnalyze())
			{
				try
				{
					CreateBoards();
				}
				catch (Exception ex)
				{
					m_IsMemoryFail = true;
				}

				if (m_FinishBoard.Values.Any())
				{
					AddCamelRankManager(m_FinishBoard.Values.ToList());
					m_FinishBoard.Clear();
				}

				if (m_IsMemoryFail)
					GameRules.Log(string.Format("\n\n Je fail at: {0} avec {1} UncompleteBoards \n\n", GetTotalProportionAllRankManagers, m_UncompleteBoards.Count));

				m_UncompleteBoards = new Dictionary<string, IBoard>(m_UnfinishBoard);
				m_UnfinishBoard.Clear();
				m_MaxDicesRoll++;
			}
		}

		public List<IBoard> GetAllBoards()
		{
			List<IBoard> allBoards = new List<IBoard>();
			allBoards.AddRange(m_UnfinishBoard.Values);
			allBoards.AddRange(m_FinishBoard.Values);

			return allBoards;
		}
		
		public Dictionary<char, double[]> GetAverageCamelRankInfo()
		{
			Dictionary<char, double[]> retval = new Dictionary<char, double[]>();

			foreach (var info in m_CamelRankInfos)
			{
				foreach (var camelRank in info.m_CamelRankManager.GetCamelRanks)
				{
					if (!retval.ContainsKey(camelRank.CamelName))
					{
						double[] newRank = new double[5];

						for (int i = 0; i < newRank.Length; i++)
							newRank[i] = camelRank.TimeFinish(i) / camelRank.m_TotalFinish * info.m_Proportion;

						retval.Add(camelRank.CamelName, newRank);
					}
					else
					{
						for (int i = 0; i < retval[camelRank.CamelName].Length; i++)
							retval[camelRank.CamelName][i] += (double)camelRank.TimeFinish(i) / camelRank.m_TotalFinish * info.m_Proportion;
					}
				}
			}

			return retval;
		}

		public Ev GetEv()
		{
			Dictionary<char, double[]> camelRanks = GetAverageCamelRankInfo();
			Tuple<char, double> highestEvFirstPlace = new Tuple<char, double>('x', -10f);
			Tuple<char, double> highestEvLastPlace = new Tuple<char, double>('x', -10f);

			foreach (var cr in camelRanks)
			{
				double ev = LongTermCardGuesser.Instance.GetPriceForFirst(cr.Key) * cr.Value[0];
				if (ev > highestEvFirstPlace.Item2)
					highestEvFirstPlace = new Tuple<char, double>(cr.Key, ev);

				ev = LongTermCardGuesser.Instance.GetPriceForLast(cr.Key) * cr.Value[4];//todo magic number
				if (ev > highestEvLastPlace.Item2)
					highestEvLastPlace = new Tuple<char, double>(cr.Key, ev);
			}

			Tuple<char, double> highest;
			bool isFirst;

			if (highestEvFirstPlace.Item2 > highestEvLastPlace.Item2)
			{
				highest = highestEvFirstPlace;
				isFirst = true;
			}
			else
			{
				highest = highestEvLastPlace;
				isFirst = false;
			}

			string info = String.Format("{0} cards pour le {1}, avec une proportion de {2}", 
				isFirst? "First" : "Last", GameRules.FullNameCamel(highest.Item1), GetTotalProportionAllRankManagers);

			Ev retval = new Ev
			{
				m_PlayerAction = GameRules.PlayerAction.PickLongTermCard,
				m_Ev = (float)highest.Item2,
				m_Info = info
			};

			return retval;
		}

		private void ManageBoards(List<IBoard> aBoards)
		{
			var finishBoards = aBoards.GroupBy(b => b.IsCamelReachEnd);

			foreach (var fb in finishBoards)
			{
				if (fb.Key)
				{
					foreach (var board in fb.ToList())
						m_FinishBoard.Add(board.BoardStateString, board);
				}
				else
				{
					foreach (var board in fb.ToList())
						m_UncompleteBoards.Add(board.BoardStateString, board);
				}
			}
		}

		private void CreateBoards()
		{
			while (m_UncompleteBoards.Count > 0)
			{
				Dictionary<string, IBoard> newUncompleteBoards = new Dictionary<string, IBoard>();

				foreach (var unCompleted in m_UncompleteBoards)
				{
					unCompleted.Value.PopulateSubBoard();

					foreach (var subBoard in unCompleted.Value.m_SubBoard)
					{
						if (subBoard.IsCamelReachEnd)
							AddBoardIntoDict(subBoard, m_FinishBoard);
						else if (subBoard.DicesHistories[0].Length < m_MaxDicesRoll)
							AddBoardIntoDict(subBoard, newUncompleteBoards);
						else AddBoardIntoDict(subBoard, m_UnfinishBoard);
					}
				}

				m_UncompleteBoards = newUncompleteBoards;
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

		private void AddCamelRankManager(List<IBoard> aBoards)
		{
			CamelRankManager newManager = new CamelRankManager(aBoards);
			var t = GetWeight(aBoards);
			m_CamelRankInfos.Add(
				new CamelRankInfo
				{
					m_CamelRankManager = newManager,
					m_Proportion = GetProportion(aBoards)
				});
		}

		private long GetWeight(List<IBoard> aDict)
		{
			long retval = 0;
			foreach (var board in aDict)
				retval += Convert.ToInt64(board.Weight);

			return retval;
		}

		private double GetProportion(List<IBoard> aBoards)
		{
			return Math.Round((1 - GetTotalProportionAllRankManagers) * GetWeight(aBoards) / TotalWeight, 15);
		}

		private bool IsContinueAnalyze()
		{
			//TODO
			return !m_IsMemoryFail;
		}
	}
}
