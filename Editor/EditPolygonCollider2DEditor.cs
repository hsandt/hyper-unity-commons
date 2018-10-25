// From http://stackoverflow.com/questions/29819697/manually-edit-unity3d-collider-coordinates
// Raphael Marques
// adapted by Long Nguyen Huu (hsandt)

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CommonsDebug
{

	[CanEditMultipleObjects]
	[CustomEditor(typeof(EditPolygonCollider2D))]
	public class EditPolygonCollider2DEditor : Editor {
	
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			/*
			 * This custom inspector is now obsolete in Unity 5.4 where coordinates can be manually edited in the main component, in Normal view
			 * I may restore this code if I add something to make it better than the native Unity coordinate editor, such as +/- buttons to insert and remove points

			// Get the edge collider component directly instead of depending on a member variable storing
			// the component reference, since Awake() may not be called before this method in the editor
			PolygonCollider2D collider = ((EditPolygonCollider2D) target).GetComponent<PolygonCollider2D>();

			if (collider != null) {
				var points = collider.points;
				for (int i = 0; i < points.Length; i++){
					points[i] = UnityEditor.EditorGUILayout.Vector2Field(i.ToString(), points[i]);
				}
				collider.points = points;
				EditorUtility.SetDirty(collider);
			}

			*/
		}

	}

}
