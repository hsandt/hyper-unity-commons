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
                "- Hold Shift before clicking to place a new key point at the end, along with 2 smooth control points (tangents).\n" +
                "- Hold Ctrl and Shift before clicking to place a new key point at the start, along with 2 smooth control points (tangents)."
            );
        }


        /* Reflection helpers */

        private static MethodInfo distanceToPolyLine3ArgsMethodInfo;

        /// <summary>
        /// Return the distance to a polyline with optional loop flag, and out index of the nearest segment.
        /// This uses Reflection to call a Unity internal method.
        /// </summary>
        /// <param name="points">Polyline points</param>
        /// <param name="loop">If true, a last segment joins the last and the first point</param>
        /// <param name="index">Out index of the nearest segment</param>
        /// <returns></returns>
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

        // Naming case differs because copied from LineHandle.cs
        private const float k_PointPickDistance = 100f;
        private const float k_LinePickDistance = 50f;
        private const float k_ActiveLineSegmentWidth = 5f;

        private static readonly int s_MovePointHash = "s_MovePointHash".GetHashCode();
        private static readonly int s_AddPointHash = "s_AddPointHash".GetHashCode();
        private static readonly int s_InsertPointHash = "s_InsertPointHash".GetHashCode();
        private static readonly int s_RemovePointHash = "s_RemovePointHash".GetHashCode();

        private static readonly Color s_PathColor = Color.cyan;
        private static readonly Color s_KeyPointColor = Color.cyan;
        private static readonly Color s_KeyPointToRemoveColor = Color.red;
        private static readonly Color s_TangentPointColor = ColorUtil.orange;
        private static readonly Color s_TangentColor = Color.yellow;

        /// Index of inserted point, useful to track inserted point and immediately chain with handle dragging
        /// This is only set to a valid index after actually inserting a key point, while dragging, until user stopped
        /// dragging it (Mouse Up)
        private int m_InsertedPointIndex = -1;

        /// True when the Edit Path button is clicked, i.e. edit mode is set to Collider, and it is
        /// the button on this custom inspector that was clicked
        private bool IsEditingCollider => EditMode.editMode == EditMode.SceneViewEditMode.Collider &&
                                          EditMode.IsOwner(this);


        private bool HasFrameBounds()
        {
            return true;
        }

        public Bounds OnGetFrameBounds()
        {
            var script = (BezierPath2DComponent) target;

            Vector2 offset = script.IsRelative ? (Vector2)script.transform.position : Vector2.zero;
            Matrix4x4 offsetMatrix = Matrix4x4.Translate(offset);

            // Tangent points are not exact bounds, but the path should be contained within a polygon containing the tangents,
            // and we should have the tangent points in view to edit them anyway, so just take the bounds of all control points,
            // key points and tangent points
            Bounds bounds = GeometryUtility.CalculateBounds(script.Path.ControlPoints.Select(p2D => (Vector3) p2D).ToArray(), offsetMatrix);
            return bounds;
        }

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

                path.SanitizePath();
                path.AddKeyPoint(Vector2.zero);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Subtract First Key Point Offset"))
            {
                BezierPath2D path = script.Path;
                Undo.RecordObject(script, "Subtract First Key Point Offset");

                path.SanitizePath();
                path.SubtractFirstKeyPointOffset();
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
                // BUG: EditMode.ChangeEditMode ignores its bounds parameters, calculated via the callback passed below
                // We cannot just pass null and count on Editor.GetWorldBoundsOfTarget as fallback either, because
                // it is internal virtual, so we cannot override it in this assembly.
                // Once this bug is fixed, this callback will work and the user will be able to focus on / frame
                // the complete path just by clicking on the Edit Mode button. Until then, user will have to
                // double-click on the game object that has the path component instead.
                OnGetFrameBounds,
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

            if (!path.IsValid())
            {
                // Path is invalid, sanitize it now
                Undo.RecordObject(script, "Sanitize Bezier Path");
                path.SanitizePath();
            }

            Vector2 offset = script.IsRelative ? (Vector2)script.transform.position : Vector2.zero;

            // Pre-compute interpolated points, they are used for both edit (to detect if cursor is close
            // to a curve) and drawing. Those interpolated points do *not* integrate the path offset.
            (List<Vector2> interpolatedPoints, List<int> curveStartIndices) = InterpolatePath(path);
            Vector2[] interpolatedPointsArray = interpolatedPoints.ToArray();

            // To take the path offset into account, so all the geometric methods below use the correct coordinates,
            // we use a Handles matrix that translates everything by the path offset.
            Matrix4x4 offsetMatrix = Matrix4x4.Translate(offset);
            using (new Handles.DrawingScope(offsetMatrix))
            {
                float distanceToNearestCurve = GetNearestCurve(interpolatedPoints, curveStartIndices, out int nearestCurveIndex, out float nearestPointParameterRatioOnCurve, out Vector2 nearestPoint);

                // Draw the interpolated path to have a smooth visualization
                DrawInterpolatedPath(interpolatedPointsArray);

                if (IsEditingCollider)
                {
                    Undo.RecordObject(script, "Change Bezier Path");

                    Event guiEvent = Event.current;

                    // Check if we should preview any user action
                    // If multiple actions are available, pick one with the following priority order:
                    // Add > Remove > Insert
                    bool readyToAddKeyPoint = false;
                    bool readyToInsertKeyPoint = false;
                    int readyToRemoveKeyPointIndex = -1;

                    if (guiEvent.shift)
                    {
                        readyToAddKeyPoint = true;
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
                    else if (distanceToNearestCurve <= k_LinePickDistance && nearestPointParameterRatioOnCurve > 0.01f && nearestPointParameterRatioOnCurve < 0.99f)
                    {
                        readyToInsertKeyPoint = true;
                    }

                    // To keep stable control IDs, we compute them at top-level then pass them to methods that need it
                    // https://forum.unity.com/threads/how-a-control-keep-same-controlid-from-getcontrolid-in-a-dynamic-ui.836527/
                    int addControlID = GUIUtility.GetControlID(s_AddPointHash, FocusType.Passive);
                    int insertControlID = GUIUtility.GetControlID(s_InsertPointHash, FocusType.Passive);
                    int removeControlID = GUIUtility.GetControlID(s_RemovePointHash, FocusType.Passive);

                    DoLayout(readyToAddKeyPoint, readyToInsertKeyPoint, readyToRemoveKeyPointIndex, addControlID, insertControlID, removeControlID);

                    // Manually ensure that hovered handles have priority over insert/remove control,
                    // by clearing the corresponding ready flag if it is not the nearest control.
                    // This way, actions won't overlap and conflict:
                    // - holding ctrl with cursor over or very close to control point handles will not
                    // highlight the nearest key point, and clicking will start dragging them as expected
                    // (this is useful when user holds ctrl before clicking, preparing for a snapping drag-and-drop)
                    // - dragging an existing key point won't try to insert a new one
                    // Note that we do *not* do this for the add action, as we want to allow the user
                    // to add a new key point near an existing control point; and it is unlikely that the user
                    // tries to hold shift before dragging a control point as it does nothing.

                    // In addition, if dragging a newly inserted key point, also clear flag
                    // Physically, user cannot really insert a key point while dragging since the mouse button is
                    // already down, but it's still important to clear it to avoid feedbacking readyToInsertKeyPoint
                    // visually (it would show the nearest curve with glow and even the nearest point for future split).

                    // Code order note: while it seems odd that this happens before DrawControlPointHandles,
                    // remember that DrawControlPointHandles was called last frame and set the layout for handles
                    // during the Layout phase, which is still remembered this frame.
                    if (readyToInsertKeyPoint && (HandleUtility.nearestControl != insertControlID || m_InsertedPointIndex != -1))
                    {
                        readyToInsertKeyPoint = false;
                    }
                    if (readyToRemoveKeyPointIndex != -1 && HandleUtility.nearestControl != removeControlID)
                    {
                        readyToRemoveKeyPointIndex = -1;
                    }

                    // Handle add/insert/remove point input
                    HandleEditInput(path, readyToAddKeyPoint, readyToRemoveKeyPointIndex, readyToInsertKeyPoint,
                        interpolatedPointsArray, curveStartIndices, nearestCurveIndex, nearestPointParameterRatioOnCurve, nearestPoint);

                    // Draw control points to allow the user to edit them
                    DrawControlPointHandles(path, readyToRemoveKeyPointIndex, insertControlID);
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
            // So for once, we don't call GetMouseWorldPositionWithoutOffset and just get mousePosition instead.
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

        private static Vector2 GetMouseWorldPositionWithoutOffset(Event guiEvent)
        {
            // It's easier to work with non-offset path positions, so when dealing with mouse world position,
            // we subtract the BezierPath2DComponent offset.
            // For this to work, we assume that the current Handles Matrix is the offset matrix in this scope,
            // so we can just apply the inverse matrix to the mouse position in world unit to subtract the offset.
            Matrix4x4 subtractOffsetMatrix = Handles.inverseMatrix;
            Vector2 mouseWorldPositionWithoutOffset = (Vector2) subtractOffsetMatrix.MultiplyPoint3x4(HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin);
            return mouseWorldPositionWithoutOffset;
        }

        /// <summary>
        /// Return distance to nearest curve, along with extra info via out parameters
        /// </summary>
        /// <param name="interpolatedPoints">Pre-computed interpolated polyline points</param>
        /// <param name="curveStartIndices">List of indices at which a new curve starts, ending with the last interpolated point index for convenience</param>
        /// <param name="nearestCurveIndex">Out index of nearest curve found</param>
        /// <param name="nearestPointParameterRatioOnCurve">Out parameter ratio of nearest point found along nearest curve found</param>
        /// <param name="nearestPoint">Out nearest point found</param>
        /// <returns>Distance to nearest curve</returns>
        private static float GetNearestCurve(IEnumerable<Vector2> interpolatedPoints, List<int> curveStartIndices,
            out int nearestCurveIndex, out float nearestPointParameterRatioOnCurve, out Vector2 nearestPoint)
        {
            Vector3[] interpolatedPoints3D = interpolatedPoints.Select(point2D => (Vector3) point2D).ToArray();

            // Find the interpolated segment the nearest to the cursor
            Debug.AssertFormat(interpolatedPoints3D.Length > 0, "[BezierPath2DComponentEditor] GetNearestCurve: No interpolated points");
            float distance = DistanceToPolyLine(interpolatedPoints3D, false, out int nearestInterpolatedSegmentIndex);

            // Find index of curve that contains the nearest interpolated segment index
            int searchIndex = curveStartIndices.BinarySearch(nearestInterpolatedSegmentIndex);
            if (searchIndex >= 0)
            {
                // The nearest interpolated point was just a key point. If it's a middle key point,
                // it belongs to 2 curves, but to simplify we just pick the curve that starts at this key point.
                // Note that for convenience, curveStartIndices's last index is the last interpolated point index,
                // so this includes the case where the nearest interpolated point is the last key point.
                nearestCurveIndex = searchIndex;
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
                nearestCurveIndex = ~searchIndex - 1;
            }

            // We must now be more precise and find the point the nearest to the cursor on that segment.
            // Unlike LineHandle.Do which works in GUI space, here we don't need to compare screen distances in pixels,
            // so it's easier to just work in World space (projection is frame-agnostic anyway).
            Vector3 segmentStart = interpolatedPoints3D[nearestInterpolatedSegmentIndex];
            Vector3 segmentEnd = interpolatedPoints3D[nearestInterpolatedSegmentIndex + 1];
            Vector2 mouseWorldPositionWithoutOffset = GetMouseWorldPositionWithoutOffset(Event.current);
            nearestPoint = VectorUtil.PointToClosestPointOnSegment(mouseWorldPositionWithoutOffset,
                segmentStart, segmentEnd, out float nearestPointParameterRatioOnSegment);

            // We must also out the curve parameter ratio of that point.
            // We already know the segment where it lies, so we can start with the parameter ratio of the segment start.
            // Since segments are uniformly spaced across the curve (by parameter ratio), the parameter ratio of a
            // segment start point is the ratio of the segment index on the total segments count.
            int curveStartIndex = curveStartIndices[nearestCurveIndex];
            int curveEndIndex = curveStartIndices[nearestCurveIndex + 1];
            int curveInterpolatedSegmentsCount = curveEndIndex - curveStartIndex;
            float segmentStartParameterRatioOnCurve = (float)(nearestInterpolatedSegmentIndex - curveStartIndex) / curveInterpolatedSegmentsCount;

            // Now, we just need to add the small contribution of the progression on the segment itself.
            // Each segment covers 1 / curveInterpolatedSegmentsCount of the full curve, and the point is located at
            // [nearestPointParameterRatioOnSegment] across the current segment. So we must add:
            // nearestPointParameterRatioOnSegment / curveInterpolatedSegmentsCount
            nearestPointParameterRatioOnCurve = segmentStartParameterRatioOnCurve + nearestPointParameterRatioOnSegment / curveInterpolatedSegmentsCount;

            // Note that nearestPoint is not a real curve point: it is just a point on the interpolated segment,
            // so it may not be exactly on the curve.
            // However, it is preferred to an actual curve point (that we would recompute with InterpolateBezier
            // passing the curve control points and nearestPointParameterRatioOnCurve), because it will be drawn
            // as a Handle on the curve, itself drawn via interpolation.
            // So, it is better visually to drawn a nearest point following the approximate segments than a point
            // following the invisible perfect curve, but leaving the trail of visible segments when zooming enough.

            return distance;
        }

        private void DoLayout(bool readyToAddKeyPoint, bool readyToInsertKeyPoint, int readyToRemoveKeyPointIndex,
            int addControlID, int insertControlID, int removeControlID)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            if (eventType == EventType.Layout || eventType == EventType.MouseMove)
            {
                if (readyToAddKeyPoint)
                {
                    // Wherever we click, adding key point is the prioritized action, so add it as default control
                    // and flag action as ready (we still need to check for mouse button down)
                    HandleUtility.AddDefaultControl(addControlID);
                }
                else if (readyToRemoveKeyPointIndex != -1)
                {
                    HandleUtility.AddDefaultControl(removeControlID);
                }
                else if (readyToInsertKeyPoint)
                {
                    HandleUtility.AddDefaultControl(insertControlID);
                }
            }
        }

        private void HandleEditInput(BezierPath2D path, bool readyToAddKeyPoint, int readyToRemoveKeyPointIndex,
            bool readyToInsertKeyPoint, Vector2[] interpolatedPoints, List<int> curveStartIndices,
            int nearestCurveIndex, float nearestPointParameterRatioOnCurve, Vector2 nearestPoint)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            // Detect hold shift (readyToAddKeyPoint) and primary button click
            if (readyToAddKeyPoint && eventType == EventType.MouseDown && guiEvent.button == 0)
            {
                // Add new key point at the end of the path, at mouse position
                // The mouse position includes any path offset, but the key point must be stored without offset,
                // so we must subtract any path offset.
                // We have set Handles.matrix to an offsetMatrix in the scope HandleEditInput is called in,
                // so we can just apply the inverse matrix to the mouse position in world unit to subtract the offset.
                Vector2 newKeyPoint = GetMouseWorldPositionWithoutOffset(guiEvent);

                // As a variant of adding key point at the end, by holding control AND shift, user can insert a
                // key point at the beginning of the path. This is not handled in the readyToInsertKeyPoint group
                // because the latter is meant for insertion in the middle of a curve via curve split.
                // Instead, both Add (at end) and Insert at start add a key point at an extremity, calculating
                // smooth tangents, so they were put together.
                if (guiEvent.control)
                {
                    path.InsertKeyPointAtStart(newKeyPoint);
                }
                else
                {
                    path.AddKeyPoint(newKeyPoint);
                }

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

            // Detect cursor near curve (readyToInsertKeyPoint)
            if (readyToInsertKeyPoint)
            {
                if (eventType == EventType.Repaint)
                {
                    // Draw nearest curve with glow
                    // Inspired by LineHandle, draw the nearest curve in bold using Anti-Aliased polyline
                    // For convenience, we made curveStartIndices end with the last point of path,
                    // so curveStartIndices has path.GetCurvesCount() + 1 elements, and we don't have to handle the edge
                    // case where nearestCurveIndex == path.GetCurvesCount() - 1
                    int curveStartIndex = curveStartIndices[nearestCurveIndex];
                    int curveEndIndex = curveStartIndices[nearestCurveIndex + 1];
                    // Remember that range .. is exclusive, but we must draw including the end point, hence + 1
                    HandlesUtil.DrawAAPolyLine2D(interpolatedPoints[curveStartIndex..(curveEndIndex + 1)],
                        k_ActiveLineSegmentWidth, s_PathColor);

                    // Preview split point to add on click
                    Vector2 splitPointPosition = nearestPoint;

                    // We could call HandlesUtil.DrawSlider2D since EventType.Repaint cannot really move handles anyway,
                    // but to show our intention to only show a preview of the future split point, we extracted
                    // the visual parts of HandlesUtil.DrawSlider2D, namely capFunction(...) with color.
                    const float defaultHandleScreenSize = 0.1f;
                    float size = HandleUtility.GetHandleSize((Vector3)splitPointPosition) * defaultHandleScreenSize;
                    using (new Handles.DrawingScope(s_KeyPointColor))
                    {
                        // controlID is ignored for EventType.Repaint, so we don't pass insertControlID at all.
                        // It still works, because here, we are just drawing a purely visual point that looks like the
                        // future inserted key point, and it's not even a handle.
                        // Instead, after inserting a new key point at split position (below in EventType.MouseDown),
                        // the new key point will automatically catch the mouse click and further mouse move (dragging)
                        // with its own control ID.
                        Handles.CubeHandleCap(0, splitPointPosition, Quaternion.identity, size, EventType.Repaint);
                    }
                }
                else if (eventType == EventType.MouseDown && guiEvent.button == 0)
                {
                    // Split curve in two parts by inserting key point near cursor with tangents calculated so that
                    // we preserve the shape of the Bezier path (not velocity).
                    // Note: we could get splitPointPosition = nearestPoint
                    // immediately, but since SplitCurveAtParameterRatio requires a parameter ratio,
                    // we will compute that ratio and let the method deduce splitPointPosition.
                    float parameterRatio = nearestPointParameterRatioOnCurve;
                    path.SplitCurveAtParameterRatio(nearestCurveIndex, parameterRatio);

                    // Track inserted point index (key point just after split curve start)
                    // so user can immediately chain with handle drag as a normal key point
                    m_InsertedPointIndex = nearestCurveIndex + 1;

                    // Do NOT consume event with guiEvent.Use() here, it would prevent the newly created key point
                    // to catch mouse click, preventing user from chaining with handle dragging!
                }
            }

            // If event Mouse Up is detected, always clear m_InsertedPointIndex (if any)
            // (inspired by LineHandle.cs > s_InsertedIndex = -1)
            // Don't do this in the if (readyToInsertKeyPoint) block above, since during inserted key point dragging,
            // we are in Move mode (key point handle), so readyToInsertKeyPoint is false.
            if (eventType == EventType.MouseUp)
            {
                m_InsertedPointIndex = -1;
            }
        }

        /// Draw the interpolated path, without control points
        private void DrawInterpolatedPath(Vector2[] interpolatedPoints)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            if (eventType == EventType.Repaint)
            {
                HandlesUtil.DrawPolyLine2D(interpolatedPoints, s_PathColor);

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
                    interpolatedPoints.Add(BezierPath2D.InterpolateBezier(curve, t));
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
            Debug.AssertFormat(interpolatedPointsCount >= 2,
                "[BezierPath2DComponentEditor] DrawArrowInPathMiddle: interpolatedPoints.Count is {0}, " +
                "expected at least 2 points", interpolatedPointsCount);

            // find the approximate middle point of the path (it depends on how the path is interpolated)
            int midIndex = interpolatedPointsCount / 2;
            // calculate local tangent (from the previous point)
            Vector2 tangent = interpolatedPoints[midIndex] - interpolatedPoints[midIndex - 1];
            HandlesUtil.DrawArrowHead2D(interpolatedPoints[midIndex], tangent, s_PathColor);
        }

        /// Draw handles for the control points of the given path, with segments between key points and non-key control points
        /// to visualize tangents, with offset
        private void DrawControlPointHandles(BezierPath2D path, int keyPointToRemoveIndex, int insertControlID)
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
                    Color color = (i == keyPointToRemoveIndex ? s_KeyPointToRemoveColor : s_KeyPointColor);

                    // Key points normally use s_MovePointHash and each has its own ID to avoid confusing handles
                    // However, just after splitting a curve to insert a key point, we want to allow the user to chain
                    // with dragging the handle of the newly created key point for convenience.
                    // The behaviour is similar to Unity's native polyline editing (see LineHandle.cs), however instead
                    // of storing s_InsertedPointPosition and adding an extra handle dedicated to it on top of the
                    // standard key point handle, we prefer reusing the standard key point handle code, and simply
                    // setting the control ID to the same ID as the preview... wait
                    int keyPointControlID = i == m_InsertedPointIndex ?
                        insertControlID :
                        GUIUtility.GetControlID(s_MovePointHash, FocusType.Passive);

                    // Draw free move handle with a bigger size when hovered, to distinguish split point to add
                    // from existing key point to move
                    float screenSizeScale = HandleUtility.nearestControl == keyPointControlID ? 1.5f : 1f;
                    HandlesUtil.DrawSlider2D(ref keyPoint, color, screenSizeScale: screenSizeScale, controlID: keyPointControlID);
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
                        HandlesUtil.DrawSlider2D(ref tangentInPoint, s_TangentPointColor);

                        // If user either moved in tangent point directly, or indirectly via the associated key point,
                        // we must move the in tangent point accordingly
                        if (check.changed || keyPointDelta != Vector2.zero)
                        {
                            path.SetInTangentPoint(i, tangentInPoint + keyPointDelta);
                        }
                    }

                    // draw in tangent line
                    HandlesUtil.DrawLine(keyPoint, tangentInPoint, s_TangentColor);
                }

                // draw out tangent point and line, if any
                if (i < keyPointsCount - 1)
                {
                    // Do not add keyPointDelta here, it would double will the `+ keyPointDelta` in SetOutTangentPoint
                    var tangentOutPoint = path.GetOutTangentPoint(i);

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        // draw out tangent point
                        HandlesUtil.DrawSlider2D(ref tangentOutPoint, s_TangentPointColor);

                        // If user either moved out tangent point directly, or indirectly via the associated key point,
                        // we must move the out tangent point accordingly
                        if (check.changed || keyPointDelta != Vector2.zero)
                        {
                            path.SetOutTangentPoint(i, tangentOutPoint + keyPointDelta);
                        }
                    }

                    // draw out tangent line
                    HandlesUtil.DrawLine(keyPoint, tangentOutPoint, s_TangentColor);
                }
            }
        }
    }
}
