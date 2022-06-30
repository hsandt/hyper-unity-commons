using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

/// Music Manager
/// Good to play simple looping BGM, but doesn't support intro-loop.
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


    public void PlayBgm(AudioClip bgm)
    {
        if (bgmAudioSource.clip != bgm)
        {
            bgmAudioSource.clip = bgm;
            bgmAudioSource.Play();
        }
    }
}