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
		if (model == null) {
			throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
		}
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.SetParent(parentTr, true);  // preserve world rotation and scaling
		gameObjectInstance.transform.localPosition = localPos;
		if (layer != -1)
			gameObjectInstance.layer = layer;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object and set it at world position under parent transform
	public static GameObject InstantiateUnderWithWorldPosition (this GameObject model, Transform parentTr, Vector3 position) {
		if (model == null) throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.SetParent(parentTr, true);
		gameObjectInstance.transform.position = position;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object at parent's position
	public static GameObject InstantiateUnder (this GameObject model, Transform parentTr) {
		return InstantiateUnderAtOn(model, parentTr, Vector3.zero);
	}

	/// Instantiate prefab / clone game object under parent retaining the local position, adding some offset
	public static GameObject InstantiateUnderWithOffset (this GameObject model, Transform parentTr, Vector3 offset) {
		if (model == null) throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.SetParent(parentTr, false);  // retain local position, rotation, scaling
		gameObjectInstance.transform.localPosition += offset;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object under parent retaining the local transform
	public static GameObject InstantiateUnderLocalTransform (this GameObject model, Transform parentTr) {
		return InstantiateUnderWithOffset(model, parentTr, Vector3.zero);
	}

	/// Duplicate object under the same parent (breaks any prefab link) with a new name
	public static GameObject Duplicate (this GameObject model, string name) {
		GameObject clone = InstantiateUnder(model, model.transform.parent);
		clone.name = name;
		return clone;
	}

}

public static class ComponentExtensions {

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrFail<T>(this Component script) where T : Component {
		return script.gameObject.GetComponentOrFail<T>();
	}

}

// from http://answers.unity3d.com/questions/150690/using-a-bitwise-operator-with-layermask.html
public static class LayerMaskExtensions {

	public static bool Contains (this LayerMask mask, int layer) {
		return (mask.value & (1 << layer)) > 0;
	}

	public static bool Contains (this LayerMask mask, GameObject go) {
		return (mask.value & (1 << go.layer)) > 0;
	}

}

// Inspired by UI Extensions CanvasGroupActivator.cs
public static class CanvasGroupExtensions {

	public static void Activate (this CanvasGroup group) {
		group.alpha = 1f;
		group.interactable = true;
		group.blocksRaycasts = true;
	}

	public static void Deactivate (this CanvasGroup group) {
		group.alpha = 0f;
		group.interactable = false;
		group.blocksRaycasts = false;
	}

}
