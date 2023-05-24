// Script explanation
// This is literally a script to prevent async methods leaking into the game after stopping Play in Editor.
// Place it on some EditorOnly game object in scenes that have this issue.
// We recommend using the SynchronizationContextSwitcher prefab provided along this script directly,
// as it already has the right tag and component setup.
// The issue is visible if you stop the editor while async methods are still running, and those methods
// do perceptible changes on the objects, such as adding a component
// (ex with Tweens: https://github.com/jeffreylanters/unity-tweens/issues/27).

using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

public class SynchronizationContextSwitcher : MonoBehaviour
{
    #if UNITY_EDITOR
    // Source: https://forum.unity.com/threads/non-stopping-async-method-after-in-editor-game-is-stopped.558283/#post-5168813
    // Author: jasonmcguirk
    void OnApplicationQuit()
    {
        var constructor = SynchronizationContext.Current.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {typeof(int)}, null);
        var newContext = constructor.Invoke(new object[] {Thread.CurrentThread.ManagedThreadId });
        SynchronizationContext.SetSynchronizationContext(newContext as SynchronizationContext);
    }
    #endif
}
