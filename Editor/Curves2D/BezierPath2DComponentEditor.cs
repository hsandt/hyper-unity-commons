// References
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/ColliderEditorBase.cs
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/EditMode.cs
// https://answers.unity.com/questions/1396922/ugui-align-icons-in-the-inspector-how-to-recreate.html

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
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
        
        private static readonly Color pathColor = Color.cyan;
        private static readonly Color keyPointColor = Color.white;
        private static readonly Color tangentPointColor = ColorUtil.orange;
        private static readonly Color tangentColor = Color.yellow;

        /// Cached reference to Bezier path component and model
        BezierPath2DComponent script;

        BezierPath2D path;

        private void OnEnable ()
        {
            script = (BezierPath2DComponent) target;
            path = script.Path;
        }

        public override void OnInspectorGUI ()
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
                // repaint so the controls appear/disappear immediately in Scene View
                SceneView.RepaintAll();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Add New Key Point at Origin"))
            {
                Undo.RecordObject(script, "Add Key Point");
                path.AddKeyPoint(Vector2.zero);
                SceneView.RepaintAll();
            }
        }

        void OnSceneGUI ()
        {
            DrawEditablePath(script.isRelative);
        }

        private void DrawEditablePath (bool isRelative)
        {
            bool editActive = SessionState.GetBool(kBezierPath2DEditActiveKey, false);
            Vector2 offset = isRelative ? (Vector2)script.transform.position : Vector2.zero;

            if (editActive)
            {
                Undo.RecordObject(script, "Change Bezier Path");

                // Phase 1: in edit mode only, draw control points to allow the user to edit them
                // The only reason we do that before HandleEditInput is to allow editing the control points
                // while detecting add/remove point input (as it uses a custom control ID and consumes all other events)
                DrawControlPoints(path, offset);
                
                // Phase 2: in edit mode only, handle add/remove point input
                HandleEditInput(offset);
            }

            // Phase 3: draw the interpolated path to have a smooth visualization
            DrawInterpolatedPath(path, offset);
        }

        private void HandleEditInput (Vector2 offset)
        {
            // Known issue: when edit is active, user cannot click on other objects even when not holding modifier key
            // Check PolygonCollider2DTool.cs and PolygonCollider2DEditor.cs in Unity repository for examples
            // on how to handle mouse input to edit polygon without catching all mouse events
            
            Event guiEvent = Event.current;

            // Get unique control ID
            EventType eventType = guiEvent.type;

            if (eventType == EventType.Layout)
            {
                // Required to catch event and avoid selecting object under cursor
                int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
                HandleUtility.AddDefaultControl(controlID);
            }
            else if (guiEvent.shift && eventType == EventType.MouseDown && guiEvent.button == 0)
            {
                // add new key point at the end at mouse position
                Vector2 newKeyPoint = (Vector2) HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin - offset;
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
                    // currently, it won't remove a point if you're hovering it, not just nearby, but it's convenient
                    // to avoid removing a point by accident when you only wanted to snap by holding ctrl 
                    Vector2 mousePoint = (Vector2) HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin - offset;
                    int index = path.GetNearestKeyPointIndex(mousePoint);
                    path.RemoveKeyPoint(index);
                }

                // still consume event to avoid unwanted effects
                guiEvent.Use();
            }
        }

        /// Draw the interpolated path, without control points, with offset
        private static void DrawInterpolatedPath (BezierPath2D path, Vector2 offset)
        {
            List<Vector2> interpolatedPoints = InterpolatePath(path, offset);
            HandlesUtil.DrawPolyLine2D(interpolatedPoints.ToArray(), pathColor);
            
            // if more than 1 point (most commonly), add an arrow in the middle to show the orientation of the path
            if (interpolatedPoints.Count > 1)
            {
                DrawArrowInPathMiddle(interpolatedPoints);
            }
        }

        /// Return a list of interpolated points over the given path, with offset
        private static List<Vector2> InterpolatePath (BezierPath2D path, Vector2 offset)
        {
            // interpolate each curve, and concatenate all of them into a smooth path
            List<Vector2> interpolatedPoints = new List<Vector2>();

            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                // TODO: camera pixel size
                // must be high enough to get decent evaluation of curve length
                // not as critical as the other parameters below, as increasing it will only add gradually
                // more precision to curveLength (increasing it slightly), but CeilToInt will round all of that 
                const int evaluateCurveLengthSegmentsCount = 10;
                const float maxSegmentLength = 0.1f; // in world units
                // to avoid freezing when interpolating a curve between points very far from each other
                const int maxSegmentCount = 30; 

                // compute number of segments required to get appropriate resolution if the curve was linear
                // we know that a Bezier curve has a curvilinear abscissa with higher derivative, but that will be enough
                // for curves that are not too crazy i.e. control points are not too far
                Vector2[] curve = path.GetCurve(i);

                // Split curve in several segments, following the resolution given by maxSegmentLength,
                // but never more than maxSegmentCount.
                float curveLength = path.EvaluateCurveLength(i, evaluateCurveLengthSegmentsCount);
                int segmentCount = Mathf.CeilToInt(curveLength / maxSegmentLength);
                int clampedSegmentCount = Mathf.Min(segmentCount, maxSegmentCount);
                for (int j = 0; j < clampedSegmentCount; ++j)
                {
                    float t = (float) j / (float) clampedSegmentCount;
                    interpolatedPoints.Add(BezierPath2D.InterpolateBezier(curve[0], curve[1], curve[2], curve[3], t) + offset);
                }
            }

            // last point
            int lastIndex = path.GetControlPointsCount() - 1;
            Vector2 lastPoint = path.GetControlPoint(lastIndex);
            interpolatedPoints.Add(lastPoint + offset);

            return interpolatedPoints;
        }

        private static void DrawArrowInPathMiddle (List<Vector2> interpolatedPoints)
        {
            // we need at least 2 points to get a middle point and a tangent
            int interpolatedPointsCount = interpolatedPoints.Count;
            Debug.AssertFormat(interpolatedPointsCount >= 2, "interpolatedPoints.Count is {0}, expected at least 2 points", interpolatedPointsCount);
            
            // find the approximate middle point of the path (it depends on how the path is interpolated)
            int midIndex = interpolatedPointsCount / 2;
            // calculate local tangent (from the previous point)
            Vector2 tangent = interpolatedPoints[midIndex] - interpolatedPoints[midIndex - 1];
            HandlesUtil.DrawArrowHead2D(interpolatedPoints[midIndex], tangent, pathColor);
        }

        /// Draw the control points of the given path, with segments between key points and non-key control points
        /// to visualize tangents, with offset
        private static void DrawControlPoints (BezierPath2D path, Vector2 offset)
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
                var p0 = path.GetControlPoint(3 * i) + offset;
                HandlesUtil.DrawFreeMoveHandle(ref p0, keyPointColor);
                path.SetControlPoint(3 * i, p0 - offset);

                // draw out tangent point
                var p1 = path.GetControlPoint(3 * i + 1) + offset;
                HandlesUtil.DrawFreeMoveHandle(ref p1, ColorUtil.orange);
                path.SetControlPoint(3 * i + 1, p1 - offset);

                // draw out tangent
                HandlesUtil.DrawLine(p0, p1, tangentColor);

                // draw in tangent point
                var p2 = path.GetControlPoint(3 * i + 2) + offset;
                HandlesUtil.DrawFreeMoveHandle(ref p2, ColorUtil.orange);
                path.SetControlPoint(3 * i + 2, p2 - offset);

                // draw in tangent
                var p3 = path.GetControlPoint(3 * i + 3) + offset;
                HandlesUtil.DrawLine(p2, p3, tangentColor);
            }

            // draw last key point
            int lastIndex = path.GetControlPointsCount() - 1;
            var lastPoint = path.GetControlPoint(lastIndex) + offset;
            HandlesUtil.DrawFreeMoveHandle(ref lastPoint, Color.white);
            path.SetControlPoint(lastIndex, lastPoint - offset);
        }
    }
}
