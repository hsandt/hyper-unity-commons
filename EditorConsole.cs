// http://answers.unity3d.com/questions/707636/clear-console-window.html

using UnityEngine;
using UnityEditor;
using System;

static class EditorConsole
{
    [MenuItem ("Tools/Clear Console _F10")] // CMD + SHIFT + C
    static void ClearConsole () {
        // This simply does "LogEntries.Clear()" the long way (reflection needed because LogEntries is an internal class)
        var logEntries = Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null,null);
    }
}
