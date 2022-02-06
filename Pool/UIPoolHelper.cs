using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsPattern
{
    public static class UIPoolHelper
    {
        /// Helper method to lazily instantiate and activate a minimum number targetCount of widget prefabs under parent.
        /// It uses the Pooling pattern with dynamic instantiation, as it creates new instances when needed,
        /// but never destroys the ones we don't need, only deactivates them in case we need them later.
        /// However, unlike Pool and Pool Manager scripts, it is meant for UI, which has a tendency to show a given
        /// number N of widgets on a Horizontal/Vertical Layout or a Grid, always in an ordered manner.
        /// This is very different from pooling objects in an unordered set because their position is determined by
        /// their transform, so their order in the Scene Hierarchy doesn't matter.
        /// Note that the caller still needs to initialise the widget's content itself after the call,
        /// as this function is unaware of the specific component types on widgetPrefab, nor their initialisation methods.
        ///
        /// Usage example: a Load menu that shows 20 save slots, where a save slot is represented by a prefab
        /// In the editor we placed 3 save slots to see what they look like, but didn't want to pre-place too many
        /// of them to avoid cluttering the scene, and because we cannot visualize them all due to the paging system
        /// anyway.
        ///
        /// Post-condition: parent must have at least targetCount children
        public static void LazyInstantiateWidgets(GameObject widgetPrefab, int targetCount, Transform parent)
        {
            // There may already be one or more widgets under parent to help us visualize the layout in the editor,
            // or because they were previously created last time we opened the sub-menu that required those widgets.
            // In this case, only add the extra widgets you need. In some cases, you may not need to add anything.
            // We assume all children of parent are instances of widgetPrefab, so we can just count them.
            // This means you shouldn't put other types of objects under parent!

            // The process is the following:
            // 1. (if there are not enough widgets) Instantiate any missing widget under parent,
            //    so there are at least targetCount of them
            // 2. (if there are too many widgets) Deactivate any extra widgets under parent that are above targetCount,
            //    if needed
            // (1 & 2 are exclusive, you never actually do things in both)
            // 3. Activate and initialise the first [targetCount] widgets

            int currentWidgetsCount = parent.childCount;

            // 1. Instantiate any missing instances to reach targetCount
            for (int i = currentWidgetsCount; i < targetCount; i++)
            {
                Object.Instantiate(widgetPrefab, parent);
            }

            // 2. Deactivate any extra widgets that we don't need now, in case they were active
            for (int i = targetCount; i < currentWidgetsCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(false);
            }

            // 3. Activate and initialise all required widgets
            for (int i = 0; i < targetCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(true);
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(parent.childCount >= targetCount, parent,
                "[UIPoolHelper] LazyInstantiateWidgets: childCount on parent {0} didn't reach targetCount {1}, " +
                "post-condition is not respected.", targetCount, parent);
            #endif
        }

        /// Get and activate available pooled widget without deactivating all the others
        /// This is akin to the PoolManager system, as a lightweight alternative when not using it
        /// Unlike PoolManager.GetFreeObject, it cannot instantiate new objects (prefab not passed), so make sure to
        /// call LazyInstantiateWidgets with enough count on Start/Setup
        public static GameObject GetPooledWidget(Transform parent)
        {
            int currentWidgetsCount = parent.childCount;

            for (int i = 0; i < currentWidgetsCount; i++)
            {
                GameObject pooledWidget = parent.GetChild(i).gameObject;
                if (!pooledWidget.activeSelf)
                {
                    pooledWidget.SetActive(true);
                    return pooledWidget;
                }
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarningFormat("[UIPoolHelper] GetPooledWidget: pool is starving at size {1}. " +
                "Consider increasing initial pool size used with LazyInstantiateWidgets to avoid this situation.",
                currentWidgetsCount);
            #endif

            return null;
        }
    }
}