using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyperUnityCommons.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HyperUnityCommons
{
    /// When editor preference HyperUnityCommons.EditorPrefsWindow.RemoveUnloadedScenesDuringPlay
    /// (see HyperUnityCommonsEditorPrefsWindow) is set to true,
    /// as long as this script is loaded in project, remove all unloaded scenes added in editor Hierarchy before
    /// entering play mode. After exiting play mode, restore them.
    [InitializeOnLoad]
    public static class RemoveUnloadedScenesDuringPlay
    {
        /// Backup of scene setups before stripping unloaded ones
        private static SceneSetup[] s_SceneSetupsBackup = null;

        private static bool s_ShouldLogSaveScenesAfterEnteringPlayMode = false;


        static RemoveUnloadedScenesDuringPlay()
        {
            EditorApplication.playModeStateChanged += ModeStateChanged;
        }

        private static void ModeStateChanged(PlayModeStateChange state)
        {
            // Make sure to work with state change on EditMode side, not PlayMode side, where RestoreSceneManagerSetup
            // call would be invalid and cause an error
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (HyperUnityCommonsEditorPrefsWindow.GetRemoveUnloadedScenesDuringPlayKeyPref())
                {
                    // Check current scene setup array (do not fill s_SceneSetupsBackup yet, as it would mean that
                    // we need to restore backup later on PlayModeStateChange.EnteredEditMode)
                    SceneSetup[] currentSceneSetups = EditorSceneManager.GetSceneManagerSetup();
                    SceneSetup[] loadedSceneSetupsOnly = currentSceneSetups.Where(sceneSetup => sceneSetup.isLoaded).ToArray();

                    // No need to change anything if no scene was unloaded
                    // This also allows us not to have to save the scenes now and keep them dirty even after exit Play
                    // mode, in case there are unsaved changes
                    if (loadedSceneSetupsOnly.Length < currentSceneSetups.Length)
                    {
                        // This log will be cut if Clear On Play is active, but useful for debugging
                        Debug.LogFormat("[RemoveUnloadedScenesDuringPlay] ModeStateChanged: exiting Edit mode, " +
                            "detected {0} unloaded scenes ({1} loaded scenes out of {2} scenes). Saving scenes and " +
                            "removing unloaded scenes.",
                            currentSceneSetups.Length - loadedSceneSetupsOnly.Length,
                            loadedSceneSetupsOnly.Length, currentSceneSetups.Length);

                        // Save open scenes now (if needed), otherwise we'll lose dirty changes when switching scene setups
                        // The changes won't be present during Play, and we won't recover them after exit Play mode
                        // We should log it, as this is not the usual behavior (which preserves scenes dirty as mentioned above)
                        // But since many users have Clear On Play checked, it's better to log it after actually entering
                        // Play Mode so this log doesn't get cleared, so set flag for later
                        s_ShouldLogSaveScenesAfterEnteringPlayMode = true;
                        EditorSceneManager.SaveOpenScenes();

                        // Now, backup scenes and remove unloaded scenes
                        s_SceneSetupsBackup = currentSceneSetups;
                        EditorSceneManager.RestoreSceneManagerSetup(loadedSceneSetupsOnly);
                    }
                }
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (s_ShouldLogSaveScenesAfterEnteringPlayMode)
                {
                    // Consume flag
                    s_ShouldLogSaveScenesAfterEnteringPlayMode = false;

                    // Log now
                    Debug.Log("[RemoveUnloadedScenesDuringPlay] ModeStateChanged: when exiting Edit mode, " +
                        "some scenes were unloaded, so we saved any dirty scenes to allow scene setup change without " +
                        "losing local modifications");
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                // When going back to Edit mode, do not check pref as when we exiting it:
                // indeed, the user may have toggled the preference while in Play mode, which means its current value
                // is not what it was when entering Play mode, and it's no use trying to restore scenes that have not
                // changed, and reversely we should restore unloaded scenes if we stripped them earlier, independently
                // of the current pref value. So, be pragmatic and check if there is some backup array. If so, make
                // sure to consume it so we don't think there is one later by accident, even when we have not removed
                // any unloaded scenes due to disabling the pref.
                if (s_SceneSetupsBackup != null)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(s_SceneSetupsBackup);
                    s_SceneSetupsBackup = null;
                }
            }
        }
    }
}
