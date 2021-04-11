using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsPattern
{
	/// Singleton manager that memoizes objects by tag
	/// It is important to make it a singleton with instance members rather than a (static) class with static members
	/// to avoid preserving taggedGameObjects across play sessions, in case we change tags between two sessions.
	/// SEO: before any script whose Awake uses LocatorManager (e.g. PoolManager setting poolTransform in Init)
	/// executionOrder was set to -50 in .meta to work with most scripts
	public class LocatorManager : SingletonManager<LocatorManager>
	{
		/// Dictionary of references to game objects
		/// Note that this has static lifetime, so avoid using Locator to find game objects that can be destroyed
		/// (including on scene change)
		private readonly Dictionary<string, GameObject> taggedGameObjects = new Dictionary<string, GameObject>();

		/// Return game object with given tag using memoization
		/// If the object was found but destroyed, warn and try to find it again
		public GameObject FindWithTag (string tag)
		{
			GameObject go;

			// return any memoized game object, if the object is still valid
			if (taggedGameObjects.TryGetValue(tag, out go))
			{
				if (go != null)
				{
					return go;
				}
				
				// GameObject was registered with tag, but is now considered null. It has probably been destroyed
				// by a script, or during scene loading while Locator was flagged DontDestroyOnLoad (not recommended).
				// Clean up destroyed object now.
				taggedGameObjects.Remove(tag);
				Debug.LogWarningFormat("Game object with tag {0} was memoized but got destroyed in the meantime. " +
				                       "Please avoid using Locator.Instance.FindWithTag() with objects that may be destroyed " +
				                       "while LocatorManager instance survives.", tag);
			}

			// search object with tag
			go = GameObject.FindWithTag(tag);
			if (go != null)
			{
				// memoize game object and return it
				taggedGameObjects[tag] = go;
				return go;
			}
			
			Debug.LogWarningFormat("Could not locate game object with tag {0}.", tag);
			return null;
		}
	}
}
