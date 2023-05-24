using UnityEngine;
using UnityEditor;

namespace HyperUnityCommons.Editor
{

	public static class ShowWorldPosition {

		[DrawGizmo(GizmoType.Selected)]
		static void DrawTransformWorldPosition(Transform transform, GizmoType gizmoType)
		{
			Handles.Label(transform.position, transform.position.ToString());
		}

	}

}
