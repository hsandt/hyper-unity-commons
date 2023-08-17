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

    // Note: we didn't add Animator as this is already stored in slaveAnimator

    /// Sprite renderers: used by Fighter
    private SpriteRenderer[] m_SpriteRenderers;
    public SpriteRenderer[] SpriteRenderers => m_SpriteRenderers;


    protected override void Init()
    {
        base.Init();

        // Optional
        m_SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public override void Setup()
    {
        base.Setup();

        if (slaveAnimator != null)
        {
            // When an FX is spawned from a Timeline signal, this happens late in the frame, after Animator update and
            // before render (e.g. of SpriteRenderer). As a result, the FX would be shown for 1 frame with its default
            // state set in prefab (e.g. a representative sprite with alpha = 1), instead of showing the first key of
            // the animation (e.g. a sprite with alpha = 0, a different sprite than the representative one, etc.),
            // leaving a visual glitch.
            // One trick is to Update the animator by delta time to force refresh animation to show the first frame.
            // https://forum.unity.com/threads/animation-lags-by-1-frame-when-activating-object-via-timeline-signal-callback.1293825/
            slaveAnimator.Update(Time.deltaTime);
        }
        else
        {
            DebugUtil.LogErrorFormat(this,
                "[FXWithAnimation] Setup: no slave animator on {0}. " +
                "Make sure to set it manually or check Add Sibling Components As Slaves",
                this);
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
                "[FXWithAnimation] WaitForLastAnimationFinishedOneCycleAsync: no slave animator on {0}. " +
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
                "[FXWithAnimation] WaitForTaggedAnimationRunningAsync: no slave animator on {0}. " +
                "Make sure to set it manually or check Add Sibling Components As Slaves",
                this);
        }
    }
}
