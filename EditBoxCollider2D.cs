/* DEPENDENCIES
 *
 * GizmosUtil from Unity Commons - Helper on BitBucket
 *
 */

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider2D))]
public class EditBoxCollider2D : MonoBehaviour
{

	// public BoxCollider2D BoxCollider2D { get { return m_BoxCollider2D; } }
	BoxCollider2D m_BoxCollider2D;

	/// <summary>
	/// Should the collider be visible even when the game object is not selected? (experimental: requires no rotation in the hierarchy and local scale only)
	/// </summary>
	[SerializeField] bool alwaysShowCollider;

	/// <summary>
	/// Color used to always show collider
	/// </summary>
	[SerializeField] Color drawColor = Color.blue;

	void Awake () {
		m_BoxCollider2D = GetComponent<BoxCollider2D>();
	}

	void OnDrawGizmos() {
		if (alwaysShowCollider) {
			GizmosUtil.DrawBounds2D(m_BoxCollider2D.bounds, drawColor);
		}
	}

}

