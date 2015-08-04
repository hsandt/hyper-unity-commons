using UnityEngine;

public static class GameObjectExtensions {

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrFail<T>(this GameObject gameObject) where T : Component {
		T component = gameObject.GetComponent<T>();
		if (component == null)
			Debug.LogErrorFormat("No component of type {0} found on {1}.", typeof(T), gameObject);
		return component;
	}

	/// Instantiate prefab / clone game object and set it at local position under parent transform on layer layer
	public static GameObject InstantiateUnderAtOn (this GameObject model, Transform parentTr, Vector3 localPos, int layer = -1) {
		/* instantiate avatar prefab at given position */
		if (model == null) {
			Debug.LogErrorFormat("Cannot instantiate null model.");
			return null;
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

}
