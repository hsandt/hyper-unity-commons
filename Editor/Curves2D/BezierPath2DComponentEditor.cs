using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper
{

    [CustomEditor(typeof(BezierPath2DComponent))]
    public class BezierPath2DComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var script = (BezierPath2DComponent) target;
            BezierPath2D path = script.Path;

            if (path == null)
            {
                // this can only happen just when adding the component, as it seems not to have been serialized yet
                return;
            }
            
            if (GUILayout.Button("Add New Key Point at Origin"))
            {
                path.AddKeyPoint(Vector2.zero);
            }
        }

        void OnSceneGUI()
        {
            var script = (BezierPath2DComponent) target;
            BezierPath2D path = script.Path;

            if (path == null)
            {
                // this can only happen just when adding the component, as it seems not to have been serialized yet
                return;
            }

            // Phase 1: draw the interpolated path to have a smooth visualization
            DrawInterpolatedPath(path);
            
            // Phase 2: draw control points to allow the user to edit them
            
            Undo.RecordObject(script, "Changed Bezier Path");
            EditorGUI.BeginChangeCheck ();

            DrawControlPoints(path);

            if (EditorGUI.EndChangeCheck())
            {
            }
        }

        /// Draw the interpolated path, without control points
        private static void DrawInterpolatedPath(BezierPath2D path)
        {
            List<Vector2> interpolatedPath = InterpolatePath(path);
            HandlesUtil.DrawPolyLine2D(interpolatedPath.ToArray(), Color.cyan);
        }

        /// Return a list of interpolated points over the given path
        private static List<Vector2> InterpolatePath(BezierPath2D path)
        {
            // interpolate each curve, and concatenate all of them into a smooth path
            List<Vector2> interpolatedPath = new List<Vector2>();

            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                // TODO: camera pixel size
                const float maxSegmentLength = 0.1f;  // in world units
                // compute number of segments required to get appropriate resolution if the curve was linear
                // we know that a Bezier curve has a curviline abscissa with higher derivative, but that will be enough
                // for curves that are not too crazy i.e. control points are not too far
                Vector2[] curve = path.GetCurve(i);

                Vector2 startToEnd = curve[3] - curve[0];
                int segmentCount = Mathf.CeilToInt(startToEnd.magnitude / maxSegmentLength);
                for (int j = 0; j < segmentCount; ++j)
                {
                    float t = (float)j / (float)segmentCount;
                    interpolatedPath.Add(BezierPath2D.InterpolateBezier(curve[0], curve[1], curve[2], curve[3], t));
                }
            }
            
            // last point
            int lastIndex = path.GetControlPointsCount() - 1;
            Vector2 lastPoint = path.GetControlPoint(lastIndex);
            interpolatedPath.Add(lastPoint);

            return interpolatedPath;
        }
        
        /// Draw the control points of the given path, with segments between key points and non-key control points
        /// to visualize tangents
        private static void DrawControlPoints(BezierPath2D path)
        {
            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                // before C# 7.0, we cannot use ref directly with GetCurve in an iteration or as return value of a getter
                // try .NET 4.5 later
                var p0 = path.GetControlPoint(3 * i);
                HandlesUtil.DrawFreeMoveHandle(ref p0, Color.white);
                path.SetControlPoint(3 * i, p0);
                
                var p1 = path.GetControlPoint(3 * i + 1);
                HandlesUtil.DrawFreeMoveHandle(ref p1, ColorUtil.orange);
                path.SetControlPoint(3 * i + 1, p1);

                // draw curve starting tangent
                HandlesUtil.DrawLine(p0, p1, Color.yellow);
                
                var p2 = path.GetControlPoint(3 * i + 2);
                HandlesUtil.DrawFreeMoveHandle(ref p2, ColorUtil.orange);
                path.SetControlPoint(3 * i + 2, p2);
                
                // draw curve ending tangent
                var p3 = path.GetControlPoint(3 * i + 3);
                HandlesUtil.DrawLine(p2, p3, Color.yellow);
            }

            // last point
            int lastIndex = path.GetControlPointsCount() - 1;
            var lastPoint = path.GetControlPoint(lastIndex);
            HandlesUtil.DrawFreeMoveHandle(ref lastPoint, Color.white);
            path.SetControlPoint(lastIndex, lastPoint);
            
            // draw last tangent
            var beforeLastPoint = path.GetControlPoint(lastIndex - 1);
            HandlesUtil.DrawLine(beforeLastPoint, lastPoint, Color.yellow);
        }
        
    }

}
