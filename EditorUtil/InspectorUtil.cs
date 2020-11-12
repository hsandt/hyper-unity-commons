#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace CommonsHelper
{

    /// Utility functions for handles. This script is exceptionally outside an Editor folder and assembly (but still
    /// inside #if UNITY_EDITOR) because non-editor classes may want to specialize their Inspector drawing in their own
    /// body. However, all drawing-related methods must be inside #if UNITY_EDITOR.
	public static class InspectorUtil {
	
		/// Modify text content on a Text component with live update in the editor
		/// Doing the usual Undo.RecordObject(text) + text.text = textContent is not enough,
		/// nor adding Canvas.ForceUpdateCanvases(), SceneView.RepaintAll() or text.SendMessage("OnValidate"),
		/// so if you write a custom editor that modifies Text and you need live preview without having to move
		/// text object in the scene to force refresh, call this function instead (this also handles Undo).
		public static void ChangeText (Text text, string textContent) {
			// In case we were editing the text field to change, we need to deselect it now to see the changes live in the inspector
			// (the property itself will be be changed indeed).
			// As a side effect, this will also deselect any other text field currently selected.
			EditorGUIUtility.editingTextField = false;

			// Setting the text property of a Text component in the editor should set the vertices dirty,
			// but for some reason, the Text graphic does not get updated in the editor, even when using refresh methods
			// mentioned in the doc comments.
			// Therefore, simulate typing text live in the text field by accessing the property directly to ensure
			// the text is updated in the Scene view.
			SerializedObject textSerializedObject = new SerializedObject(text);
			SerializedProperty textProperty = textSerializedObject.FindProperty("m_Text");
			textProperty.stringValue = textContent;
			textSerializedObject.ApplyModifiedProperties();
		}
	    
		/// Modify text content on a TextMeshProUGUI component with live update in the editor
		/// Same as ChangeText, but for TextMeshPro
		public static void ChangeTMPText (TextMeshProUGUI text, string textContent) {
			EditorGUIUtility.editingTextField = false;

			SerializedObject textSerializedObject = new SerializedObject(text);
			
			// note casing, the text field is actually on base class TMP_Text and named "m_text" lowercase
			SerializedProperty textProperty = textSerializedObject.FindProperty("m_text");
			textProperty.stringValue = textContent;
			textSerializedObject.ApplyModifiedProperties();
		}
	}

}

#endif  // UNITY_EDITOR
