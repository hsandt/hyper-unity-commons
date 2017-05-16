﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Vector2Extensions {

	public static Vector3 ToVector3 (this Vector2 vector2, float z) {
		return new Vector3(vector2.x, vector2.y, z);
	}

}

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
	public static T GetComponentOrFail<T>(this GameObject gameObject) {
		T component = gameObject.GetComponent<T>();
		if (component == null)
			throw ExceptionsUtil.CreateExceptionFormat("No component of type {0} found on {1}.", typeof(T), gameObject);
		return component;
	}

	/// Instantiate prefab / clone game object (helper to avoid casting to GameObject every time)
	public static GameObject Instantiate (this GameObject model) {
		if (model == null) throw new Exception("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object under parent (named InstantiateUnder to avoid conflict with Instantiate<T>(Transform))
	public static GameObject InstantiateUnder (this GameObject model, Transform parentTr, bool instantiateInWorldSpace = false) {
		if (model == null) throw new Exception("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model, parentTr, instantiateInWorldSpace) as GameObject;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object under parent retaining the local position, adding some offset
	[System.Obsolete("Use Instantiate as GameObject with parent argument, then add offset to localPosition instead")]
	public static GameObject InstantiateUnderWithOffset (this GameObject model, Transform parentTr, Vector3 offset) {
		if (model == null) throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.SetParent(parentTr, false);  // retain local position, rotation, scaling
		gameObjectInstance.transform.localPosition += offset;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object and set it at local position under parent transform on layer layer
	public static GameObject InstantiateUnderAtOn (this GameObject model, Transform parentTr, Vector3 localPos, int layer = -1) {
		if (model == null) throw new Exception("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.SetParent(parentTr, true);  // preserve world rotation and scaling
		gameObjectInstance.transform.localPosition = localPos;
		if (layer != -1)
			gameObjectInstance.layer = layer;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object and set it at world position under parent transform
	[System.Obsolete("Use Instantiate as GameObject with parent and position arguments instead")]
	public static GameObject InstantiateUnderWithWorldPosition (this GameObject model, Transform parentTr, Vector3 position) {
		if (model == null) throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
		GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
		gameObjectInstance.transform.SetParent(parentTr, true);
		gameObjectInstance.transform.position = position;
		return gameObjectInstance;
	}

	/// Instantiate prefab / clone game object under parent retaining the local transform
	[System.Obsolete("Use Instantiate as GameObject with parent argument instead")]
	public static GameObject InstantiateUnderLocalTransform (this GameObject model, Transform parentTr) {
		return InstantiateUnderWithOffset(model, parentTr, Vector3.zero);
	}

	/// Duplicate object under the same parent with the same local position, with a new name. This breaks any prefab link.
	public static GameObject Duplicate (this GameObject model, string name) {
		GameObject clone = Object.Instantiate(model, model.transform.parent) as GameObject;
		clone.name = name;
		return clone;
	}

}

public static class ComponentExtensions {

	/// Try to get component of type T, log error if none found
	public static T GetComponentOrFail<T>(this Component script) {
		return script.gameObject.GetComponentOrFail<T>();
	}

}

public static class AnimatorExtensions {

	/// Reset all animator parameters to their default
	public static void ResetParameters (this Animator animator) {
		AnimatorControllerParameter[] parameters = animator.parameters;

		for (int i = 0; i < parameters.Length; i++) {
			AnimatorControllerParameter parameter = parameters[i];
			switch (parameter.type)
			{
			case AnimatorControllerParameterType.Int:
				animator.SetInteger(parameter.name, parameter.defaultInt);
				break;
			case AnimatorControllerParameterType.Float:
				animator.SetFloat(parameter.name, parameter.defaultFloat);
				break;
			case AnimatorControllerParameterType.Bool:
				animator.SetBool(parameter.name, parameter.defaultBool);
				break;
			}
		}
	}

}

// from http://answers.unity3d.com/questions/150690/using-a-bitwise-operator-with-layermask.html
public static class LayerMaskExtensions {

	/// Does the layer mask contain the specified layer?
	public static bool Contains (this LayerMask mask, int layer) {
		return (mask.value & (1 << layer)) > 0;
	}

	/// Does the layer mask contain the layer of the specified game object?
	public static bool Contains (this LayerMask mask, GameObject go) {
		return (mask.value & (1 << go.layer)) > 0;
	}

}

public static class ColorExtensions {

	public static Color ToVisible (this Color color) {
		Color visibleColor = color;
		visibleColor.a = 1;
		return visibleColor;
	}

	public static Color ToInvisible (this Color color) {
		Color invisibleColor = color;
		invisibleColor.a = 0;
		return invisibleColor;
	}

	public static Color ToAlpha (this Color color, float alpha) {
		Color alphaColor = color;
		alphaColor.a = alpha;
		return alphaColor;
	}

}

// Inspired by UI Extensions CanvasGroupActivator.cs
public static class CanvasGroupExtensions {

	/// Make canvas group visible and interactable with mouse / touch input
	public static void Activate (this CanvasGroup group) {
		group.alpha = 1f;
		group.interactable = true;
		group.blocksRaycasts = true;
	}

	/// Make canvas group invisible and not interactable with mouse / touch input
	public static void Deactivate (this CanvasGroup group) {
		group.alpha = 0f;
		group.interactable = false;
		group.blocksRaycasts = false;
	}

	/// Make canvas group interactable with mouse / touch input without changing its visibility
	public static void EnableInteraction (this CanvasGroup group) {
		group.interactable = true;
		group.blocksRaycasts = true;
	}

	/// Make canvas group not interactable with mouse / touch input
	public static void DisableInteraction (this CanvasGroup group) {
		group.interactable = false;
		group.blocksRaycasts = false;
	}

}
