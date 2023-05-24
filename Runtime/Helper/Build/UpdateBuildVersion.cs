using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HyperUnityCommons
{
	/// Put this script on any game object with a Text or TextMeshProUGUI reflecting the current build version.
	/// This is convenient to always show the correct build version on the title screen.
	/// The Start method will ensure that the correct version is displayed when playing,
	/// even if you forgot to click the "Update build version in text" button in the custom inspector.
	public class UpdateBuildVersion : MonoBehaviour
	{
		private Text m_TextWidget;
		private TextMeshProUGUI m_TMPWidget;

		void Awake () {
			m_TextWidget = GetComponent<Text>();
			m_TMPWidget = GetComponent<TextMeshProUGUI>();
		}

		void Start () {
			UpdateText();
		}

		private void UpdateText () {
			string version = BuildData.GetVersionStringFromResource();

			if (m_TextWidget)
			{
				m_TextWidget.text = version;
			}

			if (m_TMPWidget)
			{
				m_TMPWidget.text = version;
			}
		}
	}
}

