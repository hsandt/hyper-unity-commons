using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

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