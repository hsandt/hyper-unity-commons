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
    [RequireComponent(typeof(BoxCollider2D))]
    public class EditBoxCollider2D : MonoBehaviour
    {
#if UNITY_EDITOR
        private BoxCollider2D m_BoxCollider2D;

        [SerializeField, Tooltip("Should the collider be visible even when the game object is not selected? " +
                                 "(experimental: requires no rotation in the hierarchy and local scale only)")]
        private bool alwaysShowCollider = false;

        [SerializeField, Tooltip("Color used to always show collider")]
        private Color drawColor = Color.blue;


        void OnDrawGizmos()
        {
            if (alwaysShowCollider)
            {
                if (m_BoxCollider2D == null)
                {
                    // lazy get component
                    m_BoxCollider2D = GetComponent<BoxCollider2D>();

                    if (m_BoxCollider2D == null)
                    {
                        return;
                    }
                }

                Gizmos.color = drawColor;
                GizmosUtil.DrawLocalBox2D(m_BoxCollider2D.offset, m_BoxCollider2D.size, transform);
            }
        }
#endif
    }
}