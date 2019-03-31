using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CommonsHelper.Editor
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
		}

	}

}
