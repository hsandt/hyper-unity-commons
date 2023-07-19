using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HyperUnityCommons
{
    // EXPERIMENTAL: right now, it finds components even on prefabs and has a lot of false positive...

    /// When editor preference HyperUnityCommons.EditorPrefsWindow.DetectRectTransformOverride
    /// (see HyperUnityCommonsEditorPrefsWindow) is set to true,
    /// as long as this script is loaded in project, on Enter Play Mode, find all game objects with a
    /// DetectRectTransformOverrideFlag component and log an error to warn user if RectTransform has overridden
    /// properties, especially dimensions set to 0 and modified anchors/pivots, the most common cause of bad prefab
    /// instances set in Scene (very common for pooled UI objects)
    /// There is also a menu item to directly call it whenever you want
    [InitializeOnLoad]
    public static class DetectRectTransformOverride
    {
        private static bool s_ShouldLogErrorAfterEnteringPlayMode = false;


        [MenuItem ("Debug/Run Detect Rect Transform Override")]
        public static void RunDetectRectTransformOverride()
        {
            DetectRectTransformOverrideFlag[] flags = Object.FindObjectsOfType<DetectRectTransformOverrideFlag>();
            foreach (DetectRectTransformOverrideFlag flag in flags)
            {
                GameObject gameObject = flag.gameObject;
                PropertyModification[] propertyModifications = PrefabUtility.GetPropertyModifications(gameObject);

                if (propertyModifications.Length > 0)
                {
                    foreach (PropertyModification propertyModification in propertyModifications)
                    {
                        if (propertyModification.propertyPath.Contains("m_SizeDelta") && propertyModification.value == "0")
                        {
                            Debug.LogWarningFormat(propertyModification.target,
                                "[DetectRectTransformOverride] Detected suspicious property override on {0}: {1} -> {2}",
                                propertyModification.target, propertyModification.propertyPath, propertyModification.value);
                        }
                    }
                }
            }
        }

        // DISABLED in code as still experimental and cannot be disabled in Editor Prefs
        /*
        static DetectRectTransformOverride()
        {
            EditorApplication.playModeStateChanged += ModeStateChanged;
        }
        */

        private static void ModeStateChanged(PlayModeStateChange state)
        {
            // Make sure to work with state change on EditMode side, not PlayMode side, where we still have access
            // to prefab instances as defined in the scene, and properly linked to the prefab assets
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                RunDetectRectTransformOverride();

                // When Clear On Play is checked, the previous logs will be lost, so it's better to log again after
                // entering Play Mode. We may not be able to log the full warnings with the right context to ping them,
                // but at least we can log an error to notify user to run Debug/Run Detect Rect Transform Override
                // manually
                s_ShouldLogErrorAfterEnteringPlayMode = true;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (s_ShouldLogErrorAfterEnteringPlayMode)
                {
                    // Consume flag
                    s_ShouldLogErrorAfterEnteringPlayMode = false;

                    // Log now
                    Debug.LogError("[DetectRectTransformOverride] ModeStateChanged: when exiting Edit mode, " +
                        "we detected some flagged RectTransform with overridden properties. If you missed the logs " +
                        "due to Clear On Play being checked in console, please run Debug/Run Detect Rect Transform Override " +
                        "directly from the top menu.");
                }
            }
        }
    }
}
