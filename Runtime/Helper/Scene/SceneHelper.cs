using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

using Eflatun.SceneReference;

namespace HyperUnityCommons
{
    public static class SceneHelper
    {
        #if COM_EFLATUN_SCENEREFERENCE

        /// <summary>
        /// Load scene asynchronously
        /// </summary>
        /// <param name="sceneReference">Scene to load</param>
        /// <param name="loadSceneMode">Whether to load as Single or Additive scene</param>
        /// <param name="loadAsActiveScene">If true, the loaded scene will be set as Active scene</param>
        /// <param name="isDonePollingPeriodSeconds">Period (seconds) used to poll whether loading is finished</param>
        /// <param name="context">Optional context for debugging</param>
        /// <param name="debugSceneReferenceName">Optional scene name or full symbol with namespace used to access scene reference for debugging</param>
        public static async Task LoadSceneAsync(SceneReference sceneReference, LoadSceneMode loadSceneMode,
            bool loadAsActiveScene = false, double isDonePollingPeriodSeconds = 0.1f,
            Object context = null, string debugSceneReferenceName = null)
        {
            if (string.IsNullOrEmpty(sceneReference.AssetGuidHex))
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] LoadSceneAsync: Scene reference '{0}' is not set, cannot load scene",
                    debugSceneReferenceName);
                return;
            }

            // Load scene with passed mode and wait
            Scene existingLoadedScene = sceneReference.LoadedScene;
            if (existingLoadedScene.IsValid())
            {
                if (existingLoadedScene.isLoaded)
                {
                    DebugUtil.LogWarningFormat("[SceneHelper] LoadSceneAsync: a loaded instance of scene reference '{0}' at '{1}' " +
                        "has been found, so we are not loading the same scene again. If this happened when testing additive " +
                        "scenes directly in the editor (so some scenes have already been added), then it won't be an issue " +
                        "in build. If you really need to add the same scene multiple scenes, add a parameter to this " +
                        "method to support this.",
                        debugSceneReferenceName, existingLoadedScene.path);
                    return;
                }
                else
                {
                    DebugUtil.LogWarningFormat("[SceneHelper] LoadSceneAsync: an unloaded instance of scene reference '{0}' at '{1}' " +
                        "has been found, which means we are going to load *another* instance of that scene, but " +
                        "SceneManager.GetScene... API (also used by sceneReference.LoadedScene) will return that first " +
                        "unloaded instance, which is not what we want. If loadAsActiveScene is true (it is {2} in this call) " +
                        "it will also cause an error on SceneManager.SetActiveScene more below, trying to set an unloaded " +
                        "scene at active. If this happened when testing additive scenes directly in the editor " +
                        "(so some scenes have already been added), then it won't be an issue in build. " +
                        "We recommend however to enable RemoveUnloadedScenesDuringPlay editor preference in " +
                        "Window/Hyper Unity Commons/Editor Prefs Window to avoid this situation.",
                        debugSceneReferenceName, existingLoadedScene.path, loadAsActiveScene);
                }
            }

            AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(sceneReference.BuildIndex, loadSceneMode);
            await AwaitOperationIsDone(isDonePollingPeriodSeconds, asyncLoading);

            if (loadAsActiveScene)
            {
                SceneManager.SetActiveScene(sceneReference.LoadedScene);
            }
        }

        /// <summary>
        /// Unload scene asynchronously
        /// </summary>
        /// <param name="sceneReference">Scene to unload</param>
        /// <param name="isDonePollingPeriodSeconds">Period (seconds) used to poll whether unloading is finished</param>
        /// <param name="context">Optional context for debugging</param>
        /// <param name="debugSceneReferenceName">Optional scene name or full symbol with namespace used to access scene reference for debugging</param>
        public static async Task UnloadSceneAsync(SceneReference sceneReference, double isDonePollingPeriodSeconds,
            Object context = null, string debugSceneReferenceName = null)
        {
            if (string.IsNullOrEmpty(sceneReference.AssetGuidHex))
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' is not set, cannot unload scene",
                    debugSceneReferenceName);
                return;
            }

            if (!sceneReference.LoadedScene.IsValid())
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' at '{1}' is not loaded, " +
                    "cannot unload scene",
                    debugSceneReferenceName, sceneReference.Path);
                return;
            }

            if (!sceneReference.LoadedScene.isLoaded)
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' at '{1}' is active but not loaded, " +
                    "cannot unload scene. If this happened when testing additive scenes directly in the editor " +
                    "(so some scenes have already been added), then it won't be an issue in build. " +
                    "We recommend however to enable RemoveUnloadedScenesDuringPlay editor preference in " +
                    "Window/Hyper Unity Commons/Editor Prefs Window to avoid this situation.",
                    debugSceneReferenceName, sceneReference.Path);
                return;
            }

            // Unload additive scene and wait
            AsyncOperation asyncUnloading = SceneManager.UnloadSceneAsync(sceneReference.BuildIndex);
            await AwaitOperationIsDone(isDonePollingPeriodSeconds, asyncUnloading);
        }

        #endif

        public static async Task AwaitOperationIsDone(double isDonePollingPeriodSeconds, AsyncOperation asyncOperation)
        {
            while (true)
            {
                // No need to check progress every frame, so just check every polling period
                await Task.Delay(TimeSpan.FromSeconds(isDonePollingPeriodSeconds));

                if (asyncOperation.isDone)
                {
                    break;
                }
            }

            // Nothing more to do, just give hand back to awaiter
        }
    }
}
