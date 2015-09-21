using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityConstants;

public class Locator : SingletonManager<Locator> {

	protected Locator () {} // guarantee this will be always a singleton only - can't use the constructor!

	// dictionary of references to transforms
	Dictionary<string, Transform> locatedTransformDict = new Dictionary<string, Transform>();

	// Use this for initialization
	void Awake () {
		Instance = this;
	}
	
	/// If not already found, locate game object with given tag and store reference. Return game object with tag goTag
	protected Transform LocateTransformWithTag (string goTag) {
		Transform locatedTr;
		bool isTagLocated = locatedTransformDict.TryGetValue(goTag, out locatedTr);
		if (isTagLocated) return locatedTr;
		GameObject locatedGo = GameObject.FindWithTag(goTag);
		if (locatedGo == null) throw ExceptionsUtil.CreateExceptionFormat("Could not locate game object with tag {0}.", goTag);
		locatedTr = locatedGo.transform;
		locatedTransformDict[goTag] = locatedTr;
		return locatedTr;
	}

}
