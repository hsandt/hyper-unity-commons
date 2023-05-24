using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HyperUnityCommons;

/// Application Manager
/// It handles things related to the application and window, but not to a gamestate in particular
/// We recommend creating it and making it DontDestroyOnLoad via some PersistentManagersGenerator (specific to project)
/// SEO: before other managers and common components in general so that IsQuitting can reliably be checked
public class AppManager : SingletonManager<AppManager>
{
    [Header("Parameters")]

    [SerializeField, Tooltip("Target framerate. -1 to keep platform default, using VSync if any.")]
    private int targetFrameRate = -1;

    [SerializeField, Tooltip("When using quick toggle fullscreen, use this to set window width.")]
    private int quickToggleFullScreenWindowWidth = 1280;

    [SerializeField, Tooltip("When using quick toggle fullscreen, use this to set window height.")]
    private int quickToggleFullScreenWindowHeight = 720;


    /* Events */

    /// Event sent on screen resolution change
    /// Any entity that places UI elements based on screen resolution
    /// should register to it.
    public event Action screenResolutionChanged;


    /* State */

    /// Tracked last screen width, to detect screen dimension change
    private int m_LastScreenWidth;

    /// Tracked last screen height, to detect screen dimension change
    private int m_LastScreenHeight;

    /// Flag tracking if application is quitting (in the editor, it means we are quitting play mode)
    private bool m_IsQuitting = false;


    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        m_LastScreenWidth = Screen.width;
        m_LastScreenHeight = Screen.height;
    }

    private void OnApplicationQuit()
    {
        // Set flag so other components (which should be after AppManager in SEO) can check if code is executed
        // as part of Application / Play Mode Quit. Particularly useful inside OnDestroy and other methods called on
        // scene closure but also Application / Play Mode Quit.
        // Note that in practice, AppManager will be destroyed and Instance set to null by the time we call callbacks
        // on other scripts, so in fact, we ony need to check `if (AppManager.Instance)`.
        // But his relies on game object destruction order, so to be safer, we recommend to always check
        // `if (AppManager.IsNotQuitting())`, which includes both checks.
        m_IsQuitting = true;
    }

    /// Return true if AppManager still exists (and it should always exist until we quit)
    /// and we are not quitting
    public static bool IsNotQuitting()
    {
        return Instance && !Instance.m_IsQuitting;
    }

    private void Update()
    {
        // Check for Screen width or height change (including toggle fullscreen)
        if (m_LastScreenWidth != Screen.width || m_LastScreenHeight != Screen.height)
        {
            // Update last value
            m_LastScreenWidth = Screen.width;
            m_LastScreenHeight = Screen.height;

            // Notify anyone who registered an event callback, so they can react
            screenResolutionChanged?.Invoke();
        }
    }

    /// If app is in fullscreen, switch to windowed 720p
    /// If app is not in fullscreen, switch to exclusive fullscreen at current monitor resolution
    /// This is useful to allow player to quickly toggle fullscreen when the settings menu is not ready,
    /// such as in prototypes and jam games, but it does not handle edge cases (monitor doesn't support 1280x720,
    /// user may prefer Exclusive Fullscreen on Windows and Maximized Window on OSX).
    /// In addition, if Project Settings > Player > Standalone Player Options > Allow Fullscreen Switch is checked,
    /// native OS bindings will be enabled to toggle fullscreen, so avoid calling this if player is on a platform
    /// where your custom binding is already mapped to toggle fullscreen.
    /// Eventually, prefer a proper settings menu to change resolution.
    public void QuickToggleFullScreen()
    {
        #if !UNITY_EDITOR
        Debug.Log("[AppManager] Toggle fullscreen (ignored in Editor)");
        #else

        if (Screen.fullScreen)
        {
            // Only changing fullscreen mode to Windowed would keep a very big window, and adding the window bar
            // dimensions it would even bleed out of the screen. So we must also set the resolution to something lower
            // than the max resolution. Technically we should check all available resolutions and pick one in the middle
            // so it works with every monitor, but for this quick helper method, we can just use the values set as
            // parameters of this script.
            Screen.SetResolution(quickToggleFullScreenWindowWidth, quickToggleFullScreenWindowHeight, FullScreenMode.Windowed);
        }
        else
        {
            // Note that current resolution is the native resolution
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }

        // Note: next Update should invoke screenResolutionChanged if any callbacks were registered to it

        #endif
    }
}
