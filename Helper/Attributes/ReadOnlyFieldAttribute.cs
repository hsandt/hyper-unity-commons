using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{
	/// Attribute for fields to gray out in the inspector
	/// The actual code is in ReadOnlyFieldDrawer
	public class ReadOnlyFieldAttribute : PropertyAttribute
	{
	}
}
