// Requirement: GizmosUtil from Unity Commons - Helper on BitBucket

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
public class EditPolygonCollider2D : MonoBehaviour
{

	// public PolygonCollider2D polygonCollider2D { get { return m_PolygonCollider2D; } }
	PolygonCollider2D m_PolygonCollider2D;

	/// <summary>
	/// Should the collider be visible even when the game object is not selected? (experimental: requires no rotation in the hierarchy and local scale only)
	/// </summary>
	[SerializeField] bool alwaysShowCollider;

	/// <summary>
	/// Color used to always show collider
	/// </summary>
	[SerializeField] Color drawColor = Color.blue;

	void Awake () {
		m_PolygonCollider2D = GetComponent<PolygonCollider2D>();
	}

	void OnDrawGizmos() {
		if (alwaysShowCollider) {
			Vector2[] points = m_PolygonCollider2D.points;
			
			Gizmos.color = drawColor;

			// for every point (except for the last one), draw line to the next point
			for(int i = 0; i < points.Length-1; i++)
			{
				GizmosUtil.DrawLocalLine(transform, (Vector3) points[i], (Vector3) points[i+1], drawColor);
			}
			// for polygons, close with the last segment
			GizmosUtil.DrawLocalLine(transform, (Vector3) points[points.Length - 1], (Vector3) points[0]);
		}
	}

}

