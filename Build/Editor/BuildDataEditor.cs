using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Commons.Helper
{

	[CustomEditor(typeof(BuildData))]
	public class BuildDataEditor : Editor {

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			BuildData data = (BuildData) target;
			if (GUILayout.Button("Update version in Player settings"))
			{
				string version = string.Format("v{0}.{1}.{2}", data.majorVersion, data.minorVersion, data.stageVersion);
				PlayerSettings.bundleVersion = version;
			}
		}

	}

}
