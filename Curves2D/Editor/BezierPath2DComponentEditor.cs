// References
//
// All source files can be found at
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector
//
// The closest to a 2D curve editor we can find in Unity is the EdgeCollider2D component.
// It uses EdgeCollider2DTool : EditablePathTool : EditorTool and EdgeColliderPath : EditablePath2D
// EditablePath2D uses LineHandle (LineHandle.cs), which is our main inspiration for the editing code.
//
// For the Edit Mode button however, we didn't use the EditorTool pipeline,
// but preferred a classic custom editor with a button using the EditMode system (EditMode.cs), as in
// ColliderEditorBase.cs, HingeJoint2DEditor.cs, OcclusionPortalEditor.cs, SkinnedMeshRendererEditor.cs
// (the key method is EditMode.DoEditModeInspectorModeButton).
//
// This gives a centered button instead of the nice property-like Edit Collider button of EdgeCollider2D,
// but it's good enough. If we really want to use an EditorTool with a tool icon, we just have to mimic
// EditablePathTool, moving OnSceneGUI code to OnToolGUI.
//
// Note that ColliderEditorBase.InspectorEditButtonGUI is not called anymore, as EdgeCollider2D and others
// collider components now use the EditorTool system. But this is still a good reference to use.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace CommonsHelper.Editor
{
    [CustomEditor(typeof(BezierPath2DComponent))]
    public class BezierPath2DComponentEditor : UnityEditor.Editor
    {
        private static class Styles
        {
            public static readonly GUIContent editModeButton = new GUIContent(
                EditorGUIUtility.IconContent("EditCollider").image,
                "Edit Bezier path.\n\n - Hold Ctrl before clicking to remove the nearest key point.\n" +
                "- Hold Shift before clicking to place a new key point, along with 2 control points (tangents)."
            );
        }


        private const float k_PointPickDistance = 50f;
        // TODO: use those to insert new key point in the middle of a curve
        private const float k_LinePickDistance = 50f;
        private const float k_ActiveLineSegmentWidth = 5f;

        private static readonly int s_InsertPointHash = "s_InsertPointHash".GetHashCode();
        private static readonly int s_RemovePointHash = "s_RemovePointHash".GetHashCode();

        private static readonly Color pathColor = Color.cyan;
        private static readonly Color keyPointColor = Color.white;
        private static readonly Color keyPointToRemoveColor = Color.red;
        private static readonly Color tangentPointColor = ColorUtil.orange;
        private static readonly Color tangentColor = Color.yellow;


        /// True when the Edit Path button is clicked, i.e. edit mode is set to Collider, and it is
        /// the button on this custom inspector that was clicked
        private bool IsEditingCollider => EditMode.editMode == EditMode.SceneViewEditMode.Collider &&
                                          EditMode.IsOwner(this);


        public override void OnInspectorGUI()
        {
            var script = (BezierPath2DComponent) target;
            if (script == null)
            {
                return;
            }

            AddEditModeButton();

            GUILayout.Space(5);

            base.OnInspectorGUI();

            if (GUILayout.Button("Add New Key Point at Origin"))
            {
                BezierPath2D path = script.Path;

                Undo.RecordObject(script, "Add Key Point");
                path.AddKeyPoint(Vector2.zero);
                SceneView.RepaintAll();
            }
        }

        private void AddEditModeButton()
        {
            EditMode.DoEditModeInspectorModeButton(
                // Not exactly a collider, but the closest category
                EditMode.SceneViewEditMode.Collider,
                "Edit Path",
                Styles.editModeButton,
                // Slight difference with ColliderEditorBase: signature that skips getBoundsOfTargets is internal,
                // so we cannot use it and must pass either bounds callback or null.
                // The returned bounds seem only used by ChangeEditMode which does nothing with it,
                // so don't bother and just pass null.
                null,
                this
            );
        }

        private void OnSceneGUI()
        {
            var script = (BezierPath2DComponent) target;

            if (script != null)
            {
                DrawEditablePath(script);
            }
        }

        private void DrawEditablePath(BezierPath2DComponent script)
        {
            BezierPath2D path = script.Path;
            Vector2 offset = script.IsRelative ? (Vector2)script.transform.position : Vector2.zero;

            // NEXT: inject offset in path so no need to use it everywhere

            // Pre-compute interpolated points, they are used for both edit (to detect if cursor is close
            // to a curve) and drawing
            (List<Vector2> interpolatedPoints, List<int> curveStartIndices) = InterpolatePath(path);

            float distanceToNearestCurve = GetNearestCurve(interpolatedPoints, curveStartIndices, out int nearestCurveIndex);

            Event guiEvent = Event.current;

            int keyPointToRemoveIndex = -1;

            Matrix4x4 offsetMatrix = Matrix4x4.Translate(offset);
            using (new Handles.DrawingScope(offsetMatrix))
            {
                // Phase 3: draw the interpolated path to have a smooth visualization
                DrawInterpolatedPath(interpolatedPoints.ToArray(), curveStartIndices, distanceToNearestCurve, nearestCurveIndex);

                if (IsEditingCollider)
                {
                    Undo.RecordObject(script, "Change Bezier Path");

                    // https://forum.unity.com/threads/how-a-control-keep-same-controlid-from-getcontrolid-in-a-dynamic-ui.836527/
                    // known issue: when clicking on control point 18 (key point), the 1st tangent point (control point 2) is highlighted in yellow
                    // it may be due to a ridiculous hashing collision, need to check
                    // but that should be an unrelated issue, due to Handle control id, limited to 18 unique entries, maybe?
                    // known issue 2: key point to remove is not colored in red anymore
                    int insertControlID = GUIUtility.GetControlID(s_InsertPointHash, FocusType.Passive);
                    int removeControlID = GUIUtility.GetControlID(s_RemovePointHash, FocusType.Passive);

                    float distanceToNearestKeyPoint = float.MaxValue;

                    if (guiEvent.control)
                    {
                        // we only care about nearest key point when holding ctrl
                        distanceToNearestKeyPoint = GetMouseNearestKeyPointDistance(path, out keyPointToRemoveIndex);
                    }

                    DoLayout(insertControlID, removeControlID, distanceToNearestKeyPoint);

                    if (!(guiEvent.control && HandleUtility.nearestControl == removeControlID))
                    {
                        keyPointToRemoveIndex = -1;
                    }

                    // Phase 2: in edit mode only, handle add/remove point input
                    HandleEditInput(path, keyPointToRemoveIndex, insertControlID, removeControlID);

                    // Phase 1: in edit mode only, draw control points to allow the user to edit them
                    // The only reason we do that before HandleEditInput is to allow editing the control points
                    // while detecting add/remove point input (as it uses a custom control ID and consumes all other events)
                    DrawControlPoints(path, keyPointToRemoveIndex);
                }
            }
        }

        /// Return distance to nearest key point, and set out variable to index of that key point
        /// If no key points (which is invalid), return float.MaxValue with index -1.
        private static float GetMouseNearestKeyPointDistance(BezierPath2D path, out int nearestKeyPointIndex)
        {
            // Inspired by HandleUtility.DistanceToPolyLine
            // It converts all positions to GUI points, which allows us to return nearestKeyPointDistance
            // also in GUI points, which is a more relevant unit for threshold comparison than world units.
            // Note that the handleMatrix below is in fact the offsetMatrix defined for the scope
            // GetMouseNearestKeyPointDistance is called in.
            Matrix4x4 handleMatrix = Handles.matrix;
            CameraProjectionCache cam = new CameraProjectionCache(Camera.current);
            Vector2 mousePosition = Event.current.mousePosition;

            nearestKeyPointIndex = -1;
            float nearestKeyPointSqrDistance = float.MaxValue;

            int keyPointsCount = path.GetKeyPointsCount();
            for (int keyIndex = 0; keyIndex < keyPointsCount; ++keyIndex)
            {
                Vector2 keyPoint = path.GetKeyPoint(keyIndex);
                Vector2 keyPointOnGUI = cam.WorldToGUIPoint(handleMatrix.MultiplyPoint3x4(keyPoint));

                float sqrDistance = Vector2.SqrMagnitude(keyPointOnGUI - mousePosition);
                if (sqrDistance < nearestKeyPointSqrDistance)
                {
                    nearestKeyPointIndex = keyIndex;
                    nearestKeyPointSqrDistance = sqrDistance;
                }
            }

            // Return distance to nearest key point if found (remember to Sqrt), else MaxValue
            return nearestKeyPointIndex >= 0 ? Mathf.Sqrt(nearestKeyPointSqrDistance) : float.MaxValue;
        }

        private float GetNearestCurve(IEnumerable<Vector2> interpolatedPoints, List<int> curveStartIndices, out int index)
        {
            Vector3[] interpolatedPoints3D = interpolatedPoints.Select(point2D => (Vector3) point2D).ToArray();

            // HandleUtility.DistanceToPolyLine
            // Reflection code for HandleUtility.DistanceToPolyLine(v, false, points);
            // internal static float DistanceToPolyLine(Vector3[] points, bool loop, out int index)

            // var distanceToPolyLineMethod = typeof(HandleUtility).GetMethod("DistanceToPolyLine",
            //     new[] { typeof(Vector3[]), typeof(bool), typeof(int).MakeByRefType() });
            // For some reason, passing types new[] { typeof(Vector3[]), typeof(bool), typeof(int).MakeByRefType() } doesn't work
            // the only recourse is to pass binding flags that distinguish it from the public, 1 param HandleUtility.DistanceToPolyLine
            // so the most important flag is BindingFlags.NonPublic
            var distanceToPolyLineMethod = typeof(HandleUtility).GetMethod("DistanceToPolyLine", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic);
            float distance = 0f;
            index = -1;
            if (distanceToPolyLineMethod != null)
            {
                object[] parameters = new object[] {interpolatedPoints3D, false, index};
                distance = (float) distanceToPolyLineMethod.Invoke(null, parameters);
                index = (int)parameters[2];

                int searchIndex = curveStartIndices.BinarySearch(index);
                if (searchIndex >= 0)
                {
                    index = searchIndex;
                }
                else
                {
                    // Binary Search could not find exact point, but found upper bound and complemented it
                    index = ~searchIndex - 1;
                }
            }

            return distance;
        }

        private void DoLayout(int insertControlID, int removeControlID, float distanceToNearestKeyPoint)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            if (eventType == EventType.Layout || eventType == EventType.MouseMove)
            {
                if (guiEvent.shift)
                {
                    // Wherever we click, inserting key point is the prioritized action, so add it as default control
                    HandleUtility.AddDefaultControl(insertControlID);
                }
                else if (guiEvent.control)
                {
                    // Control-click doesn't have priority if we are hovering some control point (including the one to remove)
                    // but in any other case, up to k_PointPickDistance, it has priority, so just do a binary check:
                    // if below that distance threshold, make key point to remove the default control.
                    // Note that Unity somewhat manages to give priority to hovered handles, so we don't have to
                    // manually check for them.
                    if (distanceToNearestKeyPoint <= k_PointPickDistance)
                    {
                        HandleUtility.AddDefaultControl(removeControlID);
                    }
                }
            }
        }

        private void HandleEditInput(BezierPath2D path, int nearestKeyPointIndex, int insertControlID, int removeControlID)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            // Detect hold shift and primary button click
            // We check nearest control to be consistent with removeControlID more below, but in this case, since we use
            // AddDefaultControl when shift-clicking in DoLayout, we always consider the insertControlID to be the nearest when this happens.
            if (guiEvent.shift && eventType == EventType.MouseDown && guiEvent.button == 0 && HandleUtility.nearestControl == insertControlID)
            {
                // Add new key point at the end of the path, at mouse position
                // The mouse position includes any path offset, but the key point must be stored without offset,
                // so we must subtract any path offset.
                // We have set Handles.matrix to an offsetMatrix in the scope HandleEditInput is called in,
                // so we can just apply the inverse matrix to the mouse position in world unit to subtract the offset.
                Matrix4x4 subtractOffsetMatrix = Handles.inverseMatrix;
                Vector2 newKeyPoint = (Vector2) subtractOffsetMatrix.MultiplyPoint3x4(HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin);
                path.AddKeyPoint(newKeyPoint);

                // Consume event
                guiEvent.Use();
                return;
            }

            // Detect hold control and primary button click
            // Also only react if key point to remove is nearest control, to handle removing it when hovering this key point
            // or another control point
            if (guiEvent.control && eventType == EventType.MouseDown && guiEvent.button == 0 && HandleUtility.nearestControl == removeControlID)
            {
                // Check if there are enough points for a removal
                if (path.GetKeyPointsCount() > 2)
                {
                    // Remove key point the nearest to mouse position (it was precomputed)
                    path.RemoveKeyPoint(nearestKeyPointIndex);
                }

                // Consume event
                guiEvent.Use();
            }
        }

        /// Draw the interpolated path, without control points
        private void DrawInterpolatedPath(Vector2[] interpolatedPoints, List<int> curveStartIndices, float distanceToNearestCurve, int nearestCurveIndex)
        {
            // Should it be done only
            Event guiEvent = Event.current;

            // Get unique control ID
            EventType eventType = guiEvent.type;
            if (eventType == EventType.Repaint)
            {
                HandlesUtil.DrawPolyLine2D(interpolatedPoints, pathColor);

                // Highlight nearest curve, like Edge Collider 2D
                if (distanceToNearestCurve < 100f)
                {
                    // Inspired by LineHandle, draw the nearest curve in bold using Anti-Aliased polyline
                    // For convenience, we made curveStartIndices end with the last point of path,
                    // so curveStartIndices has path.GetCurvesCount() + 1 elements, and we don't have to handle the edge
                    // case where nearestCurveIndex == path.GetCurvesCount() - 1
                    int curveStartIndex = curveStartIndices[nearestCurveIndex];
                    int curveEndIndex = curveStartIndices[nearestCurveIndex + 1];
                    // Remember that range .. is exclusive, but we must draw including the end point, hence + 1
                    // HandlesUtil.DrawAAPolyLine2D(interpolatedPoints[curveStartIndex..(curveEndIndex + 1)], k_ActiveLineSegmentWidth, pathColor);
                }

                // if more than 1 point (most commonly), add an arrow in the middle to show the orientation of the path
                if (interpolatedPoints.Length > 1)
                {
                    DrawArrowInPathMiddle(interpolatedPoints);
                }
            }
        }

        /// Return a tuple (interpolatedPoints, curveStartIndices)
        /// interpolatedPoints: list of interpolated points when interpolating path
        /// curveStartIndices: list of indices in the interpolated points list at which a new curve is starting,
        ///                    ending with the index of the last interpolated point for convenience.
        private static (List<Vector2>, List<int>) InterpolatePath(BezierPath2D path)
        {
            // interpolate each curve, and concatenate all of them into a smooth path
            var interpolatedPoints = new List<Vector2>();
            var curveStartIndices = new List<int>();

            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                // must be high enough to get decent evaluation of curve length
                // not as critical as the other parameters below, as increasing it will only add gradually
                // more precision to curveLength (increasing it slightly), but CeilToInt will round all of that
                const int evaluateCurveLengthSegmentsCount = 10;
                const float maxSegmentLength = 0.1f; // in world units
                // to avoid freezing when interpolating a curve between points very far from each other
                const int maxSegmentCount = 30;

                // Because we clamp the segment count to minimum 1, we know that we are adding at least one point for
                // this curve, so we can guarantee that the current interpolated points count, i.e. the index
                // of the next interpolated point added, will be the index of this curve's start.
                curveStartIndices.Add(interpolatedPoints.Count);

                // compute number of segments required to get appropriate resolution if the curve was linear
                // we know that a Bezier curve has a curvilinear abscissa with higher derivative, but that will be enough
                // for curves that are not too crazy i.e. control points are not too far
                Vector2[] curve = path.GetCurve(i);

                // Split curve in several segments, following the resolution given by maxSegmentLength,
                // but never more than maxSegmentCount.
                // Also make sure that there is at least one point for every curve, even if the curve length is 0.
                float curveLength = path.EvaluateCurveLength(i, evaluateCurveLengthSegmentsCount);
                int segmentCount = Mathf.CeilToInt(curveLength / maxSegmentLength);
                int clampedSegmentCount = Mathf.Clamp(segmentCount, 1, maxSegmentCount);
                for (int j = 0; j < clampedSegmentCount; ++j)
                {
                    float t = (float) j / (float) clampedSegmentCount;
                    interpolatedPoints.Add(BezierPath2D.InterpolateBezier(curve[0], curve[1], curve[2], curve[3], t));
                }
            }

            // last curve index is last interpolated point index, for convenience
            curveStartIndices.Add(interpolatedPoints.Count);

            // last point
            int lastIndex = path.GetControlPointsCount() - 1;
            Vector2 lastPoint = path.GetControlPoint(lastIndex);
            interpolatedPoints.Add(lastPoint);

            return (interpolatedPoints, curveStartIndices);
        }

        private static void DrawArrowInPathMiddle(IReadOnlyList<Vector2> interpolatedPoints)
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
        private static void DrawControlPoints(BezierPath2D path, int nearestKeyPointIndex)
        {
            if (path.GetControlPointsCount() < 4)
            {
                // do not log warning to avoid spamming console, but this is a defect, and we will see nothing
                // just add a new point manually to trigger the safety Init()
                return;
            }

            Color color;

            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                // before C# 7.0, we cannot use ref directly with GetCurve in an iteration or as return value of a getter
                // try .NET 4.5 later
                var p0 = path.GetControlPoint(3 * i);
                color = i == nearestKeyPointIndex ? keyPointToRemoveColor : keyPointColor;
                HandlesUtil.DrawFreeMoveHandle(ref p0, color);
                path.SetControlPoint(3 * i, p0);

                // draw out tangent point
                var p1 = path.GetControlPoint(3 * i + 1);
                HandlesUtil.DrawFreeMoveHandle(ref p1, tangentPointColor);
                path.SetControlPoint(3 * i + 1, p1);

                // draw out tangent
                HandlesUtil.DrawLine(p0, p1, tangentColor);

                // draw in tangent point
                var p2 = path.GetControlPoint(3 * i + 2);
                HandlesUtil.DrawFreeMoveHandle(ref p2, tangentPointColor);
                path.SetControlPoint(3 * i + 2, p2);

                // draw in tangent
                var p3 = path.GetControlPoint(3 * i + 3);
                HandlesUtil.DrawLine(p2, p3, tangentColor);
            }

            // draw last key point
            int lastControlPointIndex = path.GetControlPointsCount() - 1;
            var lastKeyPoint = path.GetControlPoint(lastControlPointIndex);
            // ! control point index is not key point index !
            color = path.GetCurvesCount() == nearestKeyPointIndex ? keyPointToRemoveColor : keyPointColor;
            HandlesUtil.DrawFreeMoveHandle(ref lastKeyPoint, color);
            path.SetControlPoint(lastControlPointIndex, lastKeyPoint);
        }
    }
}
