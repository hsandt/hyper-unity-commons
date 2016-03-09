using UnityEngine;
using System.Collections;

/// <summary>
/// This script allows playing particle effects that work only once,
/// but reseting them before particles start to die
/// </summary>
public class PlayParticleOnce : MonoBehaviour {

	[SerializeField] new ParticleSystem particleSystem;
	[Tooltip("Duration wanted for the particle effects. Always set it to a lower value than the particles' lifetime. Fade particles out before that time.")]
	[SerializeField] float m_ParticleEffectDuration = 1f;

	Timer m_ParticleEffectTimer;

	// Use this for initialization
	void Awake () {
		m_ParticleEffectTimer = new Timer(StopParticle);
	}

	// Update is called once per frame
	void Start () {
		Setup();
	}

	public void Setup () {
		particleSystem.gameObject.SetActive(false);
	}

	public void Clear () {
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

	public void PlayParticle () {
		// Debug.Log("Play particle");
		particleSystem.gameObject.SetActive(true);
		particleSystem.Play();  // required if PFX is not Play on Awake
		m_ParticleEffectTimer.SetTime(m_ParticleEffectDuration);
	}

	public void StopParticle () {
		// Debug.Log("Stop particle");
		//    	damageParticles.Stop();
		particleSystem.gameObject.SetActive(false);
	}

}
