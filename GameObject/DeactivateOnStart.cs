using UnityEngine;
using System.Collections;

namespace CommonsHelper
{

	public class DeactivateOnStart : MonoBehaviour
	{

		[SerializeField, Tooltip("Deactivate in editor?")]
		private bool m_DeactivateInEditor = true;
		
		[SerializeField, Tooltip("Deactivate in player?")]
		private bool m_DeactivateInPlayer = true;
		
		void Awake () {
			if (Application.isEditor && m_DeactivateInEditor || !Application.isEditor && m_DeactivateInPlayer)
			{
				gameObject.SetActive(false);
			}
		}

	}

}
