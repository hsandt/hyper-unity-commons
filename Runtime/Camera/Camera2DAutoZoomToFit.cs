using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Attach this script on an orthographic camera to make it change view height dynamically with aspect ratio,
    /// to guarantee that the camera shows the same amount of world horizontally when the aspect ratio is lower than
    /// the target one (more narrow screen), by increasing the orthographic size (zoom out).
    /// Unlike CameraBoxing, this is a orthographic true zoom and will keep showing parts of the world above and below
    /// instead of adding black bars.
    /// Note that UI is not affected, and may still display on the empty space areas.
    public class Camera2DAutoZoomToFit : MonoBehaviour
    {
        [Header("Parameters")]

        [SerializeField, Tooltip("Target aspect ratio that the camera will adjust to")]
        private float targetAspectRatio = 16f / 9f;


        /* Sibling components */

        private Camera m_Camera;


        /* Initial state */

        /// Initial orthographic size, defined in prefab/scene to fit the game world as we want for targetAspectRatio
        private float m_InitialOrthographicSize;


        private void Awake()
        {
            m_Camera = this.GetComponentOrFail<Camera>();

            m_InitialOrthographicSize = m_Camera.orthographicSize;

            // Refresh orthographic size once at the start, as if screen resolution had changed, since the event is not
            // sent by Unity on game start. This ensures that even if the game starts at aspect more narrow than target,
            // the camera zooms out properly.
            OnScreenResolutionChanged();
        }

        private void OnEnable()
        {
            // SEO ensures AppManager has been initialized
            // But in the editor, OnEnable is called on Stop playing, so still check Instance
            if (AppManager.Instance != null)
            {
                AppManager.Instance.screenResolutionChanged += OnScreenResolutionChanged;
            }
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
            // When aspect is more narrow than target aspect, apply rule of three with target and current aspect ratio
            // to get new, bigger orthographic size, showing more of the world above and below
            // (either actual things for open areas, or "black bars" if there is nothing to render beyond)
            // When aspect is wider, do nothing, as keeping the target orthographic size will naturally keep the wanted
            // part of the game world inside view, showing more of the world on the sides
            // (either actual things for open areas, or "black bars" if there is nothing to render beyond)
            float orthographicSizeMultiplier = Mathf.Max(1f, targetAspectRatio / m_Camera.aspect);

            // To avoid unwanted zoom out due to floating errors, snap multiplier to 1 if enough close to 1
            if (1f < orthographicSizeMultiplier && orthographicSizeMultiplier <= 1.001f)
            {
                orthographicSizeMultiplier = 1f;
            }

            m_Camera.orthographicSize = orthographicSizeMultiplier * m_InitialOrthographicSize;
        }
    }
}
