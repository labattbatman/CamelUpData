namespace CamelUpData.Script.ReUse
{
	public static class MathFunc
	{
		public static int Factorial(int n)
		{
			if (n >= 2) return n * Factorial(n - 1);
			return 1;
		}
	}
}
