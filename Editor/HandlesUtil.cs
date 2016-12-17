using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class HandlesUtil {

	const float handleSize = 0.1f;
	static Handles.DrawCapFunction handleCap = Handles.CubeCap;

	public static void DrawRect (ref Rect rect, Transform owner) {

		Color oldColor = Handles.color;
		// if the rectangle is reversed, change the color to notify the user
		if (rect.width >= 0 && rect.height >= 0)
			Handles.color = Color.white;
		else if (rect.width < 0 && rect.height >= 0)
			Handles.color = Color.yellow;
		else if (rect.width >= 0)
			Handles.color = Color.yellow;
		else
			Handles.color = Color.red;

		Matrix4x4 oldMatrix = Handles.matrix;

		// only use the local matrix if scale is valid (no null coordinates)
		// else, only consider position and rotation to avoid producing NaN values
		if (owner.lossyScale.x != 0 && owner.lossyScale.y != 0 && owner.lossyScale.z != 0)
			Handles.matrix = owner.localToWorldMatrix;
		else {
			Handles.matrix = Matrix4x4.TRS(owner.position, owner.rotation, Vector3.one);
		}

		// Draw rect edges
		var points = new Vector3[] {
			new Vector3(rect.xMin, rect.yMin),
			new Vector3(rect.xMax, rect.yMin),
			new Vector3(rect.xMax, rect.yMax),
			new Vector3(rect.xMin, rect.yMax),
			new Vector3(rect.xMin, rect.yMin)
		};
		Handles.DrawPolyLine(points);

		// Prepare temporary vector for the 9 handles
		Vector2 tempVec;

		// Draw center handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(rect.center);
		if (EditorGUI.EndChangeCheck ()) {
			rect.center = tempVec;
		}

		// Draw left handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.center.y));
		if (EditorGUI.EndChangeCheck ()) {
			rect.xMin = tempVec.x;
		}

		// Draw right handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.center.y));
		if (EditorGUI.EndChangeCheck ()) {
			rect.xMax = tempVec.x;
		}

		// Draw bottom handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.center.x, rect.yMin));
		if (EditorGUI.EndChangeCheck ()) {
			rect.yMin = tempVec.y;
		}

		// Draw top handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.center.x, rect.yMax));
		if (EditorGUI.EndChangeCheck ()) {
			rect.yMax = tempVec.y;
		}

		// Draw bottom-left handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.yMin));
		if (EditorGUI.EndChangeCheck ()) {
			rect.min = tempVec;
		}

		// Draw top-left handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.xMin, rect.yMax));
		if (EditorGUI.EndChangeCheck ()) {
			rect.xMin = tempVec.x;
			rect.yMax = tempVec.y;
		}

		// Draw bottom-right handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.yMin));
		if (EditorGUI.EndChangeCheck ()) {
			rect.xMax = tempVec.x;
			rect.yMin = tempVec.y;
		}

		// Draw top-right handle
		EditorGUI.BeginChangeCheck ();
		tempVec = DrawFreeMoveHandle(new Vector3(rect.xMax, rect.yMax));
		if (EditorGUI.EndChangeCheck ()) {
			rect.xMax = tempVec.x;
			rect.yMax = tempVec.y;
		}

		Handles.matrix = oldMatrix;
		Handles.color = oldColor;
	}

	static Vector2 DrawFreeMoveHandle (Vector2 pos) {
		return (Vector2) Handles.FreeMoveHandle ((Vector3) pos, Quaternion.identity,
			HandleUtility.GetHandleSize ((Vector3) pos) * handleSize, Vector3.zero, handleCap);
	}

}
