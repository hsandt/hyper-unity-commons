using System;
using UnityEditor;
using UnityEngine;

/// Code tuning editor window
public class CodeTuningEditorWindow : EditorWindow
{

	// singleton instance
	static CodeTuningEditorWindow window;

	/// Should the scene be repainted when athe user changes a value?
	bool repaintScene = false;

	[MenuItem("Debug/Code Tuning")]
	static void Init()
	{
		if (window == null)
			window = (CodeTuningEditorWindow) GetWindow(typeof(CodeTuningEditorWindow), false, "Code Tuning");

		window.Show();
	}

	void OnEnable () {
		Load ();  // important to reload values on Play, since we are not using a ScriptableObject asset nor a MonoBehaviour to hold CodeTuning data, so they get reseted on Play
	}

	void OnGUI()
	{
		// use CodeTuning.Instance instance of keeping reference of CodeTuning to be sure it has been defined (reference would be reset on Play)
		EditorGUI.BeginChangeCheck ();

		EditorGUI.BeginChangeCheck ();
		CodeTuning.Instance.branchIndex = EditorGUILayout.IntField("Branch Index", CodeTuning.Instance.branchIndex);
		CodeTuning.Instance.float1 = EditorGUILayout.FloatField ("Float 1", CodeTuning.Instance.float1);
		CodeTuning.Instance.float2 = EditorGUILayout.FloatField ("Float 2", CodeTuning.Instance.float2);
		repaintScene = EditorGUILayout.Toggle (new GUIContent ("Repaint Scene on Change", "Check if the values are used in OnDrawGizmos / OneSceneGUI"), repaintScene);
		if (EditorGUI.EndChangeCheck () && repaintScene) {
			SceneView.RepaintAll ();
		}

		if (EditorGUI.EndChangeCheck ())
			Save();
	}

	void Load () {
		if (EditorPrefs.HasKey("CodeTuning.branchIndex"))
			CodeTuning.Instance.branchIndex = EditorPrefs.GetInt("CodeTuning.branchIndex");

		if (EditorPrefs.HasKey("CodeTuning.float1"))
			CodeTuning.Instance.float1 = EditorPrefs.GetFloat("CodeTuning.float1");
	}

	void Save() {
		EditorPrefs.SetInt("CodeTuning.branchIndex", CodeTuning.Instance.branchIndex);
		EditorPrefs.SetFloat("CodeTuning.float1", CodeTuning.Instance.float1);
	}

}