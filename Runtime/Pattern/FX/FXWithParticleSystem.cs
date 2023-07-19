using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX component for objects with Particle System (auto-Release when stopped)
/// See explanations in FX docstring
public class FXWithParticleSystem : FX
{
    private TaskCompletionSource<bool> m_PlayOneShotCompletionSource;

    protected override void Init()
    {
        base.Init();

        // ParticleSystem has a stopAction mode "Callback" that allows us to automatically call OnParticleSystemStopped,
        // and therefore Release the root particle game object, when all particles, including sub-emitters / sub-particles
        // on children, have finished playing (there is also a Disable mode which is easier to use, but doesn't let us
        // notify the spawning script via WaitForPlayOneShotCompletion, which would then need to have a custom loop
        // that periodically waits a short amount of time and checks if game object is still active).
        // However, only the root game object needs to use a callback this way to deactivate and, in fact, deactivating child
        // objects would mess up with future pooled FX usage and would require us to reactivate those objects on next
        // Acquire.
        // So, in the inspector, you should set the particle system at the root is set to Callback on stop,
        // and sub-particles not to Disable (nor Destroy) on stop. The code below simply checks this, and fixes any
        // bad values for safety (but that's only done at runtime initialization, it still needs fixing in the editor).

        var particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in particleSystems)
        {
            ParticleSystem.MainModule main = system.main;

            if (system.gameObject == gameObject)
            {
                ParticleSystemStopAction stopAction = main.stopAction;
                if (stopAction != ParticleSystemStopAction.Callback)
                {
                    DebugUtil.LogWarningFormat(this, "[FXWithParticleSystem] Init: stopAction of main module " +
                        "of root particle system of {0} is {1}, expected Callback. Setting it to Callback now, " +
                        "but please set it in inspector for next time",
                        this, stopAction);

                    main.stopAction = ParticleSystemStopAction.Callback;
                }
            }
            else
            {
                ParticleSystemStopAction stopAction = main.stopAction;
                if (stopAction is ParticleSystemStopAction.Disable or ParticleSystemStopAction.Destroy)
                {
                    DebugUtil.LogWarningFormat(system, "[FXWithParticleSystem] Init: stopAction of main module " +
                        "of child particle system {0} of {1} is {2}, but it should not be Disable nor Destroy to allow " +
                        "pooling (Disable could work in theory, but currently, children are not reactivated on next Acquire). " +
                        "Setting it to None now, but please change it in inspector for next time",
                        system, this, stopAction);

                    main.stopAction = ParticleSystemStopAction.None;
                }
            }
        }
    }

    public override async Task WaitForPlayOneShotCompletion()
    {
        m_PlayOneShotCompletionSource = new TaskCompletionSource<bool>();
        await m_PlayOneShotCompletionSource.Task;
        m_PlayOneShotCompletionSource = null;
    }

    /// Callback for main ParticleSystem with stopAction = ParticleSystemStopAction.Callback
    private void OnParticleSystemStopped()
    {
        if (m_PlayOneShotCompletionSource != null)
        {
            // If some caller code was awaiting in WaitForPlayOneShotCompletion, unlock them now
            m_PlayOneShotCompletionSource.SetResult(true);
        }
    }
}
