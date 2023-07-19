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
/// Good to play simple looping BGM, but doesn't support intro-loop.
/// Also used to play non-looping stingers, on a different source.
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


    /* Cached children references */

    #if COM_E7_INTROLOOP
    /// Introloop player, sibling of bgmAudioSource
    /// When using Introloop with MusicManager, we should add an IntroloopPlayer component directly besides the
    /// bgmAudioSource (which would be deactivated). So do not use IntroloopPlayer.Instance to avoid creating
    /// yet another singleton instance, instead use MusicManager as the singleton parent and IntroloopPlayer as
    /// its child.
    private IntroloopPlayer m_IntroloopPlayer;
    #endif


    protected override void Init()
    {
        base.Init();

        DebugUtil.Assert(bgmAudioSource != null, "[InGameManager] No Bgm Audio Source set on Music Manager", this);
        DebugUtil.Assert(stingerAudioSource != null, "[InGameManager] No Stinger Audio Source set on Music Manager", this);

        #if COM_E7_INTROLOOP
        m_IntroloopPlayer = bgmAudioSource.GetComponent<IntroloopPlayer>();
        #endif
    }

    public void PlayBgm(AudioClip bgm)
    {
        if (!bgmAudioSource.isPlaying || bgmAudioSource.clip != bgm)
        {
            // Note that bgmAudioSource.loop should be true on MusicManager_Base prefab
            bgmAudioSource.clip = bgm;
            bgmAudioSource.Play();
        }
    }

    public void StopBgm()
    {
        bgmAudioSource.Stop();
    }

    #if COM_E7_INTROLOOP
    // TODO: add Introloop implementation
    #endif

    #if NL_ELRACCOONE_TWEENS
    public IEnumerator FadeOutBgmAsync(float duration)
    {
        // Remember old volume (in case it's not 1f)
        float oldVolume = bgmAudioSource.volume;

        // Fade out using tween
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
