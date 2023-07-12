using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using HyperUnityCommons;
using ElRaccoone.Tweens;

/// Main component of Canvas Splash Screen
public class CanvasSplashScreen : MonoBehaviour
{
    [Header("Parameters data")]

    [Tooltip("Splash Screen Parameters")]
    public SplashScreenParameters splashScreenParameters;


    [Header("Scene references")]

    [Tooltip("Splash background")]
    public Image splashBackground;

    [Tooltip("Splash logo (any Graphic works, image or text)")]
    public Graphic splashLogo;


    #if UNITY_EDITOR

    [Header("Editor only")]

    [SerializeField, Tooltip("Check to skip splash screen for quicker iterations")]
    private bool editorSkipSplashScreen = false;

    #endif


    private void Awake()
    {
        DebugUtil.Assert(splashScreenParameters != null, "No Splash Screen Parameters asset set on Canvas Splash Screen", this);
        DebugUtil.Assert(splashBackground != null, "No Splash Background set on Canvas Splash Screen", this);
        DebugUtil.Assert(splashLogo != null, "No Splash Logo set on Canvas Splash Screen", this);
    }

    private void Start()
    {
        if (splashLogo != null)
        {
            // Hide logo via transparency so it's ready for the alpha tween
            SetSplashLogoTransparent();
        }
    }

    private void SetSplashLogoTransparent()
    {
        Color splashLogoColor = splashLogo.color;
        splashLogoColor.a = 0f;
        splashLogo.color = splashLogoColor;
    }

    /// Show splash logo with fading, but stop just before fading out background itself
    public Task PlaySplashScreenSequenceAsync()
    {
        return PlaySplashScreenSequenceAsync(CancellationToken.None);
    }

    /// Show splash logo with fading, but stop just before fading out background itself
    /// Takes a cancellation token to allow skipping splash screen (you must call FinishAllTweensImmediately
    /// and then cancel the CancellationTokenSource for it to fully work)
    public async Task PlaySplashScreenSequenceAsync(CancellationToken cancellationToken)
    {
        #if UNITY_EDITOR
        if (editorSkipSplashScreen)
        {
            return;
        }
        #endif

        if (splashLogo != null)
        {
            // Note that Tween Await() doesn't take a cancellationToken, but it stops on Cancel via Decommission,
            // so calling StopAllTweens() during a tween will effectively make the execution proceed to
            // the Delay after it, so you can cancel the task immediately.
            await splashLogo.TweenGraphicAlpha(1f, splashScreenParameters.logoFadeInDuration).Await();
            await Task.Delay(TimeSpan.FromSeconds(splashScreenParameters.logoStayDuration), cancellationToken);
            await splashLogo.TweenGraphicAlpha(0f, splashScreenParameters.logoFadeOutDuration).Await();
            await Task.Delay(TimeSpan.FromSeconds(splashScreenParameters.backgroundStayAfterLogoDuration), cancellationToken);
        }
    }

    /// Cancel all tweens and set widgets to their final state at the end of the splash screen sequence
    /// (excluding BG fade out)
    /// Call this and then cancel the CancellationTokenSource you used to pass a cancellation token to
    /// PlaySplashScreenSequenceAsync to fully skip the splash screen sequence (excluding BG fade out)
    public void FinishAllTweensImmediately()
    {
        splashLogo.TweenCancelAll();
        SetSplashLogoTransparent();
    }

    /// Fade background out
    public async Task FadeOutAsync()
    {
        #if UNITY_EDITOR
        if (editorSkipSplashScreen)
        {
            // Deactivate game object immediately
            gameObject.SetActive(false);
            return;
        }
        #endif

        if (splashLogo != null)
        {
            await splashBackground.TweenGraphicAlpha(0f, splashScreenParameters.backgroundFadeOutDuration).Await();

            // Graphics are now invisible, deactivate game object completely for cleanup
            gameObject.SetActive(false);
        }
    }
}
