using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonsHelper
{

    /// A Bezier path is a series of connected Bezier curves
    [Serializable]
    public class BezierPath2D
    {
        /// Compute point on 2D Bezier curve of control points p0, p1, p2, p3 at parameter t
        public static Vector2 InterpolateBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return (1-t) * (1-t) * (1-t) * p0 + 3 * (1-t) * (1-t) * t * p1 + 3 * (1-t) * t * t * p2 + t * t * t * p3;
        }

        /// Compute point on 2D Bezier curve (array of 4 points) at parameter t
        public static Vector2 InterpolateBezier(Vector2[] curve, float t)
        {
            return InterpolateBezier(curve[0], curve[1], curve[2], curve[3], t);
        }


        /// List of control points of each successive Bezier curve, concatenated.
        /// The end of one Bezier curve is the start of the next one, so to reduce the size of the list,
        /// we consider points linking two curves only once.
        /// There is a key point every 3 control points (the path must go through each key point)
        /// We use `indices` for flat iterations, and `key indices` for iteration on key points only.
        /// Format: [keyPoint1, outTangentPoint1, inTangentPoint2, keyPoint2, outTangentPoint2, ... inTangentPointN, keyPointN]
        /// We assume it always has at least 4 points.
        [SerializeField]
        [FormerlySerializedAs("controlPoints")]
        private List<Vector2> m_ControlPoints;
        public ReadOnlyCollection<Vector2> ControlPoints => m_ControlPoints.AsReadOnly();


        public BezierPath2D()
        {
            Init();
        }

        public BezierPath2D(IReadOnlyCollection<Vector2> controlPoints)
        {
            if ((controlPoints.Count - 1) % 3 == 0)
            {
                m_ControlPoints = controlPoints.ToList();
            }
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogErrorFormat("[BezierPath2D] Constructor taking controlPoints is used, " +
                    "but controlPoints.Count is {0}, which is not in the form 3*n + 1. " +
                    "Falling back to default initialization.",
                    controlPoints.Count);
                Init();
            }
            #endif
        }

        private void Init()
        {
            // default to a kind of wave to demonstrate
            m_ControlPoints = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(2f, -1f),
                new Vector2(3f, 0f)
            };
        }

        #region PointAndCurveAccessors

        // Only allow access to the points via a conservative interface.
        // This is to prevent adding and removing points manually,
        // as it would break the required format of control points.

        /// Return the number of control points in this path (not the number of key points).
        public int GetControlPointsCount()
        {
            return m_ControlPoints.Count;
        }

        /// Return control point at given index
        public Vector2 GetControlPoint(int index)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(index >= 0 && index < m_ControlPoints.Count, "Invalid index: {0}. Expected index between 0 and {1}", index, m_ControlPoints.Count - 1);
            #endif

            return m_ControlPoints[index];
        }

        /// Move an existing control point at given index to given position
        public void SetControlPoint(int index, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(index >= 0 && index < m_ControlPoints.Count, "Invalid index: {0}. Expected index between 0 and {1}", index, m_ControlPoints.Count - 1);
            #endif

            m_ControlPoints[index] = controlPosition;
        }

        /// Return the number of key points (equal to the number of curves + 1)
        public int GetKeyPointsCount()
        {
            return GetCurvesCount() + 1;
        }

        /// Return key point at given key index
        public Vector2 GetKeyPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            return m_ControlPoints[3 * keyIndex];
        }

        /// Move key point at given key index to given position
        public void SetKeyPoint(int keyIndex, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            m_ControlPoints[3 * keyIndex] = controlPosition;
        }

        /// Yield all key points
        public IEnumerable<Vector2> GetKeyPoints()
        {
            int keyPointsCount = GetKeyPointsCount();
            for (int keyIndex = 0; keyIndex < keyPointsCount; keyIndex++)
            {
                yield return GetKeyPoint(keyIndex);
            }
        }

        /// Return the index of the key point the nearest to passed position
        public int GetNearestKeyPointIndex(Vector2 position)
        {
            int nearestKeyPointIndex = -1;
            float nearestKeyPointDistance = float.MaxValue;

            int keyPointsCount = GetKeyPointsCount();
            for (int keyIndex = 0; keyIndex < keyPointsCount; keyIndex++)
            {
                float distance = Vector2.SqrMagnitude(GetKeyPoint(keyIndex) - position);
                if (distance < nearestKeyPointDistance)
                {
                    nearestKeyPointIndex = keyIndex;
                    nearestKeyPointDistance = distance;
                }
            }

            return nearestKeyPointIndex;
        }

        /// Add a key point at the end of the path, automatically choosing smooth control points between the added
        /// key point and the previous one.
        public void AddKeyPoint(Vector2 newKeyPoint)
        {
            if (m_ControlPoints.Count < 4)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarningFormat("Invalid initial state: only {0} points, expected at least 4. Reinitializing points.", m_ControlPoints.Count);
                #endif

                Init();
            }

            Vector2 previousControlPointB = m_ControlPoints[m_ControlPoints.Count - 2];
            Vector2 previousKeyPoint = m_ControlPoints[m_ControlPoints.Count - 1];
            Vector2 startTangent = previousKeyPoint - previousControlPointB;
            if (startTangent.sqrMagnitude < Mathf.Epsilon)
            {
                // fallback to keep control point visible
                startTangent = Vector2.up;
            }

            // mirror the position of the control point before the current last key point
            Vector2 newControlPointA = previousKeyPoint + startTangent;

            // mirror the first new control point to get a second one that makes the curve come back smoothly to the added key point
            // (note that if the new key point is on the opposite direction of the last tangent, it may produce a spiral pattern)
            // reflecting off a normal is the same as reflecting off a tangent and opposing

            Vector2 previousToNewVector = newKeyPoint - previousKeyPoint;
            if (previousToNewVector.sqrMagnitude < Mathf.Epsilon)
            {
                // fallback to allow mirroring
                previousToNewVector = Vector2.right;
            }

            Vector2 endTangent = VectorUtil.Mirror(startTangent, previousToNewVector);
            Vector2 newControlPointB = newKeyPoint - endTangent;
            m_ControlPoints.AddRange(new[] {newControlPointA, newControlPointB, newKeyPoint});
        }

        /// Remove key point at key index, also removing the surrounding tangent points (1 for the start and end point,
        /// 2 for a middle point).
        /// UB unless there are at least 3 key points, and the keyIndex is a valid key point index.
        public void RemoveKeyPoint(int keyIndex)
        {
            int keyPointsCount = GetKeyPointsCount();

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyPointsCount > 2, "There are only {0} key points, cannot remove one more key point.", keyPointsCount);
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < keyPointsCount, "Invalid key index: {0}. Expected index between 0 and {1}.", keyIndex, keyPointsCount - 1);
            #endif

            if (keyIndex == 0)
            {
                // remove start point, its out tangent point and the 2nd point's in tangent point
                m_ControlPoints.RemoveRange(0, 3);
            }
            else if (keyIndex == keyPointsCount - 1)
            {
                // remove end point, its in tangent point and the penultimate point's out tangent point
                m_ControlPoints.RemoveRange(3 * keyIndex - 2, 3);
            }
            else
            {
                m_ControlPoints.RemoveRange(3 * keyIndex - 1, 3);
            }
        }

        /// Return in tangent control point at given key index
        /// Key index must be between 1 and key points count - 1
        public Vector2 GetInTangentPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 1 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 1 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            return m_ControlPoints[3 * keyIndex - 1];
        }

        /// Move in tangent control point at given key index to given position
        /// Key index must be between 1 and key points count - 1
        public void SetInTangentPoint(int keyIndex, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 1 && keyIndex < GetKeyPointsCount(), "Invalid keyIndex: {0}. Expected keyIndex between 1 and Key Points Count - 1 ({1})", keyIndex, GetKeyPointsCount() - 1);
            #endif

            m_ControlPoints[3 * keyIndex - 1] = controlPosition;
        }

        /// Return out tangent control point at given key index
        /// Key index must be between 0 and key points count - 2
        public Vector2 GetOutTangentPoint(int keyIndex)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount() - 1, "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 2 ({1})", keyIndex, GetKeyPointsCount() - 2);
            #endif

            return m_ControlPoints[3 * keyIndex + 1];
        }

        /// Move out tangent control point at given key index to given position
        /// Key index must be between 1 and key points count - 1
        public void SetOutTangentPoint(int keyIndex, Vector2 controlPosition)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < GetKeyPointsCount() - 1, "Invalid keyIndex: {0}. Expected keyIndex between 0 and Key Points Count - 2 ({1})", keyIndex, GetKeyPointsCount() - 2);
            #endif

            m_ControlPoints[3 * keyIndex + 1] = controlPosition;
        }

        /// Return the number of curves compounding this path (equal to the number of key points - 1)
        public int GetCurvesCount()
        {
            return (m_ControlPoints.Count - 1 ) / 3;
        }

        /// Return curve at given key index. A Bezier curve is a part of a Bezier path, compounded of 4 control points.
        public Vector2[] GetCurve(int keyIndex)
        {
            return new[]
            {
                m_ControlPoints[3 * keyIndex],
                m_ControlPoints[3 * keyIndex + 1],
                m_ControlPoints[3 * keyIndex + 2],
                m_ControlPoints[3 * keyIndex + 3]
            };
        }

        /// Yield each curve of 4 control points compounding the path, from start to end.
        public IEnumerable<Vector2[]> GetCurves()
        {
            int curvesCount = GetCurvesCount();
            for (int keyIndex = 0; keyIndex < curvesCount; keyIndex++)
            {
                yield return GetCurve(keyIndex);
            }
        }

        #endregion

        #region Interpolation

        /// Return the position on the whole path at given cumulated parameter
        /// (0 for start, 1 for end of 1st curve, N for end of N-th curve)
        public Vector2 InterpolatePathByParameter(float pathT)
        {
            int curvesCount = GetCurvesCount();

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(pathT >= 0f && pathT <= (float) curvesCount);
            #endif

            // handle edge cases (including cases that would normally assert)
            if (pathT <= 0)
            {
                return m_ControlPoints[0];
            }
            if (pathT >= (float) curvesCount)
            {
                return m_ControlPoints[^1];
            }

            int keyIndex = Mathf.FloorToInt(pathT);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // we have handled normalizedT == 1 case, so we cannot be right at the end
            // and the keyIndex should never reach curvesCount
            Debug.Assert(keyIndex < curvesCount);
            #endif

            float remainder = pathT - keyIndex;  // or pathT % 1f;
            return InterpolateCurve(keyIndex, remainder);
        }

        /// Return the position on the whole path at given normalized parameter
        /// (0 for start, 1 for end)
        /// Each curve is associated an equal range of values (1 / curvesCount),
        /// so if normalizedT is increased at constant rate, the interpolated point
        /// "spends" the same amount of time in every curve.
        /// However, its speed may still vary inside a curve (due to Bezier derivative
        /// not being constant) and between curves (longer curves will lead to faster
        /// progression in average).
        public Vector2 InterpolatePathByNormalizedParameter(float normalizedT)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(normalizedT >= 0f && normalizedT <= 1f);
            #endif

            int curvesCount = GetCurvesCount();
            float pathT = normalizedT * curvesCount;
            return InterpolatePathByParameter(pathT);
        }

        /// Return the position on the whole path at given distance from the start,
        /// using arc-length parameterization
        public Vector2 InterpolatePathByDistance(float distance)
        {
            // TODO: split path in segments, accumulate segment lengths and evaluate point at given curvilinear abscissa
            return Vector2.zero;
        }

        /// Return the position on the whole path at given distance ratio (on total path length) from the start,
        /// using arc-length normalized parameterization
        public Vector2 InterpolatePathByNormalizedDistance(float normalizedDistance)
        {
            // TODO: split path in segments, accumulate segment lengths and evaluate point at given curvilinear abscissa
            return Vector2.zero;
        }

        /// Return an evaluation of the length, as the sum of evaluated curve lengths,
        /// with [segmentsCountPerCurve] segments approximation per curve
        public float EvaluateLength(int segmentsCountPerCurve)
        {
            int curvesCount = GetCurvesCount();

            float length = 0f;

            for (int keyIndex = 0; keyIndex < curvesCount; ++keyIndex)
            {
                length += EvaluateCurveLength(keyIndex, segmentsCountPerCurve);
            }

            return length;
        }

        /// Return an evaluation of the length of curve of index i, as the sum of [segmentsCount] segment approximation
        /// lengths
        public float EvaluateCurveLength(int keyIndex, int segmentsCount)
        {
            Vector2[] curve = GetCurve(keyIndex);
            return EvaluateCurveLength(curve, segmentsCount);
        }

        /// Return an evaluation of the passed curve (array of points), as the sum of [segmentsCount] segment approximation
        /// lengths
        public static float EvaluateCurveLength(Vector2[] curve, int segmentsCount)
        {
            // store end point of the previous iteration to reuse it as start point for the next one
            Vector2 previousPoint = curve[0];

            // initialize total length to return
            float length = 0f;

            for (int i = 0; i < segmentsCount; i++)
            {
                // compute segment end point parameter on curve
                // this is the next point after current i, hence i + 1
                float nextT = (float) (i + 1) / (float) segmentsCount;

                // locate segment end and add segment length to total length
                Vector2 nextPoint = InterpolateBezier(curve, nextT);
                length += Vector2.Distance(previousPoint, nextPoint);

                // update previous point for next iteration
                previousPoint = nextPoint;
            }

            // return sum of all segment lengths
            return length;
        }

        /// Return the position of an interpolated point on curve of index i at parameter t
        public Vector2 InterpolateCurve(int keyIndex, float t)
        {
            int curvesCount = GetCurvesCount();

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(keyIndex >= 0 && keyIndex < curvesCount, "Invalid curve index: {0}. Expected index between 0 and {1}.", keyIndex, curvesCount - 1);
            Debug.AssertFormat(0f <= t && t <= 1f, "Parameter t is {0}, expected number between 0 and 1.", t);
            #endif

            Vector2[] curve = GetCurve(keyIndex);
            return InterpolateBezier(curve, t);
        }

        #endregion
    }
}
