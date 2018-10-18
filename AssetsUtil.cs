using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Commons.Editor
{

	public static class AssetsUtil {

		/// Create an asset or replace it if it already exists at path (preserves GUID)
		public static void CreateOrReplace<T>(UnityEngine.Object model, string path) where T : UnityEngine.Object {
			T output = AssetDatabase.LoadAssetAtPath<T>(path);
			if (output != null) {
				// edit asset in place to keep GUID, so that references in editor are preserved
				EditorUtility.CopySerialized(model, output);
				AssetDatabase.SaveAssets();
			}
			else {
				AssetDatabase.CreateAsset(model, path);
			}
		}

		/// Create new prefab or replace if already exists at path (preserves GUID)
		public static void CreateOrReplacePrefab(GameObject model, string path, ReplacePrefabOptions options = ReplacePrefabOptions.Default) {
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab != null) {
				PrefabUtility.ReplacePrefab(model, prefab, options);
			} else {
				PrefabUtility.CreatePrefab(path, model, options);
			}
		}

	    /// Return the directory path of the current asset selection, or null if no asset was selected.
	    /// Note that it doesn't find the last selected asset from the Project view if another view is currently active
	    /// (unlike native Unity asset creation menu items).
	    public static string GetContextDirectoryPath() {
	        string selectedAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
	        if (string.IsNullOrEmpty(selectedAssetPath)) {
	            Debug.LogWarning("Current selection is not an asset from the Project view, cannot create ");
	            return null;
	        }
            
	        if (AssetDatabase.IsValidFolder(selectedAssetPath)) {
	            // the selected asset is a folder (or we are not selecting anything inside a folder), return its path
	            return selectedAssetPath;
	        }
	        else {
	            // the selected asset is a file, get the parent directory
	            string selectedAssetDirectoryPath = System.IO.Path.GetDirectoryName(selectedAssetPath);
	            return selectedAssetDirectoryPath;
	        }
	    }

	}

}
