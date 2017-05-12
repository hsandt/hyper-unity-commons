using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UpdateBuildVersion))]
public class UpdateBuildVersionEditor : Editor {

	UpdateBuildVersion script;

	void OnEnable () {
		script = (UpdateBuildVersion) target;
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		if (!Application.isPlaying) {
			if (GUILayout.Button("Update build version in text and Player settings"))
			{
				UpdateBuildVersionTextAndSettings();
			}
		}
	}

	/// <summary>
	/// Update the build version in the Text component on this object, and also in the Player settings.
	/// If no text is found, it will still update the Palyer settings.
	/// </summary>
	public void UpdateBuildVersionTextAndSettings () {
		string version = BuildData.GetVersion();

		Text text = script.GetComponent<Text>();
		if (text != null) {
			InspectorUtil.ChangeText(text, version);
		}

		PlayerSettings.bundleVersion = version;
	}

}
