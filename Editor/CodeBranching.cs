using System;
using UnityEditor;
using UnityEngine;

public class BranchingCodeEditorWindow : EditorWindow
{

    private int branchingIndex;


    [MenuItem("Debug/Code Branching")]
    static void OpenWindow()
    {
        BranchingCodeEditorWindow editorWindow = (BranchingCodeEditorWindow) GetWindow(typeof(KeyEditorWindow), false, "Branching code");
        editorWindow.Show();
    }

}
