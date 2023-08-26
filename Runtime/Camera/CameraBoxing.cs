using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Place this component as sibling of a Camera component to enforce boxing via target rectangle to a parametered
    /// target resolution, adding empty space (often black bars) on the top and bottom or side as needed.
    /// Unlike Camera2DAutoZoomToFit, this is a true boxing and will not show parts of the world above and below.
    /// In addition, it works with any type of camera.
    /// Note that UI is not affected, and may still display on the empty space areas.
    /// SEO: after AppManager
    public class CameraBoxing : MonoBehaviour
    {
        [Header("Parameters")]

        [SerializeField, Tooltip("Target width of the camera render area (only the aspect ratio width : height matters)")]
        private int targetWidth = 1920;

        [SerializeField, Tooltip("Target height of the camera render area (only the aspect ratio width : height matters)")]
        private int targetHeight = 1080;


        /* Sibling components */

        private Camera m_Camera;


        private void Awake()
        {
            m_Camera = this.GetComponentOrFail<Camera>();
        }

        private void OnEnable()
        {
            // SEO ensures AppManager has been initialized
            // But in the editor, OnEnable is called on Stop playing, so still check Instance
            if (AppManager.Instance != null)
            {
                AppManager.Instance.screenResolutionChanged += OnScreenResolutionChanged;
            }

            // Call the callback once in case we're already in non-target aspect ratio
            OnScreenResolutionChanged();
        }

        private void OnDisable()
        {
            // This is called on Stop playing in Editor where other objects
            // may have been destroyed, so add a null check to avoid errors
            if (AppManager.Instance != null)
            {
                AppManager.Instance.screenResolutionChanged -= OnScreenResolutionChanged;
            }
        }

        private void OnScreenResolutionChanged()
        {
            // Code snippet based on: https://forum.unity.com/threads/how-to-force-black-bar-widescreen.21238/
            // post by Acegikmo
            // Changes by huulong:
            // - instead of using main camera, use sibling Camera component so we can apply this to Base + Overlay
            //   cameras
            // - width/height are parametered
            Vector2 resTarget = new Vector2( targetWidth, targetHeight );
            Vector2 resViewport = new Vector2( Screen.width, Screen.height );
            Vector2 resNormalized = resTarget / resViewport; // target res in viewport space
            Vector2 size = resNormalized / Mathf.Max( resNormalized.x, resNormalized.y );
            m_Camera.rect = new Rect( default, size ) { center = new Vector2( 0.5f, 0.5f ) };
        }
    }
}
