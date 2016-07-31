using System;
using UnityEditor;
using UnityEngine;

/// Code tuning editor window
public class CodeTuningEditorWindow : EditorWindow
{

	// singleton instance
	static CodeTuningEditorWindow window;

	[MenuItem("Debug/Code Tuning")]
	static void Init()
	{
		if (window == null)
			window = (CodeTuningEditorWindow) GetWindow(typeof(CodeTuningEditorWindow), false, "Code Tuning");

		if (EditorPrefs.HasKey("CodeTuning.branchIndex"))
			CodeTuning.Instance.branchIndex = EditorPrefs.GetInt("CodeTuning.branchIndex");

		if (EditorPrefs.HasKey("CodeTuning.float1"))
			CodeTuning.Instance.float1 = EditorPrefs.GetFloat("CodeTuning.float1");

		window.Show();
	}

	void OnGUI()
	{
		// use CodeTuning.Instance instance of keeping reference of CodeTuning to be sure it has been defined (reference would be reset on Play)
		CodeTuning.Instance.branchIndex = EditorGUILayout.IntField("Branch Index", CodeTuning.Instance.branchIndex);
		CodeTuning.Instance.float1 = EditorGUILayout.FloatField ("Float 1", CodeTuning.Instance.float1);
		if (GUILayout.Button("Save")) Save();
	}

	void Save() {
		EditorPrefs.SetInt("CodeTuning.branchIndex", CodeTuning.Instance.branchIndex);
		EditorPrefs.SetFloat("CodeTuning.float1", CodeTuning.Instance.float1);
	}

}
