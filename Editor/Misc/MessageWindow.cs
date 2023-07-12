using System.Collections;
using UnityEngine;
using UnityEditor;

namespace HyperUnityCommons.Editor
{
    public class MessageWindow : EditorWindow
    {
        /* Parameters */

        // Name of method to call via messaging
        private string m_MethodName = "Setup";


        [MenuItem("Window/Hyper Unity Commons/Message Window")]
        private static void ShowWindow()
        {
            GetWindow<MessageWindow>("Message Window");
        }

        void OnGUI()
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                string newMethodName = EditorGUILayout.TextField(
                    new GUIContent("Method name", "Name of method to call via messaging"),
                    m_MethodName);

                if (change.changed)
                {
                    m_MethodName = newMethodName;
                }
            }

            if (Application.isPlaying)
            {
                GameObject selectedGameObject = Selection.activeGameObject;
                if (selectedGameObject != null)
                {
                    if (GUILayout.Button("Call method on selection (single)"))
                    {
                        selectedGameObject.SendMessage(m_MethodName);
                    }
                }
                else
                {
                    GUILayout.Label("No game object selected");
                }
            }
            else
            {
                GUILayout.Label("Inactive outside Play mode");
            }
        }
    }
}
