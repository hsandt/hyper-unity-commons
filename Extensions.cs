using UnityEngine;

public static class GameObjectExtensions {

	/// Extension to search any child transform with name passed as argument
	public static Transform Search(this Transform parent, string name, bool includeInactive = false) {
		// in case you would search from the very transform you were looking for
		if (parent.name == name) return parent;

		var childrenTransforms = parent.GetComponentsInChildren<Transform>(includeInactive);
		foreach (Transform t in childrenTransforms) {
			if (t.name == name) return t; // a child with the suggested name was found
		}

		return null; // no transforms with the suggested name were found
	}

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrFail<T>(this GameObject gameObject) where T : Component {
		T component = gameObject.GetComponent<T>();
		if (component == null)
			throw ExceptionsUtil.CreateExceptionFormat("No component of type {0} found on {1}.", typeof(T), gameObject);
		return component;
	}

	/// Instantiate prefab / clone game object and set it at local position under parent transform on layer layer
	public static GameObject InstantiateUnderAtOn (this GameObject model, Transform parentTr, Vector3 localPos, int layer = -1) {
		/* instantiate avatar prefab at given position */
		if (model == null) {
			throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
		}
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.parent = parentTr;
		gameObjectInstance.transform.localPosition = localPos;
		if (layer != -1)
			gameObjectInstance.layer = layer;
		return gameObjectInstance;
	}

}

public static class ComponentExtensions {

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrFail<T>(this Component script) where T : Component {
		return script.gameObject.GetComponentOrFail<T>();
	}

	/// Try to find a game object with tag, throw UnityException if none was found
	public static GameObject FindWithTagOrFail(this MonoBehaviour script, string name) {
		// Note: since this looks for game objects everywhere in the scene hierarchy,
		// this could be static class method, such as SearchHelper.FindWithTagOrFail(name)
		// (except for the debug message)
		GameObject go = GameObject.FindWithTag(name);
		if (go == null)
			throw new UnityException(string.Format("No game object found with tag {0} as queried by {1}.",
													name, script));
		return go;
	}

}
