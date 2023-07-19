using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX component for pooled objects
/// We support 3 main types of FX:
/// a. FX playing a one-time animation. They should have an FXWithAnimation component and be spawned via
/// FXPoolManager.Instance.PlayOneShotFXAsync(), which will handle deactivating the game object at the end of the
/// single animation (calling code can also await it if they want), so it can be reused for pooling.
/// b. FX playing a looping animation. They should have an FXWithAnimation component and be spawned via
/// FXPoolManager.Instance.SpawnFX() + await WaitForLastAnimationFinishedOneCycleAsync/WaitForTaggedAnimationRunningAsync
/// depending on the needs, and let some other code deactivate the game object later when we want to, so it can be reused
/// for pooling.
/// c. FX playing a one-time Particle System should have Stop Action on the main particle system set to Callback,
/// have an FXWithParticleSystem component and be spawned via FXPoolManager.Instance.PlayOneShotFXAsync(), which will
/// handle deactivating the game object at the end of the particle system.
/// It is a StandardMasterPooledObject, instead of just a StandardPooledObject, so it can benefit MasterBehaviour's
/// features to auto-manage Animator and ParticleSystem for Pause/Resume, since FX may use one or the other to represent
/// a visual effect. We also use reuse slave animator as main animator, if any.
public abstract class FX : StandardMasterPooledObject
{
    [Header("Parameters")]

    [Tooltip("If checked, SFX will be played using Stack Volume Modifier set on SfxPoolManager. " +
        "Recommended when it's possible to spawn many instances of that FX (or at least playing the same SFX) " +
        "to avoid sound clutter.")]
    private bool sfxUseStackVolumeModifier = true;


    /* Sibling components (optional) */

    private SfxPlayer m_SfxPlayer;


    protected override void Init()
    {
        // Optional
        m_SfxPlayer = GetComponent<SfxPlayer>();
    }

    /// Place FX at passed anchorPosition (this ignores any prefab root local position, so if you need an offset,
    /// place the actual FX Sprite as a child under that root, with local position set to this offset)
    public void WarpTo(Vector3 anchorPosition)
    {
        // Move FX to target anchor position
        transform.position = anchorPosition;
    }

    public abstract Task WaitForPlayOneShotCompletion();

    /// Play any SFX associated at passed sfxVolumeScale
    public void PlaySfxIfAny(float sfxVolumeScale = 1f)
    {
        if (m_SfxPlayer != null)
        {
            m_SfxPlayer.PlaySFX(sfxVolumeScale, sfxUseStackVolumeModifier);
        }
    }
}
