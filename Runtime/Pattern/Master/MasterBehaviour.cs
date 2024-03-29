using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HyperUnityCommons
{
    /// Master Behaviour component
    /// Place it on a game object as the main component. It will control "slaves", namely Behaviours added manually
    /// as well as ClearableBehaviours, Animator, Rigidbody2D and ParticleSystems added manually or found if
    /// addSiblingComponentsAsSlaves is set to true.
    /// Caution: Setup will enable all slaves, so disabling Behaviours in the Inspector will not work as expected.
    public class MasterBehaviour : ClearableBehaviour, IPausable
    {
        [Header("Parameters")]

        [SerializeField, Tooltip("Check to automatically register all ClearableBehaviour, any Animator, " +
            "and any Particle Systems, as slaves on start. Note that we don't check for duplicates, " +
            "so if you check this, do not add sibling slaves manually at all.")]
        private bool addSiblingComponentsAsSlaves = true;

        [SerializeField, Tooltip("If checked, all the slave particles are paused and resumed with their children.")]
        private bool pauseSlaveParticleSystemsWithChildren = true;


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

        // Rigidbody2D is not a Behavior, and must be handled with custom code
        [Tooltip("Rigidbody2D to pause and resume")]
        public Rigidbody2D slaveRigidbody2D;

        // ParticleSystems are other types of Components with their own Play/Pause methods, so they are in another list
        [Tooltip("Particle systems to pause and resume")]
        public List<ParticleSystem> slaveParticles;


        /* State */

        /// True iff this entity is paused
        private bool m_IsPaused = false;


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
                // To avoid infinite recursion on Setup/Clear, do not register the Master script itself as its own Slave!
                if (clearableBehaviour != this)
                {
                    // Don't check if already in list, we've warned user that we don't check for duplicates in tooltip
                    slaveBehaviours.Add(clearableBehaviour);
                }
            }

            // Not all characters have those components, so don't fail if you don't find one
            slaveAnimator = GetComponent<Animator>();
            slaveRigidbody2D = GetComponent<Rigidbody2D>();

            // Cumulate slave particles set manually with any sibling particle systems found
            // Don't check if already in list, we've warned user that we don't check for duplicates in tooltip
            slaveParticles.AddRange(GetComponents<ParticleSystem>());
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
            // Note that the master script itself is not disabled by Pause(), in case it needs to keep processing things
            // during the pause. For master script custom pause behaviour, override Pause
            // (and remember to call base.Pause() inside)
            m_IsPaused = true;

            foreach (Behaviour slaveBehaviour in slaveBehaviours)
            {
                // Unlike Setup/Clear which tries to cast to ClearableBehaviour to delegate Setup/Clear,
                // we don't try to cast to IPausable to try to delegate Pause/Resume.
                // This is because most of the time, we want to disable the script anyway to stop calling
                // Update/FixedUpdate, and Unity provides OnEnable/OnDisable to plug custom behavior on
                // enabling/disabling, so this is more convenient to use.
                // Note that IPausable is still useful when it comes to identifying and storing a bunch of objects
                // with specific pause behaviours, even if they are not Unity Behaviours with OnEnable/OnDisable,
                // or when you want to allow overriding such behavior (which is the case with this MasterBehaviour script).
                // To sum-up: define OnDisable on your slave behavior script.
                if (slaveBehaviour != null) slaveBehaviour.enabled = false;
            }

            if (slaveAnimator != null) slaveAnimator.enabled = false;
            if (slaveRigidbody2D != null) slaveRigidbody2D.simulated = false;

            foreach (ParticleSystem slaveParticle in slaveParticles)
            {
                if (slaveParticle != null && slaveParticle.isPlaying)
                {
                    slaveParticle.Pause(pauseSlaveParticleSystemsWithChildren);
                }
            }
        }

        /// Resume all slave behaviours
        public virtual void Resume()
        {
            m_IsPaused = false;

            foreach (Behaviour slaveBehaviour in slaveBehaviours)
            {
                // Same remark as in Pause
                // To sum-up: define OnEnable on your slave behavior script.
                if (slaveBehaviour != null) slaveBehaviour.enabled = true;
            }

            if (slaveAnimator != null) slaveAnimator.enabled = true;
            if (slaveRigidbody2D != null) slaveRigidbody2D.simulated = true;

            foreach (ParticleSystem slaveParticle in slaveParticles)
            {
                if (slaveParticle != null && slaveParticle.isPaused)
                {
                    slaveParticle.Play(pauseSlaveParticleSystemsWithChildren);
                }
            }
        }

        public bool IsPaused()
        {
            return m_IsPaused;
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
