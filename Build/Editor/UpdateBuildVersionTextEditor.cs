using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UpdateBuildVersionText))]
public class UpdateBuildVersionTextEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		UpdateBuildVersionText script = (UpdateBuildVersionText) target;
		if (!Application.isPlaying) {
			if (GUILayout.Button("Update version text"))
			{
				// In case we were editing the text field to change, we need to deselect it now to see the changes live in the inspector
				// (the property itself will be be changed indeed).
				// This will also deselect any other text field currently selected.
				EditorGUIUtility.editingTextField = false;
				script.UpdateTextInEditor();
			}
		}
	}

}
