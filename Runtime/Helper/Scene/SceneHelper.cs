using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if COM_EFLATUN_SCENEREFERENCE
using Eflatun.SceneReference;
#endif

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
            DebugUtil.AssertFormat(sceneReference.HasValue, context,
                "[SceneHelper] LoadSceneAsync: Scene reference '{0}' is not set, so it is not safe to use",
                debugSceneReferenceName);

            if (!sceneReference.IsSafeToUse)
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] LoadSceneAsync: Scene reference '{0}' is not safe to use, cannot load scene",
                    debugSceneReferenceName);
                return;
            }

            // Check if scene to load has already been added
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

            // Load scene with passed mode and wait
            AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(sceneReference.BuildIndex, loadSceneMode);
            await AwaitOperationIsDone(asyncLoading, isDonePollingPeriodSeconds);

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
        public static async Task UnloadSceneAsync(SceneReference sceneReference, double isDonePollingPeriodSeconds = 0.1f,
            Object context = null, string debugSceneReferenceName = null)
        {
            DebugUtil.AssertFormat(sceneReference.HasValue, context,
                "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' is not set, so it is not safe to use",
                debugSceneReferenceName);

            if (!sceneReference.IsSafeToUse)
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' is not safe to use, cannot load scene",
                    debugSceneReferenceName);
                return;
            }

            // Check if scene to unload has already been added
            if (!sceneReference.LoadedScene.IsValid())
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' at '{1}' has not been added, " +
                    "cannot unload scene",
                    debugSceneReferenceName, sceneReference.Path);
                return;
            }

            // Check if scene to unload is actually loaded
            if (!sceneReference.LoadedScene.isLoaded)
            {
                DebugUtil.LogErrorFormat(context,
                    "[SceneHelper] UnloadSceneAsync: Scene reference '{0}' at '{1}' has been added, but not loaded, " +
                    "cannot unload scene. If this happened when testing additive scenes directly in the editor " +
                    "(so some scenes have already been added), then it won't be an issue in build. " +
                    "We recommend however to enable RemoveUnloadedScenesDuringPlay editor preference in " +
                    "Window/Hyper Unity Commons/Editor Prefs Window to avoid this situation.",
                    debugSceneReferenceName, sceneReference.Path);
                return;
            }

            // Unload additive scene and wait
            AsyncOperation asyncUnloading = SceneManager.UnloadSceneAsync(sceneReference.BuildIndex);
            await AwaitOperationIsDone(asyncUnloading, isDonePollingPeriodSeconds);
        }

        /// <summary>
        /// Transition from previous scene to next scene using transition scene
        /// The transition scene is mandatory even if you don't have anything special to show for the transition
        /// (for instance if you show a transition overlay in caller code side for more control), just to avoid
        /// unloading the last scene in case there is no scene loaded besides the previous scene.
        /// In this case, just prepare an empty scene that you will use as transition scene.
        /// Note that this will work even when called from an object that will be destroyed with previous scene unloading,
        /// because Tasks are running on their own
        /// </summary>
        /// <param name="previousSceneReferences">List of scenes currently loaded that will be unloaded in order during the transition. Must be loaded when calling this method.</param>
        /// <param name="nextSceneReference">Next scene to load. Must not be loaded when calling this method.</param>
        /// <param name="transitionSceneReference">Scene that acts as transition between the two other scenes (screen overlay, loading screen...). Must not be loaded when calling this method.</param>
        /// <param name="isDonePollingPeriodSeconds">Period (seconds) used to poll whether each loading/unloading is finished (also used for asset unloading if unloadUnusedAssets is true)</param>
        /// <param name="unloadUnusedAssets">If true, unload unused assets after unloading previous scene. Note that the transition scene, if used, will still be loaded at this point, so its assets won't be unloaded.</param>
        /// <param name="context">Optional context for debugging</param>
        /// <param name="debugPreviousSceneReferenceName">Optional scene name or full symbol with namespace used to access previous scene reference for debugging</param>
        /// <param name="debugNextSceneReferenceName">Optional scene name or full symbol with namespace used to access next scene reference for debugging</param>
        /// <param name="debugTransitionSceneReferenceName">Optional scene name or full symbol with namespace used to access transition scene reference for debugging</param>
        public static async Task TransitionFromToScene(List<SceneReference> previousSceneReferences, SceneReference
        nextSceneReference, SceneReference transitionSceneReference,
            double isDonePollingPeriodSeconds = 0.1f, bool unloadUnusedAssets = false, Object context = null,
            string debugPreviousSceneReferenceName = null, string debugNextSceneReferenceName = null,
            string debugTransitionSceneReferenceName = null)
        {
            // Load transition scene additively for screen transition
            await LoadSceneAsync(transitionSceneReference, LoadSceneMode.Additive, false, isDonePollingPeriodSeconds,
                context, debugTransitionSceneReferenceName);

            // Unload previous scenes one by one (if not performant enough, consider unloading them all in parallel)
            foreach (SceneReference previousSceneReference in previousSceneReferences)
            {
                await UnloadSceneAsync(previousSceneReference, isDonePollingPeriodSeconds,
                    context, debugPreviousSceneReferenceName);
            }

            if (unloadUnusedAssets)
            {
                await AwaitOperationIsDone(Resources.UnloadUnusedAssets(), isDonePollingPeriodSeconds);
            }

            // Load next scene additively as active scene, so we preserve the transition scene loaded above but still
            // make it the new main scene
            // It should contain its own manager script, which will load any required additive scenes and start running
            // that game state.
            await LoadSceneAsync(nextSceneReference, LoadSceneMode.Additive, true, isDonePollingPeriodSeconds,
                context, debugNextSceneReferenceName);

            // Unload transition scene
            await UnloadSceneAsync(transitionSceneReference, isDonePollingPeriodSeconds,
                context, debugTransitionSceneReferenceName);
        }

        /// Overload of TransitionFromToScene overload that takes a single previous scene reference
        public static async Task TransitionFromToScene(SceneReference previousSceneReference, SceneReference
                nextSceneReference, SceneReference transitionSceneReference,
            double isDonePollingPeriodSeconds = 0.1f, bool unloadUnusedAssets = false, Object context = null,
            string debugPreviousSceneReferenceName = null, string debugNextSceneReferenceName = null,
            string debugTransitionSceneReferenceName = null)
        {
            await TransitionFromToScene(new List<SceneReference> { previousSceneReference },
                nextSceneReference, transitionSceneReference,
                isDonePollingPeriodSeconds, unloadUnusedAssets, context,
                debugPreviousSceneReferenceName, debugNextSceneReferenceName, debugTransitionSceneReferenceName);
        }

        #endif

        public static async Task AwaitOperationIsDone(AsyncOperation asyncOperation,
            double isDonePollingPeriodSeconds = 0.1f)
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
