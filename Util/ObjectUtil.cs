using UnityEngine;
using System;
using System.Collections.Generic;

namespace CommonsHelper
{
	public static class ObjectUtil
	{

		/// Try to find a game object with tag, throw UnityException if none was found
		public static GameObject FindWithTagOrFail(string name)
		{
			GameObject go = GameObject.FindWithTag(name);
			if (go == null)
			{
				throw ExceptionsUtil.CreateExceptionFormat("Could not find game object with tag {0}.", name);
			}
			return go;
		}
		
		public static void DeactivateAllChildren(this Transform parent)
		{
			foreach (Transform child in parent)
			{
				child.gameObject.SetActive(false);
			}
		}
	}
}

