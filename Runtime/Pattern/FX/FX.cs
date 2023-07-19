using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX component for pooled objects
/// We support 3 main types of FX:
/// a. FX playing a one-time Animation should call Release by themselves at the end of the animation, using some
/// animation event bound to FX.Release
/// b. FX playing a looping animation cannot simply auto-release via animation event. Instead, we offer a way to detect
/// when one animation cycle is over (WaitForLastAnimationFinishedOneCycleAsync and WaitForTaggedAnimationRunningAsync)
/// and the FX must be deactivated via code when they need to stop.
/// c. FX playing a Particle System should have Stop Action on the main particle system set to Disable (but not on
/// sub-particle systems). Those should use FXWithParticleSystem.
/// It is a StandardMasterPooledObject, instead of just a StandardPooledObject, so it can benefit MasterBehaviour's
/// features to auto-manage Animator and ParticleSystem for Pause/Resume, since FX may use one or the other to represent
/// a visual effect.
/// We also use reuse slave animator as main animator, if any.
public class FX : StandardMasterPooledObject
{
    [Header("Parameters")]

    [Tooltip("If checked, SFX will be played using Stack Volume Modifier set on SfxPoolManager. " +
        "Recommended when it's possible to spawn many instances of that FX (or at least playing the same SFX) " +
        "to avoid sound clutter.")]
    private bool sfxUseStackVolumeModifier = true;


    /* Sibling components (optional) */

    private SpriteRenderer[] m_SpriteRenderers;
    public SpriteRenderer[] SpriteRenderers => m_SpriteRenderers;

    private SfxPlayer m_SfxPlayer;


    protected override void Init()
    {
        // Optional
        m_SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        m_SfxPlayer = GetComponent<SfxPlayer>();
    }

    /// Setup this FX and play SFX if any
    /// - place it at passed anchorPosition (this ignores any prefab root local position, so if you need an offset,
    /// place the actual FX Sprite as a child under that root, with local position set to this offset)
    /// - clear sprite(s) (only useful for Timeline signals)
    /// - play SFX if any at passed sfxVolumeScale
    public void Setup(Vector3 anchorPosition, float sfxVolumeScale = 1f)
    {
        // Move FX to target anchor position
        transform.position = anchorPosition;

        // Clear the sprite(s)
        // While generally not useful as animation would set the sprite before next render,
        // when an FX is spawned from a Timeline signal, this happens late in the frame, after Animator update and
        // before SpriteRenderer render.
        // As a result, the FX would be shown for 1 frame with its default sprite(s) set in prefab
        // (often a representative sprite in the middle of the animation), and the Animator will set
        // the sprite to the first sprite of the animation only on next frame, leaving a visual glitch.
        // https://forum.unity.com/threads/animation-lags-by-1-frame-when-activating-object-via-timeline-signal-callback.1293825/
        foreach (SpriteRenderer spriteRenderer in m_SpriteRenderers)
        {
            spriteRenderer.sprite = null;
        }

        PlaySfxIfAny(sfxVolumeScale);
    }

    public void PlaySfxIfAny(float sfxVolumeScale = 1f)
    {
        if (m_SfxPlayer != null)
        {
            m_SfxPlayer.PlaySFX(sfxVolumeScale, sfxUseStackVolumeModifier);
        }
    }

    public async Task WaitForLastAnimationFinishedOneCycleAsync()
    {
        if (slaveAnimator != null)
        {
            await slaveAnimator.WaitForLastAnimationFinishedOneCycleAsync();
        }
        else
        {
            DebugUtil.LogErrorFormat(this,
                "[FX] WaitForLastAnimationFinishedOneCycleAsync: no slave animator on {0}. " +
                "Make sure to set it manually or check Add Sibling Components As Slaves",
                this);
        }
    }

    public async Task WaitForTaggedAnimationRunningAsync(int tagHash)
    {
        if (slaveAnimator != null)
        {
            await slaveAnimator.WaitForTaggedAnimationRunningAsync(tagHash);
        }
        else
        {
            DebugUtil.LogErrorFormat(this,
                "[FX] WaitForLastAnimationFinishedOneCycleAsync: no slave animator on {0}. " +
                "Make sure to set it manually or check Add Sibling Components As Slaves",
                this);
        }
    }
}
