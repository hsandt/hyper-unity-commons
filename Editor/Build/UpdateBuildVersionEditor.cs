using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace CommonsHelper.Editor
{

	[CustomEditor(typeof(UpdateBuildVersion))]
	public class UpdateBuildVersionEditor : UnityEditor.Editor {

		UpdateBuildVersion script;

		void OnEnable () {
			script = (UpdateBuildVersion) target;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if (!Application.isPlaying) {
				if (GUILayout.Button("Update build version in text"))
				{
	                UpdateBuildVersionText();
				}
			}
		}

		/// <summary>
		/// Update the build version in the Text component on this object
		/// </summary>
		public void UpdateBuildVersionText () {
			Text text = script.GetComponent<Text>();
			if (text != null) {
				string version = BuildData.GetVersionStringFromResource();
				InspectorUtil.ChangeText(text, version);
			}
		}

	}

}

