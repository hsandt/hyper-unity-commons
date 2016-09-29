// from http://forum.unity3d.com/threads/replace-game-object-with-prefab.24311/
// by Elecman, modified by huulong

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReplaceGameObjects : EditorWindow
{
	GameObject myObject;
	bool keepRotation;
	bool keepScale;

	[MenuItem ("Tools/ReplaceGameObjects %g")]

	public static void Init() {
		EditorWindow.GetWindow(typeof(ReplaceGameObjects));
	}        

	void OnGUI () {

		GUILayout.Label ("Use Object", EditorStyles.boldLabel);

		myObject = EditorGUILayout.ObjectField(myObject, typeof(GameObject), true) as GameObject;

		keepRotation = GUILayout.Toggle(keepRotation, "Keep Rotation");
		keepScale = GUILayout.Toggle(keepScale, "Keep Scale");

		if (GUILayout.Button ("Replace Selected")) {            

			if (myObject != null) {

				List<GameObject> newGameObjects = new List<GameObject>();
				GameObject[] gameObjectsToReplace = Selection.gameObjects;

				foreach (Transform t in Selection.transforms) {

					GameObject o = null;
					o = PrefabUtility.GetPrefabParent(myObject) as GameObject;                    

					if (PrefabUtility.GetPrefabType(myObject).ToString() == "PrefabInstance") {

						o = (GameObject)PrefabUtility.InstantiatePrefab(o);
						PrefabUtility.SetPropertyModifications(o, PrefabUtility.GetPropertyModifications(myObject));
					}                    

					else if (PrefabUtility.GetPrefabType(myObject).ToString() == "Prefab") {

						o = (GameObject)PrefabUtility.InstantiatePrefab(myObject);
					}                    

					else {

						o = Instantiate(myObject) as GameObject;
					}

					newGameObjects.Add(o);        

					Undo.RegisterCreatedObjectUndo(o, "created prefab");

					Transform newT = o.transform;

					if(t != null){                     
						newT.SetParent(t.parent, false);
						newT.localPosition = t.localPosition;
						if (keepRotation)
							newT.localRotation = t.localRotation;
						if (keepScale)
							newT.localScale = t.localScale;
						newT.parent = t.parent;

					}
				}

				Selection.objects = newGameObjects.ToArray();

				foreach (GameObject go in gameObjectsToReplace) {

					Undo.DestroyObjectImmediate(go);
				}

			}
		}
	}
}
