using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Text))]
public class UpdateBuildVersionText : MonoBehaviour {

	Text text;

	void Awake () {
		text = this.GetComponentOrFail<Text>();
	}

	void Start () {
		UpdateText();
	}

	void UpdateText () {
		text.text = GetVersion();
	}

	string GetVersion() {
		// MICRO-OPTIMIZE: cache build data asset
		BuildData buildData = ResourcesUtil.LoadOrFail<BuildData>("Build/BuildData");
		return string.Format("v{0}.{1}.{2}", buildData.majorVersion, buildData.minorVersion, buildData.stageVersion);
	}

#if UNITY_EDITOR
	public void UpdateTextInEditor () {
		// MICRO-OPTIMIZE: cache text
		text = this.GetComponentOrFail<Text>();

		// Setting the text property of the Text component should set the vertices dirty
		// but for some reason, the Text graphic does not get updated in the editor, even when using Canvas.ForceUpdateCanvases(), SceneView.RepaintAll()
		// or text.SendMessage("OnValidate")
		// Therefore, simulate typing text live in the text field by accessing the property directly to ensure the text is updated in the Scene view.
		SerializedObject textSerializedObject = new SerializedObject(text);
		SerializedProperty textProperty = textSerializedObject.FindProperty("m_Text");
		textProperty.stringValue = GetVersion();
		textSerializedObject.ApplyModifiedProperties();
	}
#endif

}
