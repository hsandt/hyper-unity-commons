using System;
using UnityEditor;
using UnityEngine;

namespace CommonsDebug.Editor
{

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
				window = (CodeTuningEditorWindow) GetWindow<CodeTuningEditorWindow>(false, "Code Tuning");

			window.Show();
		}

		void OnEnable () {
			Load ();  // IMPORTANT to reload values on Play, since we are not using a ScriptableObject asset nor a MonoBehaviour to hold CodeTuning data, so they get reseted on Play
		}

		void OnGUI()
		{
			// use CodeTuning.Instance instance of keeping reference of CodeTuning to be sure it has been defined (reference would be reset on Play)
			EditorGUI.BeginChangeCheck ();

			CodeTuning.Instance.active = EditorGUILayout.ToggleLeft("Activate", CodeTuning.Instance.active);

			EditorGUILayout.Space();

			CodeTuning.Instance.branchIndex = EditorGUILayout.IntSlider("Branch Index", CodeTuning.Instance.branchIndex, 0, 10);
			CodeTuning.Instance.bool1 = EditorGUILayout.Toggle("Bool 1", CodeTuning.Instance.bool1);
			CodeTuning.Instance.bool2 = EditorGUILayout.Toggle("Bool 2", CodeTuning.Instance.bool2);
			CodeTuning.Instance.float1 = EditorGUILayout.FloatField ("Float 1", CodeTuning.Instance.float1);
			CodeTuning.Instance.float2 = EditorGUILayout.FloatField ("Float 2", CodeTuning.Instance.float2);
			repaintScene = EditorGUILayout.Toggle(new GUIContent ("Repaint Scene on Change", "Check if the values are used in OnDrawGizmos / OneSceneGUI"), repaintScene);

			if (EditorGUI.EndChangeCheck()) {
				Save();
				if (repaintScene)
					SceneView.RepaintAll();
			}
		}

		void Load () {
			if (EditorPrefs.HasKey("CodeTuning.active")) {
				CodeTuning.Instance.active = EditorPrefs.GetBool ("CodeTuning.active");
			}

			if (EditorPrefs.HasKey("CodeTuning.branchIndex"))
				CodeTuning.Instance.branchIndex = EditorPrefs.GetInt("CodeTuning.branchIndex");

			if (EditorPrefs.HasKey("CodeTuning.bool1"))
				CodeTuning.Instance.active = EditorPrefs.GetBool("CodeTuning.bool1");

			if (EditorPrefs.HasKey("CodeTuning.bool2"))
				CodeTuning.Instance.active = EditorPrefs.GetBool("CodeTuning.bool2");

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
			EditorPrefs.SetBool("CodeTuning.bool1", CodeTuning.Instance.bool1);
			EditorPrefs.SetBool("CodeTuning.bool2", CodeTuning.Instance.bool2);
			EditorPrefs.SetFloat("CodeTuning.float1", CodeTuning.Instance.float1);
			EditorPrefs.SetFloat("CodeTuning.float2", CodeTuning.Instance.float2);
			EditorPrefs.SetBool("CodeTuning.repaintScene", repaintScene);
		}

	}

}
