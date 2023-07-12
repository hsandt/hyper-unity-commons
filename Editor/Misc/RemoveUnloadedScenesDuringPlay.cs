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
        private static SceneSetup[] sceneSetupsBackup = null;


        static RemoveUnloadedScenesDuringPlay()
        {
            EditorApplication.playModeStateChanged += ModeStateChanged;
        }

        private static void ModeStateChanged (PlayModeStateChange state)
        {
            // Make sure to work with state change on EditMode side, not PlayMode side, where RestoreSceneManagerSetup
            // call would be invalid and cause an error
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (HyperUnityCommonsEditorPrefsWindow.GetRemoveUnloadedScenesDuringPlayKeyPref())
                {
                    sceneSetupsBackup = EditorSceneManager.GetSceneManagerSetup();
                    SceneSetup[] loadedSceneSetupsOnly = sceneSetupsBackup.Where(sceneSetup => sceneSetup.isLoaded).ToArray();
                    EditorSceneManager.RestoreSceneManagerSetup(loadedSceneSetupsOnly);
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
                if (sceneSetupsBackup != null)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(sceneSetupsBackup);
                    sceneSetupsBackup = null;
                }
            }
        }
    }
}
