using System.Collections;
using System.Collections.Generic;
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
		}

	}

}
