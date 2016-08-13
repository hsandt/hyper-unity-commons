using UnityEngine;
using System.Collections;

/// DEPRECATED: use direct method calls, or extensions if you need more features
/// Helper component: attach it to the ParticleSystem object and use a reference to this component to play the particle
[RequireComponent(typeof(ParticleSystem))]
public class ParticlePlayer : MonoBehaviour {

	/// Particle System to play
	ParticleSystem m_ParticleSystem;
	
	void Awake () {
		m_ParticleSystem = GetComponent<ParticleSystem>();
	}

	void Start () {
		Setup();
	}

	public void Setup () {
		Stop();
	}

	public void Clear () {

	}

	public void Restart () {
		Clear();
		Setup();
	}

	void OnEnable () {
		m_ParticleSystem.Play();
		// m_ParticleSystem.playbackSpeed = 1f;
	}

	void OnDisable () {
		m_ParticleSystem.Pause();
		// m_ParticleSystem.playbackSpeed = 0f;
	}

	/// Play particleSystem and stop after particleEffectDuration
	public void Play () {
		m_ParticleSystem.gameObject.SetActive(true);
		m_ParticleSystem.Play();
	}

	public void Stop () {
		m_ParticleSystem.Stop();
		// gameObject.SetActive(false);
	}

}
