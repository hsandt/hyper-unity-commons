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


        /* Reflection helpers */

        private static MethodInfo distanceToPolyLine3ArgsMethodInfo;

        private static float DistanceToPolyLine(Vector3[] points, bool loop, out int index)
        {
            // Cache method info if not done yet
            if (distanceToPolyLine3ArgsMethodInfo == null)
            {
                // We want to access the 3-parameter overload of HandleUtility.DistanceToPolyLine, which is internal:
                // internal static float DistanceToPolyLine(Vector3[] points, bool loop, out int index)
                // Normally, we can indicate overload parameters:
                // typeof(HandleUtility).GetMethod("DistanceToPolyLine",
                //     new[] { typeof(Vector3[]), typeof(bool), typeof(int).MakeByRefType() });
                // but for some reason, this doesn't work. So instead, we're indicating flags (the most important flag
                // is BindingFlags.NonPublic, which distinguishes the overload from the 1-parameter public overload)
                distanceToPolyLine3ArgsMethodInfo = typeof(HandleUtility).GetMethod("DistanceToPolyLine",
                    BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic);
            }

            float distance = 0f;
            index = -1;

            if (distanceToPolyLine3ArgsMethodInfo != null)
            {
                // Invoke method, getting return value and setting out argument manually
                object[] parameters = {points, loop, index};
                distance = (float) distanceToPolyLine3ArgsMethodInfo.Invoke(null, parameters);
                index = (int) parameters[2];
            }

            return distance;
        }


        // Number of segments a curve is split in for pre-interpolation curve length evaluation
        // The segment count must be high enough to get decent evaluation of curve length.
        // It is not as critical as the other parameters below, as increasing it will only add gradually
        // more precision to curveLength (increasing it slightly), but CeilToInt will round all of that.
        // Besides, this is only used for visual interpolation in the editor.
        // At runtime, we can always call EvaluateCurveLength with a higher segment count.
        private const int INTERPOLATION_EVALUATE_CURVE_LENGTH_SEGMENTS_COUNT = 10;

        // Preferred segment length for path interpolation, to avoid too imprecise interpolation
        // (in world units, which means it is not affected by zooming in the editor)
        private const float INTERPOLATION_PREFERRED_SEGMENT_LENGTH = 0.1f;

        // Max segment count allowed per curve interpolation. It takes priority over preferredSegmentLength,
        // to avoid freezing when interpolating a curve between points very far from each other
        private const int INTERPOLATION_MAX_SEGMENT_COUNT = 30;

        private const float k_PointPickDistance = 100f;
        // TODO: use those to insert new key point in the middle of a curve
        private const float k_LinePickDistance = 50f;
        private const float k_ActiveLineSegmentWidth = 5f;

        private static readonly int s_InsertPointHash = "s_InsertPointHash".GetHashCode();
        private static readonly int s_RemovePointHash = "s_RemovePointHash".GetHashCode();

        private static readonly Color pathColor = Color.cyan;
        private static readonly Color keyPointColor = Color.cyan;
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

            // Pre-compute interpolated points, they are used for both edit (to detect if cursor is close
            // to a curve) and drawing. Those interpolated points do *not* integrate the path offset.
            (List<Vector2> interpolatedPoints, List<int> curveStartIndices) = InterpolatePath(path);

            // To take the path offset into account, so all the geometric methods below use the correct coordinates,
            // we use a Handles matrix that translates everything by the path offset.
            Matrix4x4 offsetMatrix = Matrix4x4.Translate(offset);
            using (new Handles.DrawingScope(offsetMatrix))
            {
                float distanceToNearestCurve = GetNearestCurve(interpolatedPoints, curveStartIndices, out int nearestCurveIndex);

                // Draw the interpolated path to have a smooth visualization
                DrawInterpolatedPath(interpolatedPoints.ToArray(), curveStartIndices, distanceToNearestCurve, nearestCurveIndex);

                if (IsEditingCollider)
                {
                    Undo.RecordObject(script, "Change Bezier Path");

                    Event guiEvent = Event.current;

                    bool readyToInsertKeyPoint = false;
                    int readyToRemoveKeyPointIndex = -1;

                    if (guiEvent.shift)
                    {
                        readyToInsertKeyPoint = true;
                    }
                    else if (guiEvent.control)
                    {
                        // Check if there are enough key points to remove one
                        if (path.GetKeyPointsCount() > 2)
                        {
                            // Find nearest key point with distance
                            float distanceToNearestKeyPoint = FindMouseNearestKeyPointDistance(path, out int nearestKeyPointIndex);

                            // Control-click doesn't have priority if we are hovering some control point (including the one to remove)
                            // but in any other case, up to k_PointPickDistance, it has priority, so just do a binary check:
                            // if below that distance threshold, make key point to remove the default control.
                            if (distanceToNearestKeyPoint <= k_PointPickDistance)
                            {
                                // We are close enough to remove that key point
                                readyToRemoveKeyPointIndex = nearestKeyPointIndex;
                            }
                        }
                    }

                    // To keep stable control IDs, we compute them at top-level then pass them to methods that need it
                    // https://forum.unity.com/threads/how-a-control-keep-same-controlid-from-getcontrolid-in-a-dynamic-ui.836527/
                    int insertControlID = GUIUtility.GetControlID(s_InsertPointHash, FocusType.Passive);
                    int removeControlID = GUIUtility.GetControlID(s_RemovePointHash, FocusType.Passive);

                    DoLayout(readyToInsertKeyPoint, readyToRemoveKeyPointIndex, insertControlID, removeControlID);

                    // Manually ensure that hovered handles have priority over remove control,
                    // by canceling remove action (including highlighting key point to remove)
                    // if it is not the nearest control.
                    // This way, holding ctrl with cursor over or very close to control point handles will not
                    // highlight the nearest key point, and clicking will start dragging them (this is useful
                    // when user holds ctrl before clicking, preparing for a snapping drag-and-drop).
                    // Note that we do *not* do this for the insert action, as we want to allow the user
                    // to add a new key point near an existing control point; and it is unlikely that the user
                    // tries to hold shift before dragging a control point as it does nothing.
                    if (readyToRemoveKeyPointIndex != -1 && HandleUtility.nearestControl != removeControlID)
                    {
                        readyToRemoveKeyPointIndex = -1;
                    }

                    // Handle add/remove point input
                    HandleEditInput(path, readyToInsertKeyPoint, readyToRemoveKeyPointIndex);

                    // Draw control points to allow the user to edit them
                    DrawControlPointHandles(path, readyToRemoveKeyPointIndex);
                }
            }
        }

        /// Return distance to nearest key point, and set out variable to index of that key point
        /// If no key points (which is invalid), return float.MaxValue with index -1.
        private static float FindMouseNearestKeyPointDistance(BezierPath2D path, out int nearestKeyPointIndex)
        {
            // Inspired by HandleUtility.DistanceToPolyLine
            // It converts all positions to GUI points, which allows us to return nearestKeyPointDistance
            // also in GUI points, which is a more relevant unit for threshold comparison than world units.
            // Note that the handleMatrix below is in fact the offsetMatrix defined for the scope
            // FindMouseNearestKeyPointDistance is called in.
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

            float distance = DistanceToPolyLine(interpolatedPoints3D, false, out int nearestInterpolatedPointIndex);

            // Find index of curve that contains the nearest interpolated point index
            int searchIndex = curveStartIndices.BinarySearch(nearestInterpolatedPointIndex);
            if (searchIndex >= 0)
            {
                // The nearest interpolated point was just a key point. If it's a middle key point,
                // it belongs to 2 curves, but to simplify we just pick the curve that starts at this key point.
                // Note that for convenience, curveStartIndices's last index is the last interpolated point index,
                // so this includes the case where the nearest interpolated point is the last key point.
                index = searchIndex;
            }
            else
            {
                // Binary Search could not find exact point, which means the interpolated point was not a key point,
                // but a mid-curve interpolated point between two key points. In this case, BinarySearch returns
                // the complement of the upper bound index, where the upper bound index is the index of the key point
                // located at the *end* of the curve covering the interpolated point (if it is the last curve,
                // it is curveStartIndices.Count, the index of the last key point).
                // So we re-complement it to get the key point index, then subtract 1 to get the index of the *start*
                // key point of that curve, to keep the same convention as above.
                index = ~searchIndex - 1;
            }

            return distance;
        }

        private void DoLayout(bool readyToInsertKeyPoint, int readyToRemoveKeyPointIndex,
            int insertControlID, int removeControlID)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            if (eventType == EventType.Layout || eventType == EventType.MouseMove)
            {
                if (readyToInsertKeyPoint)
                {
                    // Wherever we click, inserting key point is the prioritized action, so add it as default control
                    // and flag action as ready (we still need to check for mouse button down)
                    HandleUtility.AddDefaultControl(insertControlID);
                }
                else if (readyToRemoveKeyPointIndex != -1)
                {
                    HandleUtility.AddDefaultControl(removeControlID);
                }
            }
        }

        private void HandleEditInput(BezierPath2D path, bool readyToInsertKeyPoint, int readyToRemoveKeyPointIndex)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            // Detect hold shift (readyToInsertKeyPoint) and primary button click
            // We check nearest control to be consistent with removeControlID more below, but in this case, since we use
            // AddDefaultControl when shift-clicking in DoLayout, we always consider the insertControlID to be the nearest when this happens.
            if (readyToInsertKeyPoint && eventType == EventType.MouseDown && guiEvent.button == 0)
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

            // Detect hold control near key point with at least 2 key points (readyToRemoveKeyPointIndex != -1) and
            // primary button click
            if (readyToRemoveKeyPointIndex != -1 && eventType == EventType.MouseDown && guiEvent.button == 0)
            {
                // Remove key point the nearest to mouse position (it was precomputed)
                path.RemoveKeyPoint(readyToRemoveKeyPointIndex);

                // Consume event
                guiEvent.Use();
            }
        }

        /// Draw the interpolated path, without control points
        private void DrawInterpolatedPath(Vector2[] interpolatedPoints, List<int> curveStartIndices, float distanceToNearestCurve, int nearestCurveIndex)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            if (eventType == EventType.Repaint)
            {
                HandlesUtil.DrawPolyLine2D(interpolatedPoints, pathColor);

                if (distanceToNearestCurve < 100f)
                {
                    // Inspired by LineHandle, draw the nearest curve in bold using Anti-Aliased polyline
                    // For convenience, we made curveStartIndices end with the last point of path,
                    // so curveStartIndices has path.GetCurvesCount() + 1 elements, and we don't have to handle the edge
                    // case where nearestCurveIndex == path.GetCurvesCount() - 1
                    int curveStartIndex = curveStartIndices[nearestCurveIndex];
                    int curveEndIndex = curveStartIndices[nearestCurveIndex + 1];
                    // Remember that range .. is exclusive, but we must draw including the end point, hence + 1
                    HandlesUtil.DrawAAPolyLine2D(interpolatedPoints[curveStartIndex..(curveEndIndex + 1)], k_ActiveLineSegmentWidth, pathColor);
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
            // Interpolate each curve, and concatenate all of them into a smooth path
            var interpolatedPoints = new List<Vector2>();
            var curveStartIndices = new List<int>();

            for (int i = 0; i < path.GetCurvesCount(); ++i)
            {
                // Because we clamp the segment count to minimum 1, we know that we are adding at least one point for
                // this curve, so we can guarantee that the current interpolated points count, i.e. the index
                // of the next interpolated point added, will be the index of this curve's start.
                curveStartIndices.Add(interpolatedPoints.Count);

                Vector2[] curve = path.GetCurve(i);

                // Split curve in several segments, following the resolution given by maxSegmentLength,
                // but never more than maxSegmentCount.
                // This is a recursive problem: to apply a resolution, we need to know the curve length.
                // But to evaluate the curve length, we need to split the curve in a certain number of segments,
                // which generally requires a resolution.
                // To break the recursion, we arbitrarily pick a segments count, independently of resolution:
                // evaluateCurveLengthSegmentsCount. Fortunately, unless we work with very cuspy curves,
                // a medium segment count should be enough (see comment above constant for more details).
                float evaluatedCurveLength = BezierPath2D.EvaluateCurveLength(curve, INTERPOLATION_EVALUATE_CURVE_LENGTH_SEGMENTS_COUNT);
                int segmentCount = Mathf.CeilToInt(evaluatedCurveLength / INTERPOLATION_PREFERRED_SEGMENT_LENGTH);

                // We make sure that there is at least one point for every curve, even if the curve length is 0.
                int clampedSegmentCount = Mathf.Clamp(segmentCount, 1, INTERPOLATION_MAX_SEGMENT_COUNT);
                for (int j = 0; j < clampedSegmentCount; ++j)
                {
                    float t = (float) j / (float) clampedSegmentCount;
                    interpolatedPoints.Add(BezierPath2D.InterpolateBezier(curve[0], curve[1], curve[2], curve[3], t));
                }
            }

            // Last curve index is last interpolated point index, for convenience
            curveStartIndices.Add(interpolatedPoints.Count);

            // Last point
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

        /// Draw handles for the control points of the given path, with segments between key points and non-key control points
        /// to visualize tangents, with offset
        private static void DrawControlPointHandles(BezierPath2D path, int keyPointToRemoveIndex)
        {
            if (path.GetControlPointsCount() < 4)
            {
                // Do not log warning to avoid spamming console, but this is a defect, and we will see nothing
                // just add a new point manually to trigger the safety Init()
                return;
            }

            // Instead of iterating on curves, iterate on key points.
            // This allows us to handle a key point and its 2 tangents (1 tangent for start and end key points)
            // at the same time to move them along, making code more symmetrical than when handling the 2 unrelated
            // in and out tangents of a given curve.
            // With this, we also don't have to draw a last point after the loop.
            int keyPointsCount = path.GetKeyPointsCount();
            for (int i = 0; i < keyPointsCount; ++i)
            {
                // Moving key point also moves attached tangent in and out (if any), so track key point move delta
                Vector2 keyPointDelta;

                var keyPoint = path.GetKeyPoint(i);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var oldP0 = keyPoint;
                    Color color = (i == keyPointToRemoveIndex ? keyPointToRemoveColor : keyPointColor);
                    HandlesUtil.DrawFreeMoveHandle(ref keyPoint, color);
                    keyPointDelta = keyPoint - oldP0;

                    if (check.changed)
                    {
                        path.SetKeyPoint(i, keyPoint);
                    }
                }

                // draw in tangent point and line, if any
                if (i > 0)
                {
                    // Do not add keyPointDelta here, it would double will the `+ keyPointDelta` in SetInTangentPoint
                    var tangentInPoint = path.GetInTangentPoint(i);

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        // draw in tangent point
                        HandlesUtil.DrawFreeMoveHandle(ref tangentInPoint, tangentPointColor);

                        // If user either moved in tangent point directly, or indirectly via the associated key point,
                        // we must move the in tangent point accordingly
                        if (check.changed || keyPointDelta != Vector2.zero)
                        {
                            path.SetInTangentPoint(i, tangentInPoint + keyPointDelta);
                        }
                    }

                    // draw in tangent line
                    HandlesUtil.DrawLine(keyPoint, tangentInPoint, tangentColor);
                }

                // draw out tangent point and line, if any
                if (i < keyPointsCount - 1)
                {
                    // Do not add keyPointDelta here, it would double will the `+ keyPointDelta` in SetOutTangentPoint
                    var tangentOutPoint = path.GetOutTangentPoint(i);

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        // draw out tangent point
                        HandlesUtil.DrawFreeMoveHandle(ref tangentOutPoint, tangentPointColor);

                        // If user either moved out tangent point directly, or indirectly via the associated key point,
                        // we must move the out tangent point accordingly
                        if (check.changed || keyPointDelta != Vector2.zero)
                        {
                            path.SetOutTangentPoint(i, tangentOutPoint + keyPointDelta);
                        }
                    }

                    // draw out tangent line
                    HandlesUtil.DrawLine(keyPoint, tangentOutPoint, tangentColor);
                }
            }
        }
    }
}
