using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace HyperUnityCommons.Editor
{

	[CustomEditor(typeof(BuildData))]
	public class BuildDataEditor : UnityEditor.Editor {

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			BuildData data = (BuildData) target;
			if (GUILayout.Button("Update version in Player settings"))
			{
				string version = data.GetVersionString();
				PlayerSettings.bundleVersion = version;
			}

			if (GUILayout.Button("Update version for all UpdateBuildVersion scripts found in active scenes"))
			{
				var scripts = FindObjectsOfType<UpdateBuildVersion>();
				foreach (UpdateBuildVersion script in scripts)
				{
					UpdateBuildVersionEditor.UpdateBuildVersionTextSiblingOf(script);
				}
			}

			if (GUILayout.Button("Open Build folder"))
			{
				OpenBuildFolder();
			}
		}

		private void OpenBuildFolder()
		{
			// Application.dataPath ends with Assets/ so we need to go one directory up to get the project root
			string projectRootPath = Path.GetDirectoryName(Application.dataPath);
			string buildFolderFullPath = Path.Combine(projectRootPath, "Build");

			// Create directory if needed
			if (!Directory.Exists(buildFolderFullPath))
			{
				Directory.CreateDirectory(buildFolderFullPath);
			}

			// Open Screenshots folder
			// https://forum.unity.com/threads/editorutility-revealinfinder-inconsistency.383939/#post-8431145
			EditorUtility.OpenWithDefaultApp(buildFolderFullPath);
		}
	}

}
