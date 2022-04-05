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
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
{
    [CustomEditor(typeof(BezierPath2DComponent))]
    public class BezierPath2DComponentEditor : Path2DComponentEditor
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


        private static readonly Color s_TangentPointColor = ColorUtil.orange;
        private static readonly Color s_TangentColor = Color.yellow;


        private void OnSceneGUI()
        {
            var script = (BezierPath2DComponent) target;

            if (script != null)
            {
                DrawEditablePath(script);
            }
        }

        protected override GUIContent GetEditModeButtonGUIContent()
        {
            return Styles.editModeButton;
        }

        /// Draw handles for the control points of the given path, with segments between key points and non-key control points
        /// to visualize tangents, with offset
        protected override void DrawControlPointHandles(Path2D path, int keyPointToRemoveIndex, int insertControlID)
        {
            BezierPath2D bezierPath = (BezierPath2D) path;

            // Instead of iterating on curves, iterate on key points.
            // This allows us to handle a key point and its 2 tangents (1 tangent for start and end key points)
            // at the same time to move them along, making code more symmetrical than when handling the 2 unrelated
            // in and out tangents of a given curve.
            // With this, we also don't have to draw a last point after the loop.
            int keyPointsCount = bezierPath.GetKeyPointsCount();
            for (int i = 0; i < keyPointsCount; ++i)
            {
                // Moving key point also moves attached tangent in and out (if any), so track key point move delta
                Vector2 keyPointDelta;

                var keyPoint = bezierPath.GetKeyPoint(i);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var oldKeyPoint = keyPoint;
                    Color color = (i == keyPointToRemoveIndex ? s_KeyPointToRemoveColor : s_KeyPointColor);

                    // Key points normally use s_MovePointHash and each has its own ID to avoid confusing handles
                    // However, just after splitting a curve to insert a key point, we want to allow the user to chain
                    // with dragging the handle of the newly created key point for convenience.
                    // The behaviour is similar to Unity's native polyline editing (see LineHandle.cs), however instead
                    // of storing m_SplitCurveIndex and adding an extra handle dedicated to it on top of the
                    // standard key point handle, we prefer reusing the standard key point handle code, and simply
                    // setting the control ID to the same ID as the preview split point control, so that the editor
                    // behaves as if hot control never changed, from the split point to the actual moved key point.

                    // Bezier: the inserted key point index is 1 after the split curve index
                    // see explanation in BezierPath2D.SplitCurveAtParameterRatio
                    int insertedKeyPointIndex = m_SplitCurveIndex + 1;
                    int keyPointControlID = i == insertedKeyPointIndex ?
                        insertControlID :
                        GUIUtility.GetControlID(s_MovePointHash, FocusType.Passive);

                    // Draw free move handle with a bigger size when hovered, to distinguish split point to add
                    // from existing key point to move
                    float screenSizeScale = HandleUtility.nearestControl == keyPointControlID ? 1.5f : 1f;
                    HandlesUtil.DrawSlider2D(ref keyPoint, color, screenSizeScale: screenSizeScale, controlID: keyPointControlID);
                    keyPointDelta = keyPoint - oldKeyPoint;

                    if (check.changed)
                    {
                        bezierPath.SetKeyPoint(i, keyPoint);
                    }
                }

                // draw in tangent point and line, if any
                if (i > 0)
                {
                    // Do not add keyPointDelta here, it would double will the `+ keyPointDelta` in SetInTangentPoint
                    var tangentInPoint = bezierPath.GetInTangentPoint(i);

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        // draw in tangent point
                        HandlesUtil.DrawSlider2D(ref tangentInPoint, s_TangentPointColor);

                        // If user either moved in tangent point directly, or indirectly via the associated key point,
                        // we must move the in tangent point accordingly
                        if (check.changed || keyPointDelta != Vector2.zero)
                        {
                            bezierPath.SetInTangentPoint(i, tangentInPoint + keyPointDelta);
                        }
                    }

                    // draw in tangent line
                    HandlesUtil.DrawLine(keyPoint, tangentInPoint, s_TangentColor);
                }

                // draw out tangent point and line, if any
                if (i < keyPointsCount - 1)
                {
                    // Do not add keyPointDelta here, it would double will the `+ keyPointDelta` in SetOutTangentPoint
                    var tangentOutPoint = bezierPath.GetOutTangentPoint(i);

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        // draw out tangent point
                        HandlesUtil.DrawSlider2D(ref tangentOutPoint, s_TangentPointColor);

                        // If user either moved out tangent point directly, or indirectly via the associated key point,
                        // we must move the out tangent point accordingly
                        if (check.changed || keyPointDelta != Vector2.zero)
                        {
                            bezierPath.SetOutTangentPoint(i, tangentOutPoint + keyPointDelta);
                        }
                    }

                    // draw out tangent line
                    HandlesUtil.DrawLine(keyPoint, tangentOutPoint, s_TangentColor);
                }
            }
        }
    }
}
