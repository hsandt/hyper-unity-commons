using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Text))]
public class UpdateBuildVersion : MonoBehaviour {

	Text text;

	void Awake () {
		text = this.GetComponentOrFail<Text>();
	}

	void Start () {
		UpdateText();
	}

	void UpdateText () {
		string version = GetVersion();
		text.text = version;
	}

	string GetVersion() {
		// MICRO-OPTIMIZE: cache build data asset reference
		BuildData buildData = ResourcesUtil.LoadOrFail<BuildData>("Build/BuildData");
		return string.Format("v{0}.{1}.{2}", buildData.majorVersion, buildData.minorVersion, buildData.stageVersion);
	}

#if UNITY_EDITOR
	/// <summary>
	/// Update the build version in the Text component on this object, and also in the Player settings.
	/// If no text is found, it will still update the Palyer settings.
	/// </summary>
	public void UpdateBuildVersionTextAndSettings () {
		string version = GetVersion();

		// MICRO-OPTIMIZE: cache text and its properties
		text = this.GetComponent<Text>();

		if (text != null) {
			// Setting the text property of the Text component should set the vertices dirty
			// but for some reason, the Text graphic does not get updated in the editor, even when using Canvas.ForceUpdateCanvases(), SceneView.RepaintAll()
			// or text.SendMessage("OnValidate")
			// Therefore, simulate typing text live in the text field by accessing the property directly to ensure the text is updated in the Scene view.
			SerializedObject textSerializedObject = new SerializedObject(text);
			SerializedProperty textProperty = textSerializedObject.FindProperty("m_Text");

			text.text = version;

			textProperty.stringValue = version;
			textSerializedObject.ApplyModifiedProperties();
		}

		PlayerSettings.bundleVersion = version;
	}
#endif

}
