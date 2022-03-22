using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonsHelper
{
    /// Component containing a BezierPath2D
    public class BezierPath2DComponent : MonoBehaviour
    {
        [Tooltip("Is the path relative to the game object's position?")]
        [FormerlySerializedAs("isRelative")]
        private bool m_IsRelative = true;
        public bool IsRelative => m_IsRelative;

        [SerializeField, Tooltip("Embedded Bezier Path. Coordinates are writable to allow numerical placement of " +
                                 "control points, but do not delete/duplicate points with right-click command as the " +
                                 "number of points would become invalid. Instead, use Shift+click or " +
                                 "Add New Key Point at Origin button to add a point, and Ctrl+click to remove the " +
                                 "point the closest to the cursor. If you change Control Points Size manually, " +
                                 "make sure to set a number N = 4 + 3 * n, n an integer >= 0.")]
        [FormerlySerializedAs("path")]
        private BezierPath2D m_Path = new BezierPath2D();
        public BezierPath2D Path => m_Path;


        /// Return a new path where each control point was offset by transform.position (as Vector2) if m_IsRelative,
        /// else preserved. Even if points are preserved, a new path is generated to avoid modifying the original one.
        public BezierPath2D GeneratePathWithIntegratedOffset()
        {
            Vector2 offset = m_IsRelative ? (Vector2)transform.position : Vector2.zero;
            return new BezierPath2D(m_Path.ControlPoints.Select(controlPoint => controlPoint + offset).ToList());
        }

        // Proxy methods to take world position into account if m_IsRelative

        public Vector2 InterpolatePathByParameter(float t)
        {
            Vector2 offset = m_IsRelative ? (Vector2)transform.position : Vector2.zero;
            return m_Path.InterpolatePathByParameter(t) + offset;
        }
        public Vector2 InterpolatePathByNormalizedParameter(float normalizedT)
        {
            Vector2 offset = m_IsRelative ? (Vector2)transform.position : Vector2.zero;
            return m_Path.InterpolatePathByNormalizedParameter(normalizedT) + offset;
        }
    }
}
