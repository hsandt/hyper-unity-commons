using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Commons.Pattern
{

	/// Static class that memoizes objects by tag
	public static class Locator {

		/// Dictionary of references to transforms
		static Dictionary<string, GameObject> taggedGameObjects = new Dictionary<string, GameObject>();

		/// Return game object transform with given tag using memoization
		public static GameObject FindWithTag (string tag) {
			GameObject go;

			// return any memoized transform, if the object is still valid
			if (taggedGameObjects.TryGetValue(tag, out go)) {
				if (go != null)
					return go;
				else {
					// GameObject was registered with tag, but is now considered null. It has probably been destroyed by a script or during scene loading.
					taggedGameObjects.Remove(tag);
					Debug.LogWarningFormat("Game object with tag {0} was memoized but got destroyed in the meantime. Please avoid using Locator.FindWithTag() with transient objects.");
				}
			}

			// search object with tag
			go = GameObject.FindWithTag(tag);
			if (go != null) {
				// memoize transform and return it
				taggedGameObjects[tag] = go;
				return go;
			}
			else {
				Debug.LogWarningFormat("Could not locate game object with tag {0}.", tag);
				return null;
			}
		}

	}

}
