using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace HyperUnityCommons
{
	public class DeactivateOnAwake : MonoBehaviour
	{
		[SerializeField, Tooltip("Deactivate in editor?")]
		private bool m_DeactivateInEditor = true;

		[SerializeField, Tooltip("Deactivate in player for Development build?")]
		[FormerlySerializedAs("m_DeactivateInPlayer")]
		private bool m_DeactivateInPlayerDevelopment = true;


		[SerializeField, Tooltip("Deactivate in player for Release build?")]
		[FormerlySerializedAs("m_DeactivateInPlayer")]
		private bool m_DeactivateInPlayerRelease = true;

		private void Awake ()
		{
			PlayMode playMode = PlayModeUtil.GetPlayMode();

			if (playMode == PlayMode.Editor && m_DeactivateInEditor ||
			    playMode == PlayMode.DevelopmentBuild && m_DeactivateInPlayerDevelopment ||
			    playMode == PlayMode.ReleaseBuild && m_DeactivateInPlayerRelease)
			{
				gameObject.SetActive(false);
			}
		}
	}
}
