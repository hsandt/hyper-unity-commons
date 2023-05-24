using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using TMPro;

namespace HyperUnityCommons.Editor
{
	[CustomEditor(typeof(UpdateBuildVersion))]
	public class UpdateBuildVersionEditor : UnityEditor.Editor
	{
		private UpdateBuildVersion m_Script;

		void OnEnable ()
		{
			m_Script = (UpdateBuildVersion) target;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!Application.isPlaying)
			{
				if (GUILayout.Button("Update build version in text"))
				{
	                UpdateBuildVersionText();
				}
			}
		}

		/// Update the build version in the Text component on this object
		private void UpdateBuildVersionText ()
		{
			UpdateBuildVersionTextSiblingOf(m_Script);
		}

		/// Update the build version in the Text component on this object
		public static void UpdateBuildVersionTextSiblingOf (UpdateBuildVersion script)
		{
			string version = BuildData.GetVersionStringFromResource();

			Text text = script.GetComponent<Text>();
			if (text != null)
			{
				InspectorUtil.ChangeText(text, version);
			}

			TextMeshProUGUI tmpWidget = script.GetComponent<TextMeshProUGUI>();
			if (tmpWidget != null)
			{
				InspectorUtil.ChangeTMPText(tmpWidget, version);
			}
		}
	}
}

