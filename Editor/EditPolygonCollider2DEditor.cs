// http://stackoverflow.com/questions/29819697/manually-edit-unity3d-collider-coordinates
// Raphael Marques
// adapted by hsandt

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(EditPolygonCollider2D))]
public class EditPolygonCollider2DEditor : Editor {
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		PolygonCollider2D collider = ((EditPolygonCollider2D) target).polygonCollider2D;
		var points = collider.points;
		for (int i = 0; i < points.Length; i++){
			points[i] = UnityEditor.EditorGUILayout.Vector2Field(i.ToString(), points[i]);
		}
		collider.points = points;
		UnityEditor.EditorUtility.SetDirty(collider);
	}

}
