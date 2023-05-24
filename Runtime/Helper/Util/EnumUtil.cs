using System;

namespace HyperUnityCommons
{

	public static class EnumUtil
	{

		public static T[] GetValues<T>()
		{
			return Enum.GetValues(typeof(T)) as T[];
		}

		public static int GetCount<T>()
		{
			return Enum.GetValues(typeof(T)).Length;
		}

	}
}
