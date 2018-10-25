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

		[MenuItem ("Tools/ReplaceGameObjects %g")]
		public static void Init () {
			EditorWindow.GetWindow<ReplaceGameObjects> (false, "Replace Game Objects", true);
		}

		void OnGUI () {
			scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);

			GUILayout.Label ("Use Object", EditorStyles.boldLabel);

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

					GameObject o = null;
					o = PrefabUtility.GetCorrespondingObjectFromSource(replacingObject) as GameObject;

					if (PrefabUtility.GetPrefabType(replacingObject).ToString() == "PrefabInstance") {
						o = (GameObject)PrefabUtility.InstantiatePrefab(o);
						PrefabUtility.SetPropertyModifications(o, PrefabUtility.GetPropertyModifications(replacingObject));
					}

					else if (PrefabUtility.GetPrefabType(replacingObject).ToString() == "Prefab") {
						o = (GameObject)PrefabUtility.InstantiatePrefab(replacingObject);
					}

					else {
						o = Instantiate(replacingObject) as GameObject;
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
