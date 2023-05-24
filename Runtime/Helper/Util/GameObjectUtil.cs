using UnityEngine;
using System;
using System.Collections.Generic;

namespace HyperUnityCommons
{
	public static class GameObjectUtil
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

		/// Return true if layer value `layer` is in layer mask with value `layerMaskValue`
		public static bool IsInLayerMask(int layer, int layerMaskValue)
		{
			return (layerMaskValue & (1 << layer)) > 0;
		}

		/// Return true if game object `go` is in layer mask with value `layerMaskValue`
		public static bool IsInLayerMask(GameObject go, int layerMaskValue)
		{
			return IsInLayerMask(go.layer, layerMaskValue);
		}
	}
}

