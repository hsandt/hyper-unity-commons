using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{
    /// Component containing a BezierPath2D
    public class BezierPath2DComponent : MonoBehaviour
    {
        [Tooltip("Is the path relative to the game object's position?")]
        public bool isRelative = true;
        
        // Proxy methods to take world position into account if isRelative
        
        public Vector2 InterpolatePathByParameter(float t)
        {
            Vector2 offset = isRelative ? (Vector2)transform.position : Vector2.zero;
            return path.InterpolatePathByParameter(t) + offset;
        }
        public Vector2 InterpolatePathByNormalizedParameter(float normalizedT)
        {
            Vector2 offset = isRelative ? (Vector2)transform.position : Vector2.zero;
            return path.InterpolatePathByNormalizedParameter(normalizedT) + offset;
        }
        public Vector2 InterpolatePathByDistance(float distance)
        {
            Vector2 offset = isRelative ? (Vector2)transform.position : Vector2.zero;
            return path.InterpolatePathByDistance(distance) + offset;
        }
        public Vector2 InterpolatePathByNormalizedDistance(float normalizedDistance)
        {
            Vector2 offset = isRelative ? (Vector2)transform.position : Vector2.zero;
            return path.InterpolatePathByNormalizedDistance(normalizedDistance) + offset;
        }

        [SerializeField, Tooltip("Embedded Bezier Path. Coordinates are writable to allow numerical placement of " +
                                 "control points, but do not delete/duplicate points with right-click command as the " +
                                 "number of points would become invalid. Instead, use Shift+click or " +
                                 "Add New Key Point at Origin button to add a point, and Ctrl+click to remove the " +
                                 "point the closest to the cursor. If you change Control Points Size manually, " +
                                 "make sure to set a number N = 4 + 3 * n, n an integer >= 0.")]
        private BezierPath2D path = new BezierPath2D();
        public BezierPath2D Path => path;
    }
}
