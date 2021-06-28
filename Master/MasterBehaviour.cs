using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CommonsPattern
{
    public class MasterBehaviour : ClearableBehaviour, IPausable
    {
        [Header("Parameters")]
        
        [SerializeField, Tooltip("Check to automatically register all ClearableBehaviour, and any Animator, as slaves on start")]
        private bool addSiblingComponentsAsSlaves = true;
        
        
        [Header("Slave components")]
        
        // Behaviour is broader than MonoBehaviour and contains all native Unity components that have an Update event,
        // along with the OnEnable and OnDisable events.
        // In Awake(), make sure you register sibling behaviours you need to pause with slaveBehaviours.Add(myBehaviour);
        // Often, you can just register all the components found with GetComponents<ClearableBehaviour>, except the Master
        // component itself (check clearableBehaviour != this).
        [Tooltip("Slave behaviours to pause and resume. Only set non-sibling components in the Inspector, " +
                 "as sibling components should be added programmatically.")]
        public List<Behaviour> slaveBehaviours;

        // Animator is a Behavior, but it has a special way to be Restarted, and we don't want to do
        // another dynamic cast with "as" in Clear, so we store its reference in a separate member
        [Tooltip("Animator to pause and resume")]
        public Animator slaveAnimator;

        // ParticleSystems are other types of Components with their own Play/Pause methods, so they are in another list
        [Tooltip("Particle systems to pause and resume")]
        public List<ParticleSystem> slaveParticles;
        
        
        private void Awake()
        {
            if (addSiblingComponentsAsSlaves)
            {
                // Auto-add option was checked, so retrieve common component types automatically to register them as slaves.
                // This is often enough, but if you need to register a few custom components too, you can always do that
                // manually in the Inspector.
                AddSiblingSlaveBehaviours();
            }

            Init();
        }
        
        /// Override this method to customize Awake behavior while preserving Awake's behaviour
        protected virtual void Init() {}
        
        /// Add all ClearableBehaviour components as slave behaviours, and any Animator component as slave animator
        protected void AddSiblingSlaveBehaviours()
        {
            var clearableBehaviours = GetComponents<ClearableBehaviour>();
            foreach (var clearableBehaviour in clearableBehaviours)
            {
                // to avoid infinite recursion on Setup/Clear, do not register the Master script itself as its own Slave!
                if (clearableBehaviour != this)
                {
                    slaveBehaviours.Add(clearableBehaviour);
                }
            }

            // Not all characters have animators, so don't fail if you don't find one 
            slaveAnimator = GetComponent<Animator>();
        }
        
        
        /* ClearableBehaviour methods */

        public override void Setup()
        {
            // Enable all behaviours. Useful because we disable the behaviours in Clear(), and also
            // because when restarting a level from an in-game menu that paused the game, we need to "Resume" the scripts.
            foreach (Behaviour slaveBehaviour in slaveBehaviours)
            {
                if (slaveBehaviour != null) slaveBehaviour.enabled = true;

                // If the slave behaviour is also a ClearableBehaviour, Setup it now. This allows not to Setup everything manually in the derived class.
                ClearableBehaviour slaveClearableBehaviour = slaveBehaviour as ClearableBehaviour;
                if (slaveClearableBehaviour)
                    slaveClearableBehaviour.Setup();
            }

            if (slaveAnimator != null)
            {
                slaveAnimator.enabled = true;
            }
        }

        public override void Clear()
        {
            foreach (Behaviour slaveBehaviour in slaveBehaviours)
            {
                if (slaveBehaviour != null) slaveBehaviour.enabled = false;

                // If the slave behaviour is also a ClearableBehaviour, Clear it now. This allows not to Clear everything manually in the derived class.
                ClearableBehaviour slaveClearableBehaviour = slaveBehaviour as ClearableBehaviour;
                if (slaveClearableBehaviour)
                    slaveClearableBehaviour.Clear();
            }

            if (slaveAnimator != null)
            {
                slaveAnimator.enabled = false;
                slaveAnimator.Rebind();
            }

            foreach (ParticleSystem slaveParticle in slaveParticles)
            {
                if (slaveParticle != null)
                {
                    // Whatever the current state of the particles, stop and clear them completely
                    // This is only required if the game object is not deactivated before/after being Cleared,
                    // so that particles do not remain in their current state and replay from there.
                    // Caution: in this case, even if they should play on start, they won't on next Setup.
                    if (slaveParticle.isPlaying || slaveParticle.isPaused)
                    {
                        slaveParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                    else if (slaveParticle.IsAlive())
                    {
                        // it must have been stopped with Stop(ParticleSystemStopBehavior.StopEmitting)
                        slaveParticle.Clear();
                    }
                }
            }
        }

        
        /* IPausable interface */

        /// Pause all slave behaviours
        public virtual void Pause()
        {
            foreach (Behaviour slaveBehaviour in slaveBehaviours)
            {
                if (slaveBehaviour != null) slaveBehaviour.enabled = false;
            }

            if (slaveAnimator != null) slaveAnimator.enabled = false;

            foreach (ParticleSystem slaveParticle in slaveParticles)
            {
                if (slaveParticle != null && slaveParticle.isPlaying) slaveParticle.Pause();
            }
        }

        /// Resume all slave behaviours
        public virtual void Resume()
        {
            foreach (Behaviour slaveBehaviour in slaveBehaviours)
            {
                if (slaveBehaviour != null) slaveBehaviour.enabled = true;
            }

            if (slaveAnimator != null) slaveAnimator.enabled = true;

            foreach (ParticleSystem slaveParticle in slaveParticles)
            {
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
}