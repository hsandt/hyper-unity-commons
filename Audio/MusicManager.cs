using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;
#if NL_ELRACCOONE_TWEENS
using ElRaccoone.Tweens;
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


    protected override void Init()
    {
        base.Init();

        #if UNITY_EDITOR
        Debug.Assert(bgmAudioSource != null, "[InGameManager] No Bgm Audio Source set on Music Manager", this);
        Debug.Assert(stingerAudioSource != null, "[InGameManager] No Stinger Audio Source set on Music Manager", this);
        #endif
    }

    public void PlayBgm(AudioClip bgm)
    {
        if (bgmAudioSource.clip != bgm)
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

    #if NL_ELRACCOONE_TWEENS
    public void FadeOutBgm(float duration)
    {
        bgmAudioSource.TweenAudioSourceVolume(0f, duration);
    }
    #endif

    public void PlayStinger(AudioClip stinger)
    {
        if (stingerAudioSource.clip != stinger)
        {
            // Note that stingerAudioSource.loop should be false on MusicManager_Base prefab
            stingerAudioSource.clip = stinger;
            stingerAudioSource.Play();
        }
    }
}