using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using ElRaccoone.Tweens;
using HyperUnityCommons;

/// Splash Screen Manager
[Obsolete("Use CanvasSplashScreen (not singleton) instead, then use Tag or subclass/sibling singleton " +
    "component if you need global access.")]
public class SplashScreenManager : SingletonManager<SplashScreenManager>
{
    [Header("Parameters data")]

    [Tooltip("Splash Screen Parameters")]
    public SplashScreenParameters splashScreenParameters;


    [Header("Scene references")]

    [Tooltip("Splash logo (any Graphic works, image or text)")]
    public Graphic splashLogo;


    #if UNITY_EDITOR

    [Header("Editor only")]

    [SerializeField, Tooltip("Check to skip splash screen for quicker iterations")]
    private bool skipSplashScreen = false;

    #endif


    protected override void Init()
    {
        base.Init();

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(splashScreenParameters != null, "No Splash Screen Parameters asset set on Splash Screen Manager", this);
        #endif
    }

    private void Start()
    {
        if (splashLogo != null)
        {
            // Hide logo via transparency so it's ready for the alpha tween
            Color splashLogoColor = splashLogo.color;
            splashLogoColor.a = 0f;
            splashLogo.color = splashLogoColor;
        }
    }

    public async Task PlaySplashScreenSequence()
    {
        #if UNITY_EDITOR
        if (skipSplashScreen)
        {
            return;
        }
        #endif

        if (splashLogo != null)
        {
            await splashLogo.TweenGraphicAlpha(1f, splashScreenParameters.logoFadeInDuration).Await();
            await Task.Delay(TimeSpan.FromSeconds(splashScreenParameters.logoStayDuration));
            await splashLogo.TweenGraphicAlpha(0f, splashScreenParameters.logoFadeOutDuration).Await();
        }
    }
}
