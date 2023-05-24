/* DEPENDENCIES
 *
 * GizmosUtil from Unity Commons - Helper on BitBucket
 * https://bitbucket.org/hsandt/unity-commons-helper/src/391a1541bf64a54f715aed072f3540ee77c8f6d0/Util/GizmosUtil.cs?at=master&fileviewer=file-view-default
 *
 */

using UnityEngine;
using CommonsHelper;

namespace CommonsDebug
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class EditPolygonCollider2D : MonoBehaviour
    {
#if UNITY_EDITOR
        private PolygonCollider2D m_PolygonCollider2D;

        [SerializeField, Tooltip("Should the collider be visible even when the game object is not selected? " +
            "(experimental: requires no rotation in the hierarchy and local scale only)")]
        private bool alwaysShowCollider = false;

        [SerializeField, Tooltip("Color used to always show collider")]
        private Color drawColor = Color.blue;


        void OnDrawGizmos()
        {
            if (alwaysShowCollider)
            {
                if (m_PolygonCollider2D == null)
                {
                    // lazy get component
                    m_PolygonCollider2D = GetComponent<PolygonCollider2D>();
                    
                    if (m_PolygonCollider2D == null)
                    {
                        return;
                    }
                }

                Vector2[] points = m_PolygonCollider2D.points;

                Gizmos.color = drawColor;

                // for every point (except for the last one), draw line to the next point
                for (int i = 0; i < points.Length - 1; i++)
                {
                    GizmosUtil.DrawLocalLine((Vector3) points[i], (Vector3) points[i + 1], transform);
                }

                // for polygons, close with the last segment
                GizmosUtil.DrawLocalLine((Vector3) points[points.Length - 1], (Vector3) points[0], transform);
            }
        }
#endif
    }
}