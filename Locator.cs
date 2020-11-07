﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommonsPattern
{

	/// Static class that memoizes objects by tag
	public static class Locator {

		/// Dictionary of references to game objects
		/// Note that this has static lifetime, so avoid using Locator to find game objects that can be destroyed
		/// (including on scene change)
		static Dictionary<string, GameObject> taggedGameObjects = new Dictionary<string, GameObject>();

		/// Return game object with given tag using memoization
		/// If the object was found but destroyed, warn and try to find it again
		public static GameObject FindWithTag (string tag) {
			GameObject go;

			// return any memoized game object, if the object is still valid
			if (taggedGameObjects.TryGetValue(tag, out go)) {
				if (go != null)
					return go;
				else {
					// GameObject was registered with tag, but is now considered null. It has probably been destroyed by a script or during scene loading.
					taggedGameObjects.Remove(tag);
					Debug.LogWarningFormat("Game object with tag {0} was memoized but got destroyed in the meantime. Please avoid using Locator.FindWithTag() with transient objects.", tag);
				}
			}

			// search object with tag
			go = GameObject.FindWithTag(tag);
			if (go != null) {
				// memoize game object and return it
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
