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
