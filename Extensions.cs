using UnityEngine;

public static class GameObjectExtensions {

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrDie<T>(this GameObject gameObject) where T : Component {
		T component = gameObject.GetComponent<T>();
		if (component == null)
			Debug.LogErrorFormat("No component of type {0} found on {1}.", typeof(T), gameObject);
		return component;
	}

}

public static class ComponentExtensions {

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrDie<T>(this Component script) where T : Component {
		return script.gameObject.GetComponentOrDie<T>();
	}

}
