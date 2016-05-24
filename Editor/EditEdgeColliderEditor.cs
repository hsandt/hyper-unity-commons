// http://stackoverflow.com/questions/29819697/manually-edit-unity3d-collider-coordinates
// Raphael Marques
// adapted by hsandt

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(EditEdgeCollider2D))]
public class EditEdgeCollider2DEditor : Editor {
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		
		// get the edge collider component yourself instead of counting on a member variable of EditEdgeCollider2D storing
		// the component reference in its Awake(), since Awake may not be called before this method in the editor
		EdgeCollider2D collider = ((EditEdgeCollider2D) target).GetComponent<EdgeCollider2D>();
		
		if (collider != null) {
			var points = collider.points;
			for (int i = 0; i < points.Length; i++){
				points[i] = UnityEditor.EditorGUILayout.Vector2Field(i.ToString(), points[i]);
			}
			collider.points = points;
			UnityEditor.EditorUtility.SetDirty(collider);
		}
	}

}
