using UnityEngine;
using System.Collections;

public static class GizmosUtil {
	
	/// <summary>
	/// Draw a line with local coordinates, with current gizmos parameters
	/// </summary>
	/// <param name="p1">Local 1st coordinates of the line.</param>
	/// <param name="p2">Local 2nt coordinates of the line.</param>
	public static void DrawLocalLine (Transform tr, Vector3 p1, Vector3 p2) {
		Gizmos.DrawLine(tr.TransformPoint(p1), tr.TransformPoint(p2));
	}

}
