using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;

/// FX component for objects with Animator (auto-Release when stopped)
/// See explanations in FX docstring
public class FXWithAnimation : FX
{
    /* Sibling components (optional) */

    private SpriteRenderer[] m_SpriteRenderers;
    public SpriteRenderer[] SpriteRenderers => m_SpriteRenderers;


    protected override void Init()
    {
        // Optional
        m_SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public override void Setup()
    {
        base.Setup();

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
    }

    public override Task WaitForPlayOneShotCompletion()
    {
        // One-shot FXs end when they finish one cycle
        return WaitForLastAnimationFinishedOneCycleAsync();
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
                "[FX] WaitForTaggedAnimationRunningAsync: no slave animator on {0}. " +
                "Make sure to set it manually or check Add Sibling Components As Slaves",
                this);
        }
    }
}
