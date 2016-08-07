using UnityEngine;
using System.Collections;

/// This component allows playing particle effects based on bursts by reseting them before the particles die
/// Attach it to the ParticleSystem object and use a reference to this component to play the particle
[RequireComponent(typeof(ParticleSystem))]
public class ParticlePlayer : MonoBehaviour {

	/// Particle System to play
	ParticleSystem m_ParticleSystem;
	
	/// Timer before stopping particle (to prevent particle exhaustion and reuse them later)
	Timer m_Timer;

	[Tooltip("Duration wanted for the particle effects. Always set it to a lower value than the particles' lifetime. Fade particles out before that time.")]
	[SerializeField] float m_Duration = 0.5f;

	void Awake () {
		m_ParticleSystem = GetComponent<ParticleSystem>();
		m_Timer = new Timer(Stop);
	}

	void Start () {
		Setup();
	}

	public void Setup () {
		Stop();
	}

	public void Clear () {
		m_Timer.Stop();
	}

	public void Restart () {
		Clear();
		Setup();
	}

	void FixedUpdate () {
		// FixedUpdate just in case there is some lag and particles are not reset early enough
		// (most probably Update would work as well)
		m_Timer.CountDown(Time.deltaTime);
	}

	/// Play particleSystem and stop after particleEffectDuration
	public void Play () {
		m_ParticleSystem.gameObject.SetActive(true);
		m_ParticleSystem.Play();
		m_Timer.SetTime(m_Duration);
	}

	public void Stop () {
		gameObject.SetActive(false);
	}

}
