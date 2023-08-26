/* DEPENDENCIES
 *
 * GizmosUtil from Unity Commons - Helper on BitBucket
 * https://bitbucket.org/hsandt/unity-commons-helper/src/391a1541bf64a54f715aed072f3540ee77c8f6d0/Util/GizmosUtil.cs?at=master&fileviewer=file-view-default
 *
 */

using System.Collections;
using UnityEngine;

namespace HyperUnityCommons
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public class EditEdgeCollider2D : MonoBehaviour
    {
#if UNITY_EDITOR
        private EdgeCollider2D m_EdgeCollider2D;

        [SerializeField, Tooltip("Should the collider be visible even when the game object is not selected? " +
                                 "(experimental: requires no rotation in the hierarchy and local scale only)")]
        private bool alwaysShowCollider = false;

        [SerializeField, Tooltip("Color used to always show collider")]
        private Color drawColor = Color.blue;


        void OnDrawGizmos()
        {
            if (alwaysShowCollider)
            {
                if (m_EdgeCollider2D == null)
                {
                    // lazy get component
                    m_EdgeCollider2D = GetComponent<EdgeCollider2D>();

                    if (m_EdgeCollider2D == null)
                    {
                        return;
                    }
                }

                Vector2[] points = m_EdgeCollider2D.points;

                Gizmos.color = drawColor;

                // for every point (except for the last one), draw line to the next point
                for (int i = 0; i < points.Length - 1; i++)
                {
                    GizmosUtil.DrawLocalLine((Vector3) points[i], (Vector3) points[i + 1], transform);
                }
            }
        }
#endif
    }
}