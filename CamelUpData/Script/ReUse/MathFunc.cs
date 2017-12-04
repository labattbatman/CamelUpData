using System;
using System.Collections.Generic;
using System.Linq;

namespace CamelUpData.Script.ReUse
{
	public static class MathFunc
	{
		public static int Factorial(int n)
		{
			if (n >= 2) return n * Factorial(n - 1);
			return 1;
		}

		public static List<bool[]> AllCombinationsBooleans(int aNBoolean)
		{
			List<bool[]> matrix = new List<bool[]>();
			double count = Math.Pow(2, aNBoolean);

			for (int i = 0; i < count; i++)
			{
				string str = Convert.ToString(i, 2).PadLeft(aNBoolean, '0');
				bool[] boolArr = str.Select((x) => x == '1').ToArray();

				matrix.Add(boolArr);
			}

			return matrix;
		}
	}
}
