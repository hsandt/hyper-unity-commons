using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace HyperUnityCommons
{
    /// Component containing a BezierPath2D
    public class BezierPath2DComponent : Path2DComponent
    {
        [SerializeField, Tooltip("Embedded Bezier Path. Coordinates are writable to allow numerical placement of " +
             "control points, but do not add/delete/duplicate points with +/- button or right-click command as " +
             "the number of points would become invalid. Instead, use either the Add New Key Point at Origin button " +
             "to add a point, or edit the path visually (see tooltip by hovering Edit Path button). " +
             "If you change Control Points Size manually, make sure to set a number N = 4 + 3 * n, n an integer >= 0.")]
        [FormerlySerializedAs("path")]
        private BezierPath2D m_Path = new BezierPath2D();
        public BezierPath2D BezierPath => m_Path;

        public override Path2D Path => m_Path;


        /// Return a new path where each control point was offset by transform.position (as Vector2) if m_IsRelative,
        /// else preserved. Even if points are preserved, a new path is generated to avoid modifying the original one.
        public BezierPath2D GeneratePathWithIntegratedOffset()
        {
            Vector2 offset = m_IsRelative ? (Vector2)transform.position : Vector2.zero;
            return new BezierPath2D(m_Path.ControlPoints.Select(controlPoint => controlPoint + offset).ToList());
        }
    }
}
