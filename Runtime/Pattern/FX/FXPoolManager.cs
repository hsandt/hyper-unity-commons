using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX Pool Manager
///
/// We support 2 main types of FX: FX using an Animator + animation, and FX using a Particle System
/// Both may be one-shot or looping, but different operations may be used depending on the specific type.
///
/// a. One-shot FX should be spawned via PlayOneShotFXAsync. This ensures that the FX game object will be deactivated
/// when the FX is over (whether by animation time check of particle system Stop Action callback), so it can be reused
/// for pooling.
/// b. Looping FX should be spawned via PlayLoopingFX and handled to be stopped and deactivated later when needed,
/// so it can be reused for pooling.
/// If FX is a looping animation, caller can also await
/// fx.WaitForLastAnimationFinishedOneCycleAsync/fx.WaitForTaggedAnimationRunningAsync to guarantee
/// some FX intro before proceeding.
/// It is a StandardMasterPooledObject, instead of just a StandardPooledObject, so it can benefit MasterBehaviour's
/// features to auto-manage Animator and ParticleSystem for Pause/Resume. We also use reuse slave animator as main
/// animator, if any.
///
/// SEO: after LocatorManager
public class FXPoolManager : MultiPoolManager<FX, FXPoolManager>
{
    [SerializeField, Tooltip("Tag used to find FX Pool, if Pool Transform is not set in the inspector and this field " +
         "is not empty")]
    private string fxPoolTag = "FXPool";


    protected override void Init()
    {
        if (poolTransform == null)
        {
            poolTransform = LocatorManager.Instance.FindWithTag(fxPoolTag)?.transform;
        }

        base.Init();
    }

    /// Spawn one-shot (non-looping) FX by name at [anchorPosition] and play any associated SFX at [sfxVolumeScale]
    /// Assume that FX is played automatically on activation and wait for FX end,
    /// deactivating it if needed so it's considered released for reuse in pooling.
    /// ! Do not auto-Release the FX in animation event at the end of the animation, or it will prevent animation end
    /// detection and awaiting this method will never end !
    public async Task<FX> PlayOneShotFXAsync(string fxName, Vector3 anchorPosition, float sfxVolumeScale = 1f)
    {
        // Start like SpawnFX. The only reason we don't just call it is to insert the custom non-looping check
        // in the middle
        FX fx = AcquireFreeObject(fxName);
        if (fx != null)
        {
            #if UNITY_EDITOR
            // If FX uses Particle System, check that it is not looping
            // For FX using Animator, it's harder to decide as non-looping animations may still result in a looping
            // animator state cycle if the animator states are connected in circle, etc.
            if (fx is FXWithParticleSystem fxWithParticleSystem)
            {
                DebugUtil.AssertFormat(!fxWithParticleSystem.MainParticleSystem.main.loop,
                    fxWithParticleSystem.MainParticleSystem,
                    "[FXPoolManager] PlayOneShotFXAsync: FX '{0}' uses Particle System but the latter is " +
                    "set to loop, so it shouldn't be played as one-shot.",
                    fx);
            }
            #endif

            // Setup, as it's a MasterBehaviour, and besides FX subclass may have additional setup
            fx.Setup();

            fx.WarpTo(anchorPosition);
            fx.PlaySfxIfAny(sfxVolumeScale);

            // Then wait for FX to complete (method depends on FX subclass) and release it
            // (important because IsInUse only checks for game object active state, not whether FX is actually over)
            await fx.WaitForPlayOneShotCompletion();
            fx.Release();

            return fx;
        }

        Debug.LogErrorFormat(this,
            "[FXPoolManager] PlayOneShotFXAsync: Could not acquire free object for fxName '{0}'",
            fxName);
        return null;
    }


    /// Spawn FX by name at [anchorPosition] and play any associated SFX at [sfxVolumeScale]
    /// Looping FX must use this. One-shot FX can use this too, but if you don't need to store the returned FX
    /// immediately before waiting for completion, consider PlayOneShotFXAsync instead.
    /// Keep it running, but return FX so the caller can:
    /// a. await fx.WaitForPlayOneShotCompletion for one-shot FX if they want to do something after playing,
    ///    generally Release
    /// b. stop it manually later (with immediate deactivation or fade-out), so it can be reused for pooling
    ///    (generally for looping FX, but could also be done to interrupt a one-shot FX before it ends)
    /// If the FX is looping but you need to wait for one cycle, or that the FX finishes some intro,
    /// consider getting the returned fx then call fx.WaitForLastAnimationFinishedOneCycleAsync or
    /// fx.WaitForTaggedAnimationRunningAsync.
    public FX SpawnFX(string fxName, Vector3 anchorPosition, float sfxVolumeScale = 1f)
    {
        // Acquire FX. This will activate the game object. We assume the FX starts playing automatically.
        FX fx = AcquireFreeObject(fxName);
        if (fx != null)
        {
            // Setup, as it's a MasterBehaviour, and besides FX subclass may have additional setup
            fx.Setup();

            fx.WarpTo(anchorPosition);
            fx.PlaySfxIfAny(sfxVolumeScale);
            return fx;
        }

        Debug.LogErrorFormat(this,
            "[FXPoolManager] SpawnFX: Could not acquire free object for fxName '{0}'",
            fxName);
        return null;
    }

    public void PauseAllFX()
    {
        foreach (FX fx in GetObjectsInUseInAllPools())
        {
            fx.Pause();
        }
    }

    public void ResumeAllFX()
    {
        foreach (FX fx in GetObjectsInUseInAllPools())
        {
            fx.Resume();
        }
    }
}
