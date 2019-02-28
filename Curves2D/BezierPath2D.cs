﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{

    /// A Bezier path is a series of connected Bezier curves
    [Serializable]
    public class BezierPath2D
    {
        
        /// List of control points of each successive Bezier curve, concatenated.
        /// The end of one Bezier curve is the start of the next one, so to reduce the size of the list,
        /// we consider points linking two curves only once.
        /// There is a key point every 3 control points (the path must go through each key point)
        /// Format: [keyPoint1, controlPoint1A, controlPoint1B, keyPoint2, controlPoint2A, controlPoint2B, ... keyPointN]
        [SerializeField]
        private List<Vector2> controlPoints = new List<Vector2>
        {
            // default to a kind of wave to demonstrate
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(2f, -1f),
            new Vector2(3f, 0f)
        };
    
        public BezierPath2D()
        {
        }

        // Only allow access to the points via a conservative interface.
        // This is to prevent adding and removing points manually,
        // as it would break the required format of control points.
        
        /// Return the number of control points in this path (not the number of key points).
        public int GetControlPointsCount()
        {
            return controlPoints.Count;
        }

        /// Return control point at given index
        public Vector2 GetControlPoint(int index)
        {
            Debug.AssertFormat(index >= 0 && index < controlPoints.Count, "Invalid index: {0}. Expected index between 0 and {1}", index, controlPoints.Count - 1);
            return controlPoints[index];
        }

        /// Move an existing control point at given index to given position
        public void SetControlPoint(int index, Vector2 controlPosition)
        {
            Debug.AssertFormat(index >= 0 && index < controlPoints.Count, "Invalid index: {0}. Expected index between 0 and {1}", index, controlPoints.Count - 1);
            controlPoints[index] = controlPosition;
        }

        public int GetCurvesCount()
        {
            return (controlPoints.Count - 1 ) / 3;
        }

        // Return curve at given index. A Bezier curve is a part of a Bezier path, compounded of 4 control points.
        public Vector2[] GetCurve(int index)
        {
            return new[]
            {
                controlPoints[3 * index],
                controlPoints[3 * index + 1],
                controlPoints[3 * index + 2],
                controlPoints[3 * index + 3]
            };
        }

        // Yield each curve of 4 control points compounding the path, from start to end.
        public IEnumerable<Vector2[]> GetCurves()
        {
            for (int i = 0; i < GetCurvesCount(); i++)
            {
                yield return new[]
                {
                    controlPoints[3 * i],
                    controlPoints[3 * i + 1],
                    controlPoints[3 * i + 2],
                    controlPoints[3 * i + 3]
                };
            }
        }

        
        /// Add a key point at the end of the path, automatically choosing smooth control points between the added
        /// key point and the previous one.
        public void AddKeyPoint(Vector2 newKeyPoint)
        {
            Vector2 previousControlPointB = controlPoints[controlPoints.Count - 2];
            Vector2 previousKeyPoint = controlPoints[controlPoints.Count - 1];
            Vector2 startTangent = previousKeyPoint - previousControlPointB;
                
            // mirror the position of the control point before the current last key point
            Vector2 newControlPointA = previousKeyPoint + startTangent;
            
            // mirror the first new control point to get a second one that makes the curve come back smoothly to the added key point
            // (note that if the new key point is on the opposite direction of the last tangent, it may produce a spiral pattern)
            // reflecting off a normal is the same as reflecting off a tangent and opposing
            Vector2 endTangent = VectorUtil.Mirror(startTangent, newKeyPoint - previousKeyPoint);
            Vector2 newControlPointB = newKeyPoint - endTangent;
            controlPoints.AddRange(new[] {newControlPointA, newControlPointB, newKeyPoint});
        }
    
    }

}
