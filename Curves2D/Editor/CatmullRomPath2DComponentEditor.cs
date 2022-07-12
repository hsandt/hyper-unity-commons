// References
//
// All source files can be found at
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector
//
// This editor is strongly based on BezierPath2DComponentEditor, which is itself based on Unity native editors
// such as collision editors and in particular LineHandle.cs (see BezierPath2DComponentEditor.cs for more information).

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonsHelper.Editor
{
    [CustomEditor(typeof(CatmullRomPath2DComponent))]
    public class CatmullRomPath2DComponentEditor : Path2DComponentEditor
    {
        private static class Styles
        {
            public static readonly GUIContent editModeButton = new GUIContent(
                EditorGUIUtility.IconContent("EditCollider").image,
                "Edit Catmull-Rom path.\n\n - Hold Ctrl before clicking to remove the nearest control point.\n" +
                "- Hold Shift before clicking to place a new control point at the end.\n" +
                "- Hold Ctrl and Shift before clicking to place a new control point at the start."
            );
        }


        private const float DOTTED_LINE_SCREEN_SPACE_SIZE = 5f;

        private static readonly Color s_ExtraPathColor = new Color(1f, 0.71f, 0.45f);


        private void OnSceneGUI()
        {
            var script = (CatmullRomPath2DComponent)target;

            if (script != null)
            {
                DrawEditablePath(script);
            }
        }

        protected override GUIContent GetEditModeButtonGUIContent()
        {
            return Styles.editModeButton;
        }

        /// Draw extra parts for the interpolated path, such as dotted lines linking to external control points
        /// not part of the path. This may not apply to all path types and may be left empty for some child classes.
        /// - Catmull-Rom draw dotted lines to link start/end points to first/last (external) control points
        protected override void DrawExtraInterpolatedPath(Path2D path)
        {
            Event guiEvent = Event.current;
            EventType eventType = guiEvent.type;

            // As a small optimization, check for event type early (DrawPolyLine2D also checks it anyway)
            if (eventType == EventType.Repaint)
            {
                // Draw dotted line between first (external) control point and actual path start (second point)
                HandlesUtil.DrawDottedLine2D(path.GetKeyPoint(0), path.GetPathStartPoint(), DOTTED_LINE_SCREEN_SPACE_SIZE, s_ExtraPathColor);

                // Draw dotted line between last (external) control point and actual path end (pre-last point)
                // GetKeyPoint takes an int, so we must compute index from count manually
                // If this is done often, we can also add an overload that takes an Index to support ^1
                HandlesUtil.DrawDottedLine2D(path.GetPathEndPoint(), path.GetKeyPoint(path.GetKeyPointsCount() - 1), DOTTED_LINE_SCREEN_SPACE_SIZE, s_ExtraPathColor);
            }
        }

        /// Draw handles for the control points of the given path, with segments between key points and non-key control points
        /// to visualize tangents, with offset
        protected override void DrawControlPointHandles(Path2D path, int keyPointToRemoveIndex, int insertControlID)
        {
            CatmullRomPath2D catmullRomPath = (CatmullRomPath2D)path;

            // Instead of iterating on curves, iterate on key points (= control points here)
            // so don't have to draw a last point after the loop.
            int keyPointsCount = catmullRomPath.GetControlPointsCount();
            for (int i = 0; i < keyPointsCount; ++i)
            {
                var keyPoint = catmullRomPath.GetControlPoint(i);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    Color color = (i == keyPointToRemoveIndex ? s_KeyPointToRemoveColor : s_KeyPointColor);

                    // Key points normally use s_MovePointHash and each has its own ID to avoid confusing handles
                    // However, just after splitting a curve to insert a key point, we want to allow the user to chain
                    // with dragging the handle of the newly created key point for convenience.
                    // The behaviour is similar to Unity's native polyline editing (see LineHandle.cs), however instead
                    // of storing m_SplitCurveIndex and adding an extra handle dedicated to it on top of the
                    // standard key point handle, we prefer reusing the standard key point handle code, and simply
                    // setting the control ID to the same ID as the preview split point control, so that the editor
                    // behaves as if hot control never changed, from the split point to the actual moved key point.

                    // Catmull-Rom: the inserted key point index is 2 after the split curve index
                    // see explanation in CatmullRomPath2D.SplitCurveAtParameterRatio
                    int insertedKeyPointIndex = m_SplitCurveIndex + 2;
                    int keyPointControlID = i == insertedKeyPointIndex
                        ? insertControlID
                        : GUIUtility.GetControlID(s_MovePointHash, FocusType.Passive);

                    // Draw free move handle with a bigger size when hovered, to distinguish split point to add
                    // from existing key point to move
                    float screenSizeScale = HandleUtility.nearestControl == keyPointControlID ? 1.5f : 1f;
                    HandlesUtil.DrawSlider2D(ref keyPoint, color, screenSizeScale: screenSizeScale,
                        controlID: keyPointControlID);

                    if (check.changed)
                    {
                        catmullRomPath.SetControlPoint(i, keyPoint);
                    }
                }
            }
        }
    }
}
