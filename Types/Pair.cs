﻿/// Tuple class for 2 elements of arbitrary types
public struct Pair<T, U> {

	public T First { get; set; }
	public U Second { get; set; }

	public Pair(T first, U second) {
		this.First = first;
		this.Second = second;
	}

	public override string ToString()
	{
		return string.Format("({0}, {1})", First, Second);
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
