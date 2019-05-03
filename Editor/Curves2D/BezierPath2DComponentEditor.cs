// References
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/ColliderEditorBase.cs
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/EditMode.cs
// https://answers.unity.com/questions/1396922/ugui-align-icons-in-the-inspector-how-to-recreate.html

using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine.Experimental.UIElements;

namespace CommonsHelper
{

    // Styles can only be statically constructed in non ScriptableObject class
    public class GUIData
    {
        public static class Styles
        {
            public static readonly GUIStyle singleButtonStyle = "EditModeSingleButton";
        }
    }

    [CustomEditor(typeof(BezierPath2DComponent))]
    public class BezierPath2DComponentEditor : UnityEditor.Editor
    {
        /// Session State key for toggle that is true when editing a Bezier Path 2D with custom editor input
        private const string kBezierPath2DEditActiveKey = "BezierPath2DEditActive";

        /// Padding around edit icon in toggle button
        private const float toggleButtonPaddingX = 10f;
        private const float toggleButtonPaddingY = 10f;

        /// Cached reference to Bezier path component and model
        BezierPath2DComponent script;
        BezierPath2D path;

        private void OnEnable()
        {
            script = (BezierPath2DComponent) target;
            path = script.Path;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            // Draw toggle button using the same Edit icon as Unity's native collider components
            GUIContent iconContent = EditorGUIUtility.IconContent("EditCollider");
            
            // retrieve icon dimensions and compute wanted button size
            float buttonWidth = iconContent.image.width + toggleButtonPaddingX * 2;
            float buttonHeight = iconContent.image.height + toggleButtonPaddingY * 2;
            
            // retrieve last edit state from Editor Session State
            bool editActive = SessionState.GetBool(kBezierPath2DEditActiveKey, false);
            
            EditorGUI.BeginChangeCheck();

            // Draw toggle button and label
            GUILayout.BeginHorizontal();
            
            editActive = GUILayout.Toggle(editActive, EditorGUIUtility.IconContent("EditCollider"),
                GUIData.Styles.singleButtonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight));
            GUILayout.Label("Edit Bezier Path");
            
            if (EditorGUI.EndChangeCheck())
            {
                SessionState.SetBool(kBezierPath2DEditActiveKey, editActive);
            }
            
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Add New Key Point at Origin"))
            {
                Undo.RecordObject(script, "Add Key Point");
                path.AddKeyPoint(Vector2.zero);
                SceneView.RepaintAll();
            }
        }

        void OnSceneGUI()
        {
            Undo.RecordObject(script, "Change Bezier Path");

            bool editActive = SessionState.GetBool(kBezierPath2DEditActiveKey, false);
            if (editActive)
            {
                HandleEditInput();
            }
            
            // Phase 1: draw the interpolated path to have a smooth visualization
            DrawInterpolatedPath(path);
            
            // Phase 2: draw control points to allow the user to edit them
            
            EditorGUI.BeginChangeCheck ();

            DrawControlPoints(path);

            if (EditorGUI.EndChangeCheck())
            {
            }
        }

        private void HandleEditInput()
        {
            Event guiEvent = Event.current;

            // Get unique control ID
            int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            EventType eventType = guiEvent.GetTypeForControl(controlID);

            if (eventType == EventType.Layout)
            {
                // Required to catch event and avoid selecting object under cursor
                HandleUtility.AddDefaultControl(controlID);
            }
            else if (guiEvent.shift && eventType == EventType.MouseDown && guiEvent.button == 0)
            {
                // add new key point at the end at mouse position
                Vector2 newKeyPoint = (Vector2)HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
                path.AddKeyPoint(newKeyPoint);
                
                // consume event (AddDefaultControl is necessary and sufficient, but useful if other events in same control)
                guiEvent.Use();
            }
            else if (guiEvent.control && eventType == EventType.MouseDown && guiEvent.button == 0)
            {
                // check if there are enough points for a removal
                if (path.GetKeyPointsCount() > 2)
                {
                    // remove key point the nearest to mouse position
                    Vector2 mousePoint = (Vector2)HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
                    int index = path.GetNearestKeyPointIndex(mousePoint);
                    path.RemoveKeyPoint(index);
                }
                
                // still consume event to avoid unwanted effects
                guiEvent.Use();
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
                const int maxSegmentCount = 30;  // to avoid freezing when interpolating a curve between points very far from each other
                
                // compute number of segments required to get appropriate resolution if the curve was linear
                // we know that a Bezier curve has a curviline abscissa with higher derivative, but that will be enough
                // for curves that are not too crazy i.e. control points are not too far
                Vector2[] curve = path.GetCurve(i);

                Vector2 startToEnd = curve[3] - curve[0];
                int segmentCount = Mathf.Min(Mathf.CeilToInt(startToEnd.magnitude / maxSegmentLength), maxSegmentCount);
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
            if (path.GetControlPointsCount() < 4)
            {
                // do not log warning to avoid spamming console, but this is a defect, and we will see nothing
                // just add a new point manually to trigger the safety Init()
                return;
            }

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
