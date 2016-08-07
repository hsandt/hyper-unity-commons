using UnityEngine;
using System.Collections;

/// DEPRECATED: use ParticlePlayer on *the object with the ParticleSystem* instead
/// This component allows playing particle effects based on bursts by reseting them before the particles die
public class PlayParticleOnce : MonoBehaviour {

	Timer m_ParticleEffectTimer;
	private ParticleSystem currentParticleSystem;

	// Use this for initialization
	void Awake () {
		m_ParticleEffectTimer = new Timer(StopParticle);
	}

	// Update is called once per frame
	void Start () {
		Setup();
	}

	public void Setup () {
	}

	public void Clear () {
		StopParticle();
		m_ParticleEffectTimer.Stop();
	}

	public void Restart () {
		Clear();
		Setup();
	}

	void FixedUpdate () {
		// fixed update just in case there is some lag and particles are not reset early enough
		// (most probably Update would work as well)
		m_ParticleEffectTimer.CountDown(Time.deltaTime);
	}

	/// Play particleSystem and stop after particleEffectDuration
	/// particleEffectDuration: Duration wanted for the particle effects. Always set it to a lower value than the particles' lifetime. Fade particles out before that time.
	public void PlayParticle (ParticleSystem particleSystem, float particleEffectDuration) {
		// Debug.Log("Play particle");
		currentParticleSystem = particleSystem;
		currentParticleSystem.gameObject.SetActive(true);
		currentParticleSystem.Play();  // required if PFX is not Play on Awake
		m_ParticleEffectTimer.SetTime(particleEffectDuration);
	}

	public void StopParticle () {
		// Debug.Log("Stop particle");
		// if any current particle playing, stop it now
		if (currentParticleSystem) {
			currentParticleSystem.gameObject.SetActive(false);
			currentParticleSystem = null;
		}
	}

}
