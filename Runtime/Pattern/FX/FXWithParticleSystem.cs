using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HyperUnityCommons;

/// FX component for objects with Particle System (auto-Release when stopped)
public class FXWithParticleSystem : FX
{
    protected override void Init()
    {
        base.Init();

        // ParticleSystem has a stopAction system that allows us to automatically deactivate, and therefore Release,
        // the particle game object when all particles, including sub-emitters / sub-particles on children, have
        // finished playing. However, only the root game object needs to be deactivated and, in fact, deactivating child
        // objects would mess up with future pooled FX usage and would require us to reactivate those objects on next
        // Acquire.
        // So, in the inspector, you should set the particle system at the root is set to Disable on stop,
        // and sub-particles not to Disable (nor Destroy) on stop. The code below simply checks this, and fixes any
        // bad values for safety (but that's only done at runtime initialization, it still needs fixing in the editor).

        var particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in particleSystems)
        {
            ParticleSystem.MainModule main = system.main;

            if (system.gameObject == gameObject)
            {
                ParticleSystemStopAction stopAction = main.stopAction;
                if (stopAction != ParticleSystemStopAction.Disable)
                {
                    DebugUtil.LogWarningFormat(this, "[FXWithParticleSystem] Init: stopAction of main module " +
                        "of root particle system of {0} is {1}, expected Disable. Setting it to Disable now, " +
                        "but please set it in inspector for next time",
                        this, stopAction);

                    main.stopAction = ParticleSystemStopAction.Disable;
                }
            }
            else
            {
                ParticleSystemStopAction stopAction = main.stopAction;
                if (stopAction is ParticleSystemStopAction.Disable or ParticleSystemStopAction.Destroy)
                {
                    DebugUtil.LogWarningFormat(system, "[FXWithParticleSystem] Init: stopAction of main module " +
                        "of child particle system {0} of {1} is {2}, but it should not be Disable nor Destroy to allow pooling (Disable could work " +
                        "in theory, but currently, children are not reactivated on next Acquire). Setting it to None " +
                        "now, but please change it in inspector for next time",
                        system, this, stopAction);

                    main.stopAction = ParticleSystemStopAction.None;
                }
            }
        }
    }
}
