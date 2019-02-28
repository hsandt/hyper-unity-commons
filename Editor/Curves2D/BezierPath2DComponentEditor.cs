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

            Undo.RecordObject(script, "Changed Bezier Path");
            
            EditorGUI.BeginChangeCheck ();

            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                if (i > 0)
                {
                    // if not first curve, join to previous one
                    HandlesUtil.DrawLine(path.GetControlPoint(3 * i - 1), path.GetControlPoint(3 * i), Color.cyan);
                }
                
                // before C# 7.0, we cannot use ref directly with GetCurve in an iteration or as return value of a getter
                // try .NET 4.5 later
                var p1 = path.GetControlPoint(3 * i);
                HandlesUtil.DrawFreeMoveHandle(ref p1, Color.white);
                path.SetControlPoint(3 * i, p1);
                
                var p2 = path.GetControlPoint(3 * i + 1);
                HandlesUtil.DrawFreeMoveHandle(ref p2, Color.blue);
                path.SetControlPoint(3 * i + 1, p2);
                
                var p3 = path.GetControlPoint(3 * i + 2);
                HandlesUtil.DrawFreeMoveHandle(ref p3, Color.blue);
                path.SetControlPoint(3 * i + 2, p3);

                HandlesUtil.DrawPolyLine2D(new []{ p1, p2, p3 }, Color.cyan);
            }
            
            // last point
            int lastIndex = path.GetControlPointsCount() - 1;
            var lastPoint = path.GetControlPoint(lastIndex);
            HandlesUtil.DrawFreeMoveHandle(ref lastPoint, Color.white);
            path.SetControlPoint(lastIndex, lastPoint);
            
            var beforeLastPoint = path.GetControlPoint(lastIndex - 1);
            HandlesUtil.DrawLine(beforeLastPoint, lastPoint, Color.cyan);

            if (EditorGUI.EndChangeCheck())
            {
            }
        }
    }

}
