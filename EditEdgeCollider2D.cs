﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(EdgeCollider2D))]
public class EditEdgeCollider2D : MonoBehaviour
{

	// public EdgeCollider2D edgeCollider2D { get { return m_EdgeCollider2D; } }
	EdgeCollider2D m_EdgeCollider2D;

	/// <summary>
	/// Should the collider be visible even when the game object is not selected? (experimental: requires no rotation in the hierarchy and local scale only)
	/// </summary>
	[SerializeField] bool alwaysShowCollider;

	/// <summary>
	/// Color used to always show collider
	/// </summary>
	[SerializeField] Color drawColor = Color.blue;

	void Awake () {
		m_EdgeCollider2D = GetComponent<EdgeCollider2D>();
	}

	void OnDrawGizmos() {
		if (alwaysShowCollider && m_EdgeCollider2D != null) {
			Vector2[] points = m_EdgeCollider2D.points;

			Gizmos.color = drawColor;

			// for every point (except for the last one), draw line to the next point
			for(int i = 0; i < points.Length-1; i++)
			{
				GizmosUtil.DrawLocalLine(transform, (Vector3) points[i], (Vector3) points[i+1]);
			}
		}
	}

}

