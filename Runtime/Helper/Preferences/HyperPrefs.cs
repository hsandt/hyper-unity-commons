using UnityEditor;
using UnityEngine;

namespace HyperUnityCommons
{
    public class HyperPrefs : MonoBehaviour
    {
        public const string EDITOR_PREFS_NAMESPACE = "HyperUnityCommons.EditorPrefsWindow";

        /// If true, RemoveUnloadedScenesDuringPlay script will remove any added but unloaded scenes in the editor
        public static readonly string RemoveUnloadedScenesDuringPlayKey =
            $"{EDITOR_PREFS_NAMESPACE}.RemoveUnloadedScenesDuringPlay";

        /// If true, PlayModeUtil.GetPlayMode will check this to return PlayMode
        public static readonly string SimulateReleaseBuildKey =
            $"{EDITOR_PREFS_NAMESPACE}.SimulateReleaseBuild";


        /* Preference accessors */

        public static bool GetRemoveUnloadedScenesDuringPlayPref()
        {
            #if UNITY_EDITOR
            return EditorPrefs.GetBool(RemoveUnloadedScenesDuringPlayKey);
            #else
            return false;
            #endif
        }

        #if UNITY_EDITOR
        public static void SetRemoveUnloadedScenesDuringPlayPref(bool value)
        {
            EditorPrefs.SetBool(RemoveUnloadedScenesDuringPlayKey, value);
        }
        #endif

        public static bool GetSimulateReleaseBuildPref()
        {
            #if UNITY_EDITOR
            return EditorPrefs.GetBool(SimulateReleaseBuildKey);
            #else
            return false;
            #endif
        }

        #if UNITY_EDITOR
        public static void SetSimulateReleaseBuildPref(bool value)
        {
            EditorPrefs.SetBool(SimulateReleaseBuildKey, value);
        }
        #endif
    }
}
