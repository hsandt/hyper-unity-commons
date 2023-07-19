using System.Collections;
using System.Collections.Generic;
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
/// If you have installed Introloop, Hyper Unity Commons Runtime assembly definition should define COM_E7_INTROLOOP
/// and you will unlock Introloop-specific API.
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

    /// Current Introloop Audio being played (only tracked correctly if set to Introloop or Loop)
    private IntroloopAudio m_CurrentIntroloopBgm;
    #endif


    protected override void Init()
    {
        base.Init();

        DebugUtil.Assert(bgmAudioSource != null, "[MusicManager] No Bgm Audio Source set on Music Manager", this);
        DebugUtil.Assert(stingerAudioSource != null, "[MusicManager] No Stinger Audio Source set on Music Manager", this);

        DebugUtil.AssertFormat(bgmAudioSource.loop,
            "[MusicManager] bgmAudioSource is not set to loop. BGM will still be played once, and natural end " +
            "of track will be detected, but this is probably a misconfiguration. Use Stinger for this instead.");
    }

    /// Play BGM if new
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
            if (m_CurrentIntroloopBgm != null)
            {
                StopIntroloopBgm();
            }
            #endif
        }
    }

    /// Stop current BGM
    public void StopBgm()
    {
        bgmAudioSource.Stop();
    }

    #if COM_E7_INTROLOOP

    /// Play Introloop BGM if new, and track it
    public void PlayIntroloopBgm(IntroloopAudio introloopBgm)
    {
        DebugUtil.AssertFormat(introloopBgm != null,
            "[MusicManager] PlayIntroloopBgm: passed introloopBgm is null. " +
            "if any BGM is playing, it will stop now");

        DebugUtil.AssertFormat(introloopBgm.Loop,
            "[MusicManager] PlayIntroloopBgm: passed introloopBgm is not looping. " +
            "It will still be played, but natural end of track won't be detected as we rely on " +
            "StopIntroloopBgm being called for this.");

        // m_CurrentIntroloopBgm *is* cleared on StopIntroloopBgm, so we can directly compare
        // references, considering null as "not playing"
        if (m_CurrentIntroloopBgm != introloopBgm)
        {
            m_CurrentIntroloopBgm = introloopBgm;
            introloopPlayer.Play(introloopBgm);

            // We should only play one BGM at a time, so stop native BGM if any
            // Note that if both the new native BGM and the current Introloop BGM share the same base AudioClip
            // (not recommended), the Introloop BGM will now replace it.
            if (bgmAudioSource.isPlaying)
            {
                StopBgm();
            }
        }
    }

    /// Stop current Introloop BGM and clear tracking field
    public void StopIntroloopBgm()
    {
        introloopPlayer.Stop();
        m_CurrentIntroloopBgm = null;
    }

    #endif

    /// Play BGM and return true if any, else do nothing return false
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

    public void PlayBgmWrapper(AudioAssetWrapper bgmWrapper)
    {
        bool success = PlayBgmWrapperIfAny(bgmWrapper);
        DebugUtil.AssertFormat(success, "[MusicManager] PlayBgmWrapper: bgmWrapper is null, could not play");
    }

    #if NL_ELRACCOONE_TWEENS
    public IEnumerator FadeOutBgmAsync(float duration)
    {
        // Remember old volume (in case it's not 1f)
        float oldVolume = bgmAudioSource.volume;

        // Fade out using tween
        // Note that while this is linear as we want, this clamps updates to the framerate
        // For a smoother update, consider upading the BGM AudioMixer volume instead (using log conversion)
        // See https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
        yield return bgmAudioSource.TweenAudioSourceVolume(0f, duration).Yield();

        // To clean up, stop BGM properly then restore old volume so BGM source is ready for next play
        bgmAudioSource.Stop();
        bgmAudioSource.volume = oldVolume;
    }
    #endif

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
