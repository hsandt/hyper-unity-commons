using System;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
	/// Mutable and serializable tuple struct for 2 elements of arbitrary types
	/// While C# System.Tuple can be used in most cases, it is immutable and not serializable, so if you need to modify
	/// pair elements later or edit them in the inspector, use this struct instead.
	/// Note that it is a struct, so instances will be copied by value
	/// (but First and Second will follow the rules of types T and U)
	[Serializable]
	public struct Pair<T, U>
	{
		public T first;

		[Obsolete("Use first directly")]
		public T First { get => first; set => first = value; }

		public U second;

		[Obsolete("Use second directly")]
		public U Second { get => second; set => second = value; }

		public Pair(T first, U second)
		{
			this.first = first;
			this.second = second;
		}

		public void Deconstruct(out T first, out U second)
		{
			first = this.first;
			second = this.second;
		}

		public override string ToString()
		{
			return $"({first}, {second})";
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

	/// Alternative to Pair with more explicit names when used for key-value pairs
	[Serializable]
	public struct KeyValuePair<TKey, TValue>
	{
		public TKey key;
		public TValue value;

		public KeyValuePair(TKey key, TValue value)
		{
			this.key = key;
			this.value = value;
		}

		public void Deconstruct(out TKey key, out TValue value)
		{
			key = this.key;
			value = this.value;
		}

		public override string ToString()
		{
			return $"{{key = {key}, value = {value}}}";
		}
	}
}
