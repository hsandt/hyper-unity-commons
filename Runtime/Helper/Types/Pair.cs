using System;

namespace HyperUnityCommons
{
	/// Mutable tuple class for 2 elements of arbitrary types
	/// While C# System.Tuple can be used in most cases, it is immutable, so if you need to modify
	/// pair elements later, use this class instead.
	public struct Pair<T, U>
	{
		public T First { get; set; }
		public U Second { get; set; }

		public Pair(T first, U second)
		{
			First = first;
			Second = second;
		}

		public override string ToString()
		{
			return $"({First}, {Second})";
		}
	}

	/// Static construction class (inspired by Vexe Tuple.Create)
	public static class Pair
	{
		public static Pair<T1, T2> Create<T1, T2>(T1 first, T2 second)
		{
			return new Pair<T1, T2>(first, second);
		}
	}
}

