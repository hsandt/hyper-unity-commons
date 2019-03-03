// from http://forum.unity3d.com/threads/replace-game-object-with-prefab.24311/
// Elecman, modified by huulong

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonsEditor
{

	public class ReplaceGameObjects : EditorWindow
	{
		/// Window vertical scroll position
		Vector2 scrollPosition;

		GameObject replacingObject;
		bool keepName = false;
		bool keepIcon = false;
		bool keepRotation = false;
		bool keepScale = false;

		[MenuItem ("Tools/Replace Game Objects %g")]
		public static void Init () {
			EditorWindow.GetWindow<ReplaceGameObjects> (false, "Replace Game Objects", true);
		}

		void OnGUI () {
			scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);

			GUILayout.Label ("Use Object", EditorStyles.boldLabel);

			// Expose Replacing Object
			// note that it will be cleared to null when leaving the scene/stage containing it
			replacingObject = EditorGUILayout.ObjectField (replacingObject, typeof(GameObject), true) as GameObject;

			keepName = EditorGUILayout.ToggleLeft ("Keep Name", keepName);
			keepIcon = EditorGUILayout.ToggleLeft ("Keep Icon", keepIcon);
			keepRotation = EditorGUILayout.ToggleLeft ("Keep Rotation", keepRotation);
			keepScale = EditorGUILayout.ToggleLeft ("Keep Scale", keepScale);

			if (GUILayout.Button ("Replace Selected")) {
				ReplaceSelected ();
			}

			EditorGUILayout.EndScrollView ();
		}

		void ReplaceSelected () {

			if (replacingObject != null) {

				List<GameObject> replacedObjects = new List<GameObject> ();

				foreach (Transform t in Selection.transforms) {

					GameObject o;

					// check if object is an actual prefab instance root in the Scene (from model, regular or variant prefab)
					// use IsAnyPrefabInstanceRoot to make sure it is a prefab root (including a prefab instance root parented to
					// another prefab instance), and not a non-prefab object parented to a prefab instance
					if (PrefabUtility.GetPrefabInstanceStatus(replacingObject) == PrefabInstanceStatus.Connected &&
					    PrefabUtility.IsAnyPrefabInstanceRoot(replacingObject)) {
						// instantiate it from the prefab to keep the link, but also keep properties 
						// overriden at instance level
						
						// GetCorrespondingObjectFromSource seems to sometimes return the object instance instead of the prefab
						// GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(replacingObject);

						// as a workaround, we find the path of the prefab asset and load it
						// to avoid this back-and-forth query, you can also use PrefabUtility.GetOriginalSourceOrVariantRoot
						// but you'll need reflection as it is internal
						string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(replacingObject);
						GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
	
						// FIXME: it seems that even with this workaround, SetParent below will cause an Error in some cases
						// because it considers you are reparenting a prefab directly
						o = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
						PrefabUtility.SetPropertyModifications(o, PrefabUtility.GetPropertyModifications(replacingObject));
					}

					// check if object is an actual prefab asset from Project view
					// note: we don't check PrefabUtility.GetPrefabAssetType(replacingObject) since a GameObject that
					// is an asset is always a prefab, never PrefabAssetType.NotAPrefab or PrefabAssetType.MissingAsset
					else if (AssetDatabase.Contains(replacingObject)) {
						// instantiate it with default values
						o = (GameObject)PrefabUtility.InstantiatePrefab(replacingObject);
					}

					else {
						// replacing object is a non-prefab (not even an instance) or prefab is missing
						// this includes a non-prefab object located under a prefab root
						o = Instantiate(replacingObject);
					}

					Undo.RegisterCreatedObjectUndo(o, "created prefab");

					Transform newT = o.transform;

					if(t != null) {
						if (keepName)
							newT.name = t.name;

						if (keepIcon)
							SetIcon (newT.gameObject, GetIcon (t.gameObject));

						newT.SetParent(t.parent, false);
						newT.localPosition = t.localPosition;

						if (keepRotation)
							newT.localRotation = t.localRotation;

						if (keepScale)
							newT.localScale = t.localScale;

						replacedObjects.Add (newT.gameObject);
					}
				}

				// store replaced object transforms
				Transform[] oldTransforms = Selection.transforms;

				// Select new objects
				Selection.objects = replacedObjects.ToArray ();

				for (int i = oldTransforms.Length - 1; i >= 0; --i) {
					Undo.DestroyObjectImmediate(oldTransforms[i].gameObject);
				}

			}
		}

		// based on http://answers.unity3d.com/questions/213140/programmatically-assign-an-editor-icon-to-a-game-o.html
		Texture2D GetIcon(GameObject gameObject) {
			var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
			var args = new object[] { gameObject };
			var setIcon = typeof(EditorGUIUtility).GetMethod("GetIconForObject", flags);
			return setIcon.Invoke(null, args) as Texture2D;
		}

		void SetIcon(GameObject gameObject, Texture2D icon)
		{
			var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
			var args = new object[] { gameObject, icon };
			var setIcon = typeof(EditorGUIUtility).GetMethod("SetIconForObject", flags);
			setIcon.Invoke(null, args);
		}
	}

}
