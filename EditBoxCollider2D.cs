/* DEPENDENCIES
 *
 * GizmosUtil from Unity Commons - Helper on BitBucket
 * https://bitbucket.org/hsandt/unity-commons-helper/src/391a1541bf64a54f715aed072f3540ee77c8f6d0/Util/GizmosUtil.cs?at=master&fileviewer=file-view-default
 *
 */

using UnityEngine;
using System.Collections;

namespace Commons.Debug
{
    
	[ExecuteInEditMode]
	[RequireComponent(typeof(BoxCollider2D))]
	public class EditBoxCollider2D : MonoBehaviour
	{
		#if UNITY_EDITOR
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
				if (m_BoxCollider2D == null) {
					// component is serialized so reference is not lost on compilation, but the component may have been removed
					// and another collider component may have been added, so check for it
					m_BoxCollider2D = GetComponent<BoxCollider2D>();
					if (m_BoxCollider2D == null)
						return;
				}

				Gizmos.color = drawColor;
				GizmosUtil.DrawLocalBox2D(m_BoxCollider2D.offset, m_BoxCollider2D.size, transform);
			}
		}
		#endif
	}

}
