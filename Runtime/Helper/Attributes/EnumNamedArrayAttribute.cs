// https://answers.unity.com/questions/1589226/showing-an-array-with-enum-as-keys-in-the-property.html
// Authors:
// - bonzairob: original code
// - huulong (hsandt): minor improvements

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
	/// Add this attribute to an array of int where each index corresponds to an enum value to display indices
	/// as enum names.
	/// Ex:
	/// [EnumNamedArray(typeof(MyEnum))]
	/// public int[] valuesPerEnum = new int[EnumUtil.GetCount<MyEnum>()];
	public class EnumNamedArrayAttribute : PropertyAttribute
	{
		public readonly string[] names;

		public EnumNamedArrayAttribute(System.Type enumType)
		{
			names = System.Enum.GetNames(enumType);
		}
	}
}
