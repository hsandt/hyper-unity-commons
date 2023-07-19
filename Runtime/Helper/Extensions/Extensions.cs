using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HyperUnityCommons
{

	public static class Vector2Extensions {

		public static Vector3 ToVector3 (this Vector2 vector2, float z) {
			return new Vector3(vector2.x, vector2.y, z);
		}

	}

	public static class RectExtensions {

		/// Expand bounds of rect just enough to contain point
		/// Equivalent to `rect = RectUtil.MBR(rect, point);`
		public static void Expand (ref this Rect rect, Vector2 point)
		{
			// We could use RectUtil.MBR, but it would create a new struct just to copy fields,
			// so we prefer assigning fields directly.
			rect.min = Vector2.Min(rect.min, point);
			rect.max = Vector2.Max(rect.max, point);
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
		/// Note that we ensure T : Component unlike the original GetComponent.
		/// If you need to get a component verifying some unrelated interface IMyInterface,
		/// use GetComponent[IMyInterface] and check the result yourself
		public static T GetComponentOrFail<T>(this GameObject gameObject) where T : Component {
			T component = gameObject.GetComponent<T>();
			// At some point Unity returned a pseudo-null if component was missing, which had to be checked via instance ID or ToString
			// https://stackoverflow.com/questions/44991173/getcomponent-returning-null-instead-of-null
			// Now it returns a proper null again, but for backward compatibility we still check the instance ID.
	        if (component == null || component.GetInstanceID() == 0) {
		        #if UNITY_EDITOR || DEVELOPMENT_BUILD
		        Debug.LogErrorFormat(gameObject, "GetComponentOrFail: no component of type {0} found on {1}", typeof(T), gameObject);
		        throw new Exception("GetComponentOrFail failed, see error above");
		        #else
		        throw ExceptionsUtil.CreateExceptionFormat("GetComponentOrFail: no component of type {0} found on {1}",
			        typeof(T), gameObject);
				#endif
	        }
			return component;
		}

		/// Instantiate prefab / clone game object (helper to avoid casting to GameObject every time)
		[System.Obsolete("Use GameObject.Instantiate which now returns with same type as first argument")]
		public static GameObject Instantiate (this GameObject model) {
			if (model == null) throw new Exception("Cannot instantiate null model.");
			GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
			return gameObjectInstance;
		}

		/// Instantiate prefab / clone game object under parent (named InstantiateUnder to avoid conflict with Instantiate<T>(Transform))
		[System.Obsolete("Use GameObject.Instantiate which now handles common arguments and returns with same type as first argument")]
		public static GameObject InstantiateUnder (this GameObject model, Transform parentTr, bool instantiateInWorldSpace = false) {
			if (model == null) throw new Exception("Cannot instantiate null model.");
			GameObject gameObjectInstance = Object.Instantiate(model, parentTr, instantiateInWorldSpace) as GameObject;
			return gameObjectInstance;
		}

		/// Instantiate prefab / clone game object under parent retaining the local position, adding some offset
		[System.Obsolete("Use GameObject.Instantiate with worldPositionStays: false, " +
		                 "then add offset to localPosition instead")]
		public static GameObject InstantiateUnderWithOffset (this GameObject model, Transform parentTr, Vector3 offset) {
			if (model == null) throw ExceptionsUtil.CreateExceptionFormat("Cannot instantiate null model.");
			GameObject gameObjectInstance = Object.Instantiate(model) as GameObject;
			gameObjectInstance.transform.SetParent(parentTr, false);  // retain local position, rotation, scaling
			gameObjectInstance.transform.localPosition += offset;
			return gameObjectInstance;
		}

		/// Instantiate prefab / clone game object and set it at local position under parent transform on layer layer
		[System.Obsolete("Use GameObject.Instantiate with worldPositionStays: false, " +
			"then add offset to localPosition and set layer manually instead")]
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
 		[System.Obsolete("Use GameObject.Instantiate which now handles common arguments and returns with same type as first argument")]
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

		/// Duplicate object under the same parent (as last sibling) with the same local position, with a new name
		/// This breaks any prefab link, but is meant for runtime anyway.
		/// For edit time, prefer DuplicateGameObjects.DuplicateSelectionBelow, which places the clone right under
		/// the original.
		public static GameObject Duplicate (this GameObject model, string name) {
			GameObject clone = Object.Instantiate(model, model.transform.parent);
			clone.name = name;
			return clone;
		}

		public static void DeactivateAllChildren(this Transform parent) {
			foreach (Transform child in parent) {
				child.gameObject.SetActive(false);
			}
		}

		/// Return true if this game object is in layer mask with value `layerMaskValue`
		public static bool IsInLayerMask(this GameObject go, int layerMaskValue)
		{
			return GameObjectUtil.IsInLayerMask(go, layerMaskValue);
		}

	}

	public static class ComponentExtensions {

		/// Try to get component of type T, log error if none found
		public static T GetComponentOrFail<T>(this Component script) where T : Component {
			return script.gameObject.GetComponentOrFail<T>();
		}

	}

	public static class AnimatorExtensions
	{
		/// Reset all animator parameters to their default
		public static void ResetParameters(this Animator animator)
		{
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

		/// Return true if the last animation (for the natural chain of animation for the current animator parameters)
		/// on the given animation layer index has finished at least one cycle
		/// For a non-looping animation, this simply means finished. But it is convenient to have a method that covers
		/// both looping and non-looping animations, so you can wait for this to be true without risking being stuck
		/// in the looping case.
		/// If there is a transition, this is false while transitioning
		public static bool HasLastAnimationFinishedOneCycle(this Animator animator, int animationLayerIndex = 0)
		{
			// See https://answers.unity.com/questions/362629/how-can-i-check-if-an-animation-is-being-played-or.html
			// Note that we check for > 1f just in case Unity decides not to transition immediately when value is 1f
			// (it's hard to test landing on 1f exactly, so safer). This works because normalizedTime keeps increasing
			// beyond 1 after end of animation if we don't transition immediately (whether looping or not).
			// We must also check !animator.IsInTransition so we are sure that there is not another animation after that.
			return animator.GetCurrentAnimatorStateInfo(animationLayerIndex).normalizedTime > 1f &&
				!animator.IsInTransition(animationLayerIndex);
		}

		/// Coroutine method to wait for last animation to end or, if looping, finish one cycle
		public static IEnumerator WaitForLastAnimationFinishedOneCycleCoroutine(this Animator animator, int animationLayerIndex = 0)
		{
			// Wait one frame to let the animator enter the next animation first, in case we just set animator
			// parameters this frame. This is a waste of 1 frame if we were already in the correct animation this frame
			// (e.g. when it was the Default animation), but safer in general case, so it's worth always doing it.
			yield return null;

			yield return new WaitUntil(() => HasLastAnimationFinishedOneCycle(animator, animationLayerIndex));
		}

		/// Async method to wait for last animation to end or, if looping, finish one cycle
		public static async Task WaitForLastAnimationFinishedOneCycleAsync(this Animator animator, int animationLayerIndex = 0)
		{
			while (true)
			{
				// Yield before checking condition in the block, so that we are sure to wait one frame at the beginning
				// to let the animator enter the next animation first before the first check
				// Same remark as WaitForFirstLayerAnimationEndCoroutine, a waste in some cases, but safer in general
				await Task.Yield();

				if (HasLastAnimationFinishedOneCycle(animator, animationLayerIndex))
				{
					return;
				}
			}
		}

		/// Return true when animator is playing animation with tag (by hash) on animation layer index
		/// If there is a transition, this is true while transitioning *from* tagged animation, but not
		/// *to* tagged animation (unless the previous animation has the same tag)
		public static bool IsTaggedAnimationRunning(this Animator animator, int tagHash, int animationLayerIndex = 0)
		{
			// Since we have tag hash already, comparing hashes directly is faster than passing tag string to IsTag
			return animator.GetCurrentAnimatorStateInfo(animationLayerIndex).tagHash == tagHash;
		}

		/// Coroutine method to wait for tagged animation to start
		public static IEnumerator WaitForTaggedAnimationRunningCoroutine(this Animator animator, int tagHash,
			int animationLayerIndex = 0)
		{
			// Wait one frame to let the animator enter the next animation first, in case we just set animator
			// parameters this frame. This is a waste of 1 frame if we were already in the correct animation this frame
			// (e.g. when it was the Default animation), but safer in general case, so it's worth always doing it.
			yield return null;

			yield return new WaitUntil(() => IsTaggedAnimationRunning(animator, tagHash, animationLayerIndex));
		}

		/// Async method to wait for tagged animation to start
		public static async Task WaitForTaggedAnimationRunningAsync(this Animator animator, int tagHash,
			int animationLayerIndex = 0)
		{
			while (true)
			{
				// Yield before checking condition in the block, so that we are sure to wait one frame at the beginning
				// to let the animator enter the next animation first before the first check
				// Same remark as WaitForFirstLayerAnimationEndCoroutine, a waste in some cases, but safer in general
				await Task.Yield();

				if (IsTaggedAnimationRunning(animator, tagHash, animationLayerIndex))
				{
					return;
				}
			}
		}
	}

	public static class AnimationCurveExtensions {

		/// Return duration of animation (time of last key)
		public static float GetDuration(this AnimationCurve animationCurve) {
			return AnimationCurveUtil.GetDuration(animationCurve);
		}

	}

	// from http://answers.unity3d.com/questions/150690/using-a-bitwise-operator-with-layermask.html
	public static class LayerMaskExtensions {

		/// Does the layer mask contain the specified layer?
		public static bool Contains (this LayerMask mask, int layer) {
			return GameObjectUtil.IsInLayerMask(layer, mask);
		}

		/// Does the layer mask contain the layer of the specified game object?
		public static bool Contains (this LayerMask mask, GameObject go) {
			return GameObjectUtil.IsInLayerMask(go, mask);
		}

	}

	public static class ColorExtensions {

		public static Color ToVisible (this Color color) {
			return color.ToAlpha(1f);
		}

		public static Color ToInvisible (this Color color) {
			return color.ToAlpha(0f);
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

}
