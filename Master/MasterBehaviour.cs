using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MasterBehaviour : ClearableBehaviour, IPausable {

	// Behaviour is broader than MonoBehaviour and contains all native Unity components that have an Update event,
	// along with the OnEnable and OnDisable events.
	// In Awake(), make sure you register sibling behaviours you need to pause with slaveBehaviours.Add(myBehaviour);
	[Tooltip("Slave behaviours to pause and resume. Only set non-sibling components here, as sibling components should be added programmatically.")]
	public List<Behaviour> slaveBehaviours;

	// ParticleSystems are other types of Components with their own Play/Pause methods, so they are in another list
	[Tooltip("Particle systems to pause and resume")]
	public List<ParticleSystem> slaveParticles;

	public override void Setup () {
		// Enable all behaviours. Useful because we disable the behaviours in Clear(), and also
		// because when restarting a level from an in-game menu that paused the game, we need to "Resume" the scripts.
		for (int i = 0; i < slaveBehaviours.Count; ++i) {
			Behaviour slaveBehaviour = slaveBehaviours[i];
			if (slaveBehaviour != null) slaveBehaviour.enabled = true;

			// If the slave behaviour is also a ClearableBehaviour, Setup it now. This allows not to Setup everything manually in the derived class.
			ClearableBehaviour slaveClearableBehaviour = slaveBehaviour as ClearableBehaviour;
			if (slaveClearableBehaviour)
				slaveClearableBehaviour.Setup();
		}
	}

	public override void Clear() {
		for (int i = 0; i < slaveBehaviours.Count; ++i) {
			// Disable the behaviours to stop Updating in case the game object won't be deactivated and there will be some time before the next Setup
			Behaviour slaveBehaviour = slaveBehaviours[i];
			if (slaveBehaviour != null) slaveBehaviour.enabled = false;

			// If the slave behaviour is also a ClearableBehaviour, Clear it now. This allows not to Clear everything manually in the derived class.
			ClearableBehaviour slaveClearableBehaviour = slaveBehaviour as ClearableBehaviour;
			if (slaveClearableBehaviour)
				slaveClearableBehaviour.Clear();
		}

		for (int i = 0; i < slaveParticles.Count; ++i) {
			ParticleSystem slaveParticle = slaveParticles[i];
			if (slaveParticle != null) {
				// Whatever the current state of the particles, stop and clear them completely
				// This is only required if the game object is not deactivated before/after being Cleared,
				// so that particles do not remain in their current state and replay from there.
				// Caution: in this case, even if they should play on start, they won't on next Setup.
				if (slaveParticle.isPlaying || slaveParticle.isPaused) slaveParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				else if (slaveParticle.IsAlive()) slaveParticle.Clear();  // must have been stopped with Stop(ParticleSystemStopBehavior.StopEmitting)
			}
		}
	}

	/// Pause all slave behaviours
	public virtual void Pause ()
	{
		for (int i = 0; i < slaveBehaviours.Count; ++i) {
			Behaviour slaveBehaviour = slaveBehaviours[i];
			if (slaveBehaviour != null) slaveBehaviour.enabled = false;
		}
		for (int i = 0; i < slaveParticles.Count; ++i) {
			ParticleSystem slaveParticle = slaveParticles[i];
			if (slaveParticle != null && slaveParticle.isPlaying) slaveParticle.Pause();
		}
	}

	/// Resume all slave behaviours
	public virtual void Resume () {
		for (int i = 0; i < slaveBehaviours.Count; ++i) {
			Behaviour slaveBehaviour = slaveBehaviours[i];
			if (slaveBehaviour != null) slaveBehaviour.enabled = true;
		}
		for (int i = 0; i < slaveParticles.Count; ++i) {
			ParticleSystem slaveParticle = slaveParticles[i];
			if (slaveParticle != null && slaveParticle.isPaused) slaveParticle.Play();
		}
	}

	/// Pause event callback
	protected void OnPause(object sender, EventArgs e)
	{
		Pause();
	}

	/// Resume event callback
	protected void OnResume(object sender, EventArgs e)
	{
		Resume();
	}

}
