using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityConstants;

/// Static class that memoizes objects by tag
public static class Locator {

	/// Dictionary of references to transforms
	static Dictionary<string, Transform> taggedTransforms = new Dictionary<string, Transform>();

	/// Return game object with given tag using memoization
	public static Transform GetTransform (string tag) {
		Transform tr;
		// return any memoized transform
		if (taggedTransforms.TryGetValue(tag, out tr))
			return tr;

		// function body (if memoize pattern is reused elsewhere, refactor)
		GameObject go = GameObject.FindWithTag(tag);
		if (go == null) throw ExceptionsUtil.CreateExceptionFormat("Could not locate game object with tag {0}.", tag);
		tr = go.transform;

		// memoize transform and return it
		taggedTransforms[tag] = tr;
		return tr;
	}

}
