using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CommonsDebug
{
	public static class AssertUtil
	{
		// Strip unless UNITY_EDITOR || DEVELOPMENT_BUILD
		// This may be useful to remove the various #if UNITY_EDITOR || DEVELOPMENT_BUILD in the project
		// but we need to define the same for all Debug.Log variants.
		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
		public static void Assert(bool condition, string message)
		{
			Debug.Assert(condition, message);
		}

		/// <summary>
		/// Assert that passed list/array is not null, and that no elements are null
		/// </summary>
		/// <param name="list">List list/array to verify</param>
		/// <param name="context">Object owning the list/array, if any. Used as context for the Debug Console.</param>
		/// <param name="listName">Name of list/array variable for debug</param>
		/// <typeparam name="T">Type of elements in the list/array</typeparam>
		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
		public static void AssertListElementsNotNull<T>(IReadOnlyList<T> list, Object context, string listName)
		{
			if (list != null)
			{
				// Assert on null entry, but not on absence of entry:
				// entries may be added later by code
				for (int i = 0; i < list.Count; i++)
				{
					Debug.AssertFormat(list[i] != null, context, "{0}[{1}] is null on {2}", listName, i, context);
				}
			}
			else
			{
				Debug.LogErrorFormat(context, "{0} is null on {1}", listName, context);
			}
		}
	}
}
