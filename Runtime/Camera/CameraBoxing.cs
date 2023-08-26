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


        /* State */

        /// This flag exists to delay the refresh to avoid using Screen.width/height which are known to return
        /// Inspector size when called inside OnEnable/OnDisable due to toggling component via inspector:
        /// https://issuetracker.unity3d.com/issues/screen-dot-width-slash-screen-dot-height-in-onenable-shows-inspector-window-size-when-the-component-is-enabled-by-a-toggle-in-inspector-window
        private bool m_NeedsRefresh = false;


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

            // On scene load / camera spawn, we must do a initial refresh to get the correct boxing
            // If we disabled the component momentarily and just re-enabled it, we also want a refresh now
            // However, as explained in flag doc comment, we must avoid using Screen.width/height in this context,
            // so delay call to OnScreenResolutionChanged to next Update using this flag
            m_NeedsRefresh = true;
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

        private void Update()
        {
            if (m_NeedsRefresh)
            {
                m_NeedsRefresh = false;
                OnScreenResolutionChanged();
            }
        }

        private void OnScreenResolutionChanged()
        {
            // Code snippet based on: https://forum.unity.com/threads/how-to-force-black-bar-widescreen.21238/
            // post by Acegikmo
            // Changes by huulong:
            // - instead of using main camera, use sibling Camera component so we can apply this to Base + Overlay
            //   cameras
            // - target width/height are parametered
            Vector2 resTarget = new Vector2( targetWidth, targetHeight );
            Vector2 resViewport = new Vector2( Screen.width, Screen.height );
            Vector2 resNormalized = resTarget / resViewport; // target res in viewport space
            Vector2 size = resNormalized / Mathf.Max( resNormalized.x, resNormalized.y );
            m_Camera.rect = new Rect( default, size ) { center = new Vector2( 0.5f, 0.5f ) };
        }
    }
}
