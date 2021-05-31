// From http://stackoverflow.com/questions/29819697/manually-edit-unity3d-collider-coordinates
// Raphael Marques
// adapted by Long Nguyen Huu (hsandt)

using UnityEngine;
using UnityEditor;
using System.Collections;

using CommonsHelper;

namespace CommonsDebug.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(EditPolygonCollider2D))]
	public class EditPolygonCollider2DEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Round all coordinates to 1/16 px"))
			{
				var script = (EditPolygonCollider2D) target;
				PolygonCollider2D collider = script.GetComponent<PolygonCollider2D>();
				
				if (collider != null)
				{
					Undo.RecordObject(collider, "Snap polygon collider 2D coordinates to 1/16 px");
					
					// .points return a temporary array copy, so we can work on it,
					// but we must re-assign it to collider.points at the end
					Vector2[] points = collider.points;
					
					for (int i = 0; i < points.Length; i++)
					{
						points[i] = VectorUtil.RoundVector2(points[i], 1f/16f);
					}
					
					collider.points = points;
				}
			}
			
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
