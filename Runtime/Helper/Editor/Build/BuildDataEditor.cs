using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace HyperUnityCommons.Editor
{
	[CustomEditor(typeof(BuildData))]
	public class BuildDataEditor : UnityEditor.Editor
	{
		private const string BuildProfilesFolder = "Assets/Settings/Build Profiles";
		
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			BuildData data = (BuildData) target;
			if (GUILayout.Button("Update version in Player settings"))
			{
				string version = data.GetVersionString();
				SetNewVersion(version);
			}

			if (GUILayout.Button("Update version for all UpdateBuildVersion scripts found in active scenes"))
			{
				var scripts = FindObjectsByType<UpdateBuildVersion>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
		
		// Extracted from EirikWeave's SetNewVersion below to be reusable by other scripts
		public static string[] GetBuildProfileGUIDs()
		{
			var buildProfiles = AssetDatabase.FindAssets(
				"t:BuildProfile",
				new[] { BuildProfilesFolder });
			return buildProfiles;
		}
		
		// Method by EirikWeave posted on https://discussions.unity.com/t/new-build-profiles-is-an-embarrassment/937021/52
		// MODIFICATIONS by hsandt:
		// 1. if no `bundleVersion` line is found, continue to next build profile:
		//    this is normal for profiles that don't override Player Settings
		// 2. refresh assets after writing new lines to profile file, so we immediately see the changes
		//    if Build profile window is opened
		// 3. removed dialog box at the end. Instead, (a) debug log changing version in base Player Settings,
		//    and (b) improve debug log text for each build profile Player Settings Overrides
		private static void SetNewVersion(string inputText)
		{
			PlayerSettings.bundleVersion = inputText;
			// Save project:
			AssetDatabase.SaveAssets();
			// MODIFICATION 3a
			Debug.Log($"Updated base Player Settings with new version number {inputText}");

			// There is no API for changing build profile settings, without switching to each and every build profile
			// first, which requires recompilation in Unity. So we manipulate the text files instead:
			var buildProfiles = GetBuildProfileGUIDs();

			if (buildProfiles.Length == 0)
				throw new Exception($"No build profiles found in folder {BuildProfilesFolder}");

			foreach (var buildProfileGuid in buildProfiles)
			{
				var buildProfilePath = AssetDatabase.GUIDToAssetPath(buildProfileGuid);

				// Load as text, overwrite the line with version number, and save back to disk:
				var lines = File.ReadAllLines(buildProfilePath);

				var foundVersionLine = false;
				for (var i = 0; i < lines.Length; i++)
					if (lines[i].Contains("    - line: '|   bundleVersion: ", StringComparison.Ordinal))
					{
						lines[i] = $"    - line: '|   bundleVersion: {inputText}'";
						foundVersionLine = true;
						break;
					}

				if (!foundVersionLine)
					// MODIFICATION 1
					continue;

				// Save back to disk:
				File.WriteAllLines(buildProfilePath, lines);
				// MODIFICATION 2
				AssetDatabase.Refresh();
				// MODIFICATION 3b
				Debug.Log($"Updated build profile '{buildProfilePath}' Player Settings Overrides with new version number {inputText}");
			}
		}

		private void OpenBuildFolder()
		{
			// Application.dataPath ends with Assets/ so we need to go one directory up to get the project root
			string projectRootPath = Path.GetDirectoryName(Application.dataPath);
			if (projectRootPath == null)
			{
				return;
			}

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
