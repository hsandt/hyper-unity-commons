using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using HyperUnityCommons;
#if NL_ELRACCOONE_TWEENS
using ElRaccoone.Tweens;
#endif
#if COM_E7_INTROLOOP
using E7.Introloop;
#endif

/// Music Manager
/// Good to play simple looping BGM
/// Also used to play non-looping stingers, on a different source
/// If you have installed nl.elraccoone.tweens, Hyper Unity Commons Runtime assembly definition should define
/// NL_ELRACCOONE_TWEENS and you will unlock FadeOutBgmAsync.
/// If you have installed Introloop, Hyper Unity Commons Runtime assembly definition should define COM_E7_INTROLOOP
/// and you will unlock Introloop-specific API (and you don't need nl.elraccoone.tweens to fade out Introloop BGM).
/// We recommend to use the MusicManager_Base prefab provided in Commons Pattern/Audio, which has a child with
/// an audio source set as bgmAudioSource.
/// Then, if you need to customize things further (e.g. set Audio Source Output Mixer to game-specific BGM channel),
/// create a prefab variant of MusicManager_Base (e.g. MusicManager_Variant), and on it (esp. the audio source child),
/// override the properties you want.
/// This way, you benefit from the latest changes done on the base prefabs, while keeping your game-specific overrides.
public class MusicManager : SingletonManager<MusicManager>
{
    [Header("Children references")]

    [Tooltip("Audio Source for BGM")]
    public AudioSource bgmAudioSource;

    [Tooltip("Audio Source for stingers")]
    public AudioSource stingerAudioSource;


    #if COM_E7_INTROLOOP

    [Tooltip("Introloop Player used as alternative to bgmAudioSource. Place it on a extra child, along with disabled " +
        "AudioSource for introloop BGM. When playing introloop BGM, we use this directly (instead of " +
        "IntroloopPlayer.Instance which would create a new one)")]
    public IntroloopPlayer introloopPlayer;

    /// Current Introloop Audio being either played (including during fade-out), or paused
    /// (only tracked correctly if set to Introloop or Loop)
    /// This is merely to avoid patching Introloop source code with a public method to provide the current clip
    private IntroloopAudio m_CurrentIntroloopBgm;

    /// True iff Introloop Audio is currently playing, so this is false during pause
    /// (only tracked correctly if IntroloopAudio is set to Introloop or Loop)
    /// This is merely to avoid patching Introloop source code with some public method IsPlaying
    private bool m_IsIntroloopBgmPlaying;

    #endif


    protected override void Init()
    {
        base.Init();

        DebugUtil.Assert(bgmAudioSource != null, "[MusicManager] No Bgm Audio Source set on Music Manager", this);
        DebugUtil.Assert(stingerAudioSource != null, "[MusicManager] No Stinger Audio Source set on Music Manager", this);

        DebugUtil.AssertFormat(bgmAudioSource.loop,
            "[MusicManager] bgmAudioSource is not set to loop. BGM will still be played once, and natural end " +
            "of track will be detected, but this is probably a misconfiguration. Use Stinger for this instead.");

        #if COM_E7_INTROLOOP
        DebugUtil.Assert(introloopPlayer != null, "[MusicManager] introloopPlayer not set on Music Manager", this);
        #endif
    }


    #region NativeBgm

    /// Play native BGM if new, stopping any BGM currently playing (including Introloop BGM)
    public void PlayBgm(AudioClip bgm)
    {
        DebugUtil.AssertFormat(bgm != null, "[MusicManager] PlayBgm: passed bgm is null. " +
            "if any BGM is playing, it will stop now");

        // Only play if BGM is not already playing
        // clip is not automatically cleared on StopBgm, so check for !isPlaying first to make sure clip comparison
        // makes sense
        if (!bgmAudioSource.isPlaying || bgmAudioSource.clip != bgm)
        {
            // Note that bgmAudioSource.loop should be true on MusicManager_Base prefab
            bgmAudioSource.clip = bgm;
            bgmAudioSource.Play();

            #if COM_E7_INTROLOOP
            // We should only play one BGM at a time, so stop Introloop BGM if any
            // Note that if both the new native BGM and the current Introloop BGM share the same base AudioClip
            // (not recommended), the native BGM will now replace it.
            if (m_IsIntroloopBgmPlaying)
            {
                StopIntroloopBgm();
            }
            #endif
        }
    }

    /// Pause current native BGM if any
    public void PauseBgm()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }
    }

    /// Resume current native BGM if any and not already playing, stopping any BGM currently playing
    /// (including Introloop BGM)
    public void ResumeBgm()
    {
        if (!bgmAudioSource.isPlaying && bgmAudioSource.clip != null)
        {
            bgmAudioSource.Play();
        }
    }

    /// Stop current native BGM if any
    /// Always clear current clip
    public void StopBgm()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        // Cleanup to make timing of native clip being null or not consistent with m_CurrentIntroloopBgm
        // We do this even if source is not playing, in case it already stopped, so we still end with a clean state
        bgmAudioSource.clip = null;
    }

    #if NL_ELRACCOONE_TWEENS
    /// Fade out current native BGM over [duration] seconds and wait for fading to end
    public IEnumerator FadeOutBgmCoroutine(float duration)
    {
        // Remember old volume (in case it's not 1f)
        float oldVolume = bgmAudioSource.volume;

        // Fade out using tween
        // Note that while this is linear as we want, this clamps updates to the framerate
        // For a smoother update, consider updating the BGM AudioMixer volume instead (using log conversion)
        // See https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
        yield return bgmAudioSource.TweenAudioSourceVolume(0f, duration).Yield();

        // To clean up, stop BGM properly then restore old volume so BGM source is ready for next play
        bgmAudioSource.Stop();
        bgmAudioSource.volume = oldVolume;

        // Cleanup to make timing of native clip being null or not consistent with m_CurrentIntroloopBgm
        bgmAudioSource.clip = null;
    }

    /// Fade out current native BGM over [duration] seconds and wait for fading to end
    public async Task FadeOutBgmAsync(float duration)
    {
        // Same implementation as FadeOutBgmCoroutine, except using await
        float oldVolume = bgmAudioSource.volume;

        await bgmAudioSource.TweenAudioSourceVolume(0f, duration).Await();

        bgmAudioSource.Stop();
        bgmAudioSource.volume = oldVolume;

        // Cleanup to make timing of native clip being null or not consistent with m_CurrentIntroloopBgm
        bgmAudioSource.clip = null;
    }
    #endif

    #endregion


    #region IntroloopBgm

    #if COM_E7_INTROLOOP

    /// Play Introloop BGM if new, track it, and stop any BGM currently playing (including native BGM)
    public void PlayIntroloopBgm(IntroloopAudio introloopBgm, float fadeLengthSeconds = 0)
    {
        DebugUtil.AssertFormat(introloopBgm != null,
            "[MusicManager] PlayIntroloopBgm: passed introloopBgm is null. " +
            "if any BGM is playing, it will stop now");

        DebugUtil.AssertFormat(introloopBgm.Loop,
            "[MusicManager] PlayIntroloopBgm: passed introloopBgm is not looping. " +
            "It will still be played, but natural end of track won't be detected as we rely on " +
            "StopIntroloopBgm being called for this.");

        if (!m_IsIntroloopBgmPlaying || m_CurrentIntroloopBgm != introloopBgm)
        {
            // Custom tracking fields
            m_CurrentIntroloopBgm = introloopBgm;
            m_IsIntroloopBgmPlaying = true;

            introloopPlayer.Play(introloopBgm, fadeLengthSeconds);

            // We should only play one BGM at a time, so stop native BGM if any
            // Note that if both the new native BGM and the current Introloop BGM share the same base AudioClip
            // (not recommended), the Introloop BGM will now replace it.
            if (bgmAudioSource.isPlaying)
            {
                StopBgm();
            }
        }
    }

    /// Pause current Introloop BGM and clear tracking field
    public void PauseIntroloopBgm()
    {
        if (m_IsIntroloopBgmPlaying)
        {
            introloopPlayer.Pause();
        }
    }

    /// Resume current Introloop BGM and clear tracking field
    public void ResumeIntroloopBgm()
    {
        if (!m_IsIntroloopBgmPlaying && m_CurrentIntroloopBgm != null)
        {
            introloopPlayer.Resume();
        }
    }

    /// Stop current Introloop BGM and clear tracking fields
    public void StopIntroloopBgm()
    {
        if (m_IsIntroloopBgmPlaying)
        {
            introloopPlayer.Stop();
            m_IsIntroloopBgmPlaying = false;
        }

        // Always clear clip to be safe, although since we're not supposed to use non-looping Introloop audio assets,
        // they shouldn't have stopped naturally, so after playing Introloop audio, m_IsIntroloopBgmPlaying should be
        // true anyway, so this should also work if placed inside the block above
        m_CurrentIntroloopBgm = null;
    }

    /// Fade out current Introloop BGM over [duration] seconds, wait for fading to end and clear tracking field
    public async Task FadeOutIntroloopBgmAsync(float duration)
    {
        introloopPlayer.Stop(duration);

        // There is no way to poll introloopPlayer to check when it finished fading out, so we run a parallel
        // Delay. This is not perfect, but enough for common usage (no time scale, no pause during fade-out, etc.).
        // If you want to actually check that introloopPlayer is done, you'll need to patch the source code to add
        // a public IsPlaying method that checks if the current track is playing.
        await Task.Delay(TimeSpan.FromSeconds(duration));

        // Fade-out should be complete about now, clear tracking fields
        m_CurrentIntroloopBgm = null;
        m_IsIntroloopBgmPlaying = false;
    }

    #endif

    #endregion


    #region AnyBgm

    /// Play BGM wrapper and return true if any, else do nothing return false
    public bool PlayBgmWrapperIfAny(AudioAssetWrapper bgmWrapper)
    {
        if (bgmWrapper != null)
        {
            #if COM_E7_INTROLOOP
            if (bgmWrapper.introloopAudio != null)
            {
                PlayIntroloopBgm(bgmWrapper.introloopAudio);
                return true;
            }
            #endif

            if (bgmWrapper.nativeAudioClip != null)
            {
                PlayBgm(bgmWrapper.nativeAudioClip);
                return true;
            }
        }

        return false;
    }

    /// Play BGM wrapper
    public void PlayBgmWrapper(AudioAssetWrapper bgmWrapper)
    {
        bool success = PlayBgmWrapperIfAny(bgmWrapper);
        DebugUtil.AssertFormat(success, "[MusicManager] PlayBgmWrapper: bgmWrapper is null, could not play");
    }

    /// Pause current BGM of any type
    public void PauseAnyBgm()
    {
        #if COM_E7_INTROLOOP
        if (m_IsIntroloopBgmPlaying)
        {
            PauseIntroloopBgm();

            // We're not supposed to have both an introloop and native BGM playing, so return now
            return;
        }
        #endif

        if (bgmAudioSource.isPlaying)
        {
            PauseBgm();
        }
    }

    /// Resume current BGM of any type
    public void ResumeAnyBgm()
    {
        #if COM_E7_INTROLOOP
        if (!m_IsIntroloopBgmPlaying && m_CurrentIntroloopBgm != null)
        {
            ResumeIntroloopBgm();

            // We're not supposed to have both an introloop and native BGM paused and still set, so return now
            return;
        }
        #endif

        if (bgmAudioSource.isPlaying)
        {
            ResumeBgm();
            return;
        }
    }

    /// Stop current BGM of any type
    public void StopAnyBgm()
    {
        #if COM_E7_INTROLOOP
        if (m_IsIntroloopBgmPlaying)
        {
            StopIntroloopBgm();

            // We're not supposed to have both an introloop and native BGM paused and still set, so return now
            return;
        }
        #endif

        if (bgmAudioSource.isPlaying)
        {
            StopBgm();
        }
    }

    /// Fade out current BGM of any type over [duration] seconds
    public async void FadeOutAnyBgm(float duration)
    {
        await FadeOutAnyBgmAsync(duration);
    }

    /// Fade out current BGM of any type over [duration] seconds and wait for fading to end
    public async Task FadeOutAnyBgmAsync(float duration)
    {
        #if COM_E7_INTROLOOP
        if (m_IsIntroloopBgmPlaying)
        {
            await FadeOutIntroloopBgmAsync(duration);

            // We're not supposed to have both an introloop and native BGM paused and still set, so return now
            // (and since we awaited, even if we didn't return, it would be too late to fade out another BGM now)
            return;
        }
        #endif

        if (bgmAudioSource.isPlaying)
        {
            await FadeOutBgmAsync(duration);
        }
    }

    #endregion


    public void PlayStinger(AudioClip stinger)
    {
        if (!stingerAudioSource.isPlaying || stingerAudioSource.clip != stinger)
        {
            // Note that stingerAudioSource.loop should be false on MusicManager_Base prefab
            stingerAudioSource.clip = stinger;
            stingerAudioSource.Play();
        }
    }

    /// Play stinger and wait for it to end
    /// Note that this will consider pause as being stopped
    public IEnumerator PlayStingerAsync(AudioClip stinger)
    {
        PlayStinger(stinger);

        // Wait until audio source is not playing
        // This considers paused sources as not playing, so if you need to support paused sources,
        // prefer tracking pause state in your own members and using your own coroutine method
        // to check tracked pause state.
        yield return new WaitUntil(() => !stingerAudioSource.isPlaying);
    }
}
