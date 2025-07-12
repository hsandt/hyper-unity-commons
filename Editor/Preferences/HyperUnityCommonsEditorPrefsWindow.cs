using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace HyperUnityCommons.Editor
{
    public class HyperUnityCommonsEditorPrefsWindow : EditorWindow
    {
        /* Queried elements */

        private Toggle m_RemoveUnloadedScenesDuringPlayToggle;
        private Toggle m_SimulateReleaseBuildToggle;


        [MenuItem("Window/Hyper Unity Commons/Editor Prefs Window")]
        private static void Init()
        {
            HyperUnityCommonsEditorPrefsWindow window = GetWindow<HyperUnityCommonsEditorPrefsWindow>();
            window.titleContent = new GUIContent("Hyper Unity Commons Editor Prefs Window");
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            string assetPath =
                "Packages/com.longnguyenhuu.hyper-unity-commons/Editor/Preferences/HyperUnityCommonsEditorPrefsWindow.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            Debug.AssertFormat(visualTree != null,
                "[HyperUnityCommonsEditorPrefsWindow] No VisualTreeAsset found at '{0}'", assetPath);
            visualTree.CloneTree(root);

            // Query existing elements
            m_RemoveUnloadedScenesDuringPlayToggle = root.Q<Toggle>("RemoveUnloadedScenesDuringPlayToggle");
            Debug.AssertFormat(m_RemoveUnloadedScenesDuringPlayToggle != null, visualTree,
                "[HyperUnityCommonsEditorPrefsWindow] No Toggle 'RemoveUnloadedScenesDuringPlayToggle' found on Hyper Unity Commons Prefs Window UXML");

            m_SimulateReleaseBuildToggle = root.Q<Toggle>("SimulateReleaseBuildToggle");
            Debug.AssertFormat(m_SimulateReleaseBuildToggle != null, visualTree,
                "[HyperUnityCommonsEditorPrefsWindow] No Toggle 'SimulateReleaseBuildToggle' found on Hyper Unity Commons Prefs Window UXML");

            // Initialise toggles and bind callbacks
            m_RemoveUnloadedScenesDuringPlayToggle.SetValueWithoutNotify(HyperPrefs.GetRemoveUnloadedScenesDuringPlayPref());
            m_RemoveUnloadedScenesDuringPlayToggle.RegisterValueChangedCallback(
                changeEvent => HyperPrefs.SetRemoveUnloadedScenesDuringPlayPref(changeEvent.newValue));

            m_SimulateReleaseBuildToggle.SetValueWithoutNotify(HyperPrefs.GetSimulateReleaseBuildPref());
            m_SimulateReleaseBuildToggle.RegisterValueChangedCallback(
                changeEvent => HyperPrefs.SetSimulateReleaseBuildPref(changeEvent.newValue));
        }
    }
}
