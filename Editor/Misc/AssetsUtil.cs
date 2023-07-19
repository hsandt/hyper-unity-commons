﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HyperUnityCommons.Editor
{
    public static class AssetsUtil
    {
        /// Return list of assets of type T
        /// If searchInFolders is not null nor empty, search in passed folder paths
        /// https://answers.unity.com/questions/486545/getting-all-assets-of-the-specified-type.html
        /// adapted to support searchInFolders
        public static List<T> FindAssetsByType<T>(string[] searchInFolders = null) where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            // FindAssets itself understands null and empty array of folders, so pass it directly
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}", searchInFolders);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
                else
                {
                    Debug.LogWarningFormat("[AssetsUtil] FindAssetsByType: no asset found for GUID guids[{0}] = {1} " +
                        "when searching folders: {2} for assets of type {3}",
                        i, guids[i],
                        searchInFolders != null && searchInFolders.Length > 0
                            ? string.Join(",", searchInFolders.Select(path => $"'{path}'"))
                            : "(everywhere)",
                        typeof(T));
                }
            }

            return assets;
        }

        /// Create an asset from model if it doesn't exist at path yet OR has the wrong type
        /// Else, replace asset at path, modifying properties in-place (preserves GUID)
        public static void CreateOrReplace<T>(UnityEngine.Object model, string path) where T : UnityEngine.Object
        {
            T output = AssetDatabase.LoadAssetAtPath<T>(path);
            if (output != null)
            {
                // edit asset in place to keep GUID, so that references in editor are preserved
                EditorUtility.CopySerialized(model, output);
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.CreateAsset(model, path);
            }
        }

        /// Create new prefab from model, or replace if already exists at path (preserves GUID)
        [Obsolete("Use PrefabUtility.SaveAsPrefabAsset (if options is ReplacePrefabOptions.Default) or " +
                  "PrefabUtility.SaveAsPrefabAssetAndConnect (if options is ReplacePrefabOptions.ConnectToPrefab) instead")]
        public static void CreateOrReplacePrefab(GameObject model, string path,
            ReplacePrefabOptions options = ReplacePrefabOptions.Default)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                PrefabUtility.ReplacePrefab(model, prefab, options);
            }
            else
            {
                PrefabUtility.CreatePrefab(path, model, options);
            }
        }

        /// Return the directory path of the current asset selection, or null if no asset was selected.
        /// Note that it doesn't find the last selected asset from the Project view if another view is currently active
        /// (unlike native Unity asset creation menu items).
        public static string GetContextDirectoryPath()
        {
            string selectedAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(selectedAssetPath))
            {
                Debug.LogWarning("Current selection is not an asset from the Project view, cannot create ");
                return null;
            }

            if (AssetDatabase.IsValidFolder(selectedAssetPath))
            {
                // the selected asset is a folder (or we are not selecting anything inside a folder), return its path
                return selectedAssetPath;
            }
            else
            {
                // the selected asset is a file, get the parent directory
                string selectedAssetDirectoryPath = System.IO.Path.GetDirectoryName(selectedAssetPath);
                return selectedAssetDirectoryPath;
            }
        }

        [MenuItem("Assets/Label Data Assets")]
        static void LabelDataAssets()
        {
            // Search the Assets/Data folder for all assets and label them with "Data", preserving existing labels
            if (AssetDatabase.IsValidFolder("Assets/Data"))
            {
                foreach (var guid in AssetDatabase.FindAssets("", new[] {"Assets/Data"}))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadMainAssetAtPath(path);

                    // Exclude folders
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        // Add label "Data" if not already assigned
                        string[] existingLabels = AssetDatabase.GetLabels(asset);
                        if (!existingLabels.Contains("Data"))
                        {
                            string[] newLabels = existingLabels.Append("Data").ToArray();
                            AssetDatabase.SetLabels(asset, newLabels);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[AssetsUtil] LabelDataAssets: no folder Assets/Data found, nothing will be labeled");
            }
        }
    }
}
