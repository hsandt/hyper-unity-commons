using System;
using UnityEditor;
using UnityEngine;

/// Code tuning editor window
public class CodeTuningEditorWindow : EditorWindow
{

	/// Singleton instance
	static CodeTuningEditorWindow window;

	/// Should the scene be repainted when the user changes a value?
	bool repaintScene = false;

	[MenuItem("Debug/Code Tuning")]
	static void Init()
	{
		if (window == null)
			window = GetWindow<CodeTuningEditorWindow>(false, "Code Tuning");

		window.Show();
	}

	void OnEnable () {
		Debug.Log ("OnEnable CodeTuning Editor Window");
		Load ();  // IMPORTANT to reload values on Play, since we are not using a ScriptableObject asset nor a MonoBehaviour to hold CodeTuning data, so they get reseted on Play
	}

	void OnGUI()
	{
		// use CodeTuning.Instance instance of keeping reference of CodeTuning to be sure it has been defined (reference would be reset on Play)
		EditorGUI.BeginChangeCheck ();
		// more simple: a drawer showing code tuning instance
		CodeTuning.Instance.active = EditorGUILayout.Toggle("Activate", CodeTuning.Instance.active);
		CodeTuning.Instance.branchIndex = EditorGUILayout.IntField("Branch Index", CodeTuning.Instance.branchIndex);
		CodeTuning.Instance.float1 = EditorGUILayout.FloatField ("Float 1", CodeTuning.Instance.float1);
		CodeTuning.Instance.float2 = EditorGUILayout.FloatField ("Float 2", CodeTuning.Instance.float2);
		repaintScene = EditorGUILayout.Toggle (new GUIContent ("Repaint Scene on Change", "Check if the values are used in OnDrawGizmos / OneSceneGUI"), repaintScene);
		if (EditorGUI.EndChangeCheck ()) {
			Save();
			if (repaintScene)
				SceneView.RepaintAll ();
		}
	}

	void Load () {
		if (EditorPrefs.HasKey("CodeTuning.active"))
			CodeTuning.Instance.active = EditorPrefs.GetBool("CodeTuning.active");

		if (EditorPrefs.HasKey("CodeTuning.branchIndex"))
			CodeTuning.Instance.branchIndex = EditorPrefs.GetInt("CodeTuning.branchIndex");

		if (EditorPrefs.HasKey("CodeTuning.float1"))
			CodeTuning.Instance.float1 = EditorPrefs.GetFloat("CodeTuning.float1");

		if (EditorPrefs.HasKey("CodeTuning.float2"))
			CodeTuning.Instance.float2 = EditorPrefs.GetFloat("CodeTuning.float2");

		if (EditorPrefs.HasKey("CodeTuning.repaintScene"))
			repaintScene = EditorPrefs.GetBool("CodeTuning.repaintScene");
	}

	void Save() {
		EditorPrefs.SetBool("CodeTuning.active", CodeTuning.Instance.active);
		EditorPrefs.SetInt("CodeTuning.branchIndex", CodeTuning.Instance.branchIndex);
		EditorPrefs.SetFloat("CodeTuning.float1", CodeTuning.Instance.float1);
		EditorPrefs.SetFloat("CodeTuning.float2", CodeTuning.Instance.float2);
		EditorPrefs.SetBool("CodeTuning.repaintScene", repaintScene);
	}

}
