using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Splash Screen parameters
[CreateAssetMenu(fileName = "SplashScreenParameters", menuName = "Menu/Splash Screen Parameters")]
public class SplashScreenParameters : ScriptableObject
{
    [Tooltip("Fade-in duration of each logo (s)")]
    public float logoFadeInDuration = 1f;

    [Tooltip("Stay duration of each logo (s)")]
    public float logoStayDuration = 1f;

    [Tooltip("Fade-out duration of each logo (s)")]
    public float logoFadeOutDuration = 1f;

    [Tooltip("Stay duration of background after logo has completely faded out (s)")]
    public float backgroundStayAfterLogoDuration = 0.5f;

    [Tooltip("Fade-out duration of background (s)")]
    public float backgroundFadeOutDuration = 0.5f;
}
