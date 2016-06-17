using UnityEngine;
using UnityEditor;

public class ShowWorldPosition {

	[DrawGizmo(GizmoType.Selected)]
	static void DrawTransformWorldPosition(Transform transform, GizmoType gizmoType)
	{
		Handles.Label(transform.position, transform.position.ToString());
	}

}
