using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityConstants;

/// Class that memoizes objects by tag (deprecated: prefer pure FindWithTag + store ref in Awake)
public class TagLocator : SingletonManager<TagLocator> {

	protected TagLocator () {} // guarantee this will be always a singleton only - can't use the constructor!

	// REFACTOR: allow storing monobehaviours too, as MonoBehaviour objects, then downcast in the get properties of the TagLocator subclass,
	// to immediately retrieve script of interest
	// dictionary of references to transforms
	Dictionary<string, Transform> taggedTransforms = new Dictionary<string, Transform>();

	// Use this for initialization
	void Awake () {
		Instance = this;
	}

	/// If not already found, locate game object with given tag and store reference. Return game object with tag goTag
	public Transform LocateTransformWithTag (string goTag) {
		Debug.LogWarning("TagLocator is deprecated");

		Transform locatedTr;
		if (taggedTransforms.TryGetValue(goTag, out locatedTr))
			return locatedTr;

		// function body (if pattern reused, refactor)
		GameObject locatedGo = GameObject.FindWithTag(goTag);
		if (locatedGo == null) throw ExceptionsUtil.CreateExceptionFormat("Could not locate game object with tag {0}.", goTag);
		locatedTr = locatedGo.transform;

		taggedTransforms[goTag] = locatedTr;
		return locatedTr;
	}

}
