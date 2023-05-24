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

		void Awake ()
		{
			if (Application.isEditor && m_DeactivateInEditor ||
				!Application.isEditor &&
					(Debug.isDebugBuild && m_DeactivateInPlayerDevelopment ||
					!Debug.isDebugBuild && m_DeactivateInPlayerRelease))
			{
				gameObject.SetActive(false);
			}
		}
	}
}
