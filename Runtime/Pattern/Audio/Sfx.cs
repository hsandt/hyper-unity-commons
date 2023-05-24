using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

/// SFX component for objects able to call Release by themselves (e.g. animated sprite with animation event at the end)
public class Sfx : MonoBehaviour, IPooledObject
{
    /* Sibling components */

    private AudioSource m_AudioSource;
    public AudioSource AudioSource => m_AudioSource;


    private void Awake()
    {
        m_AudioSource = this.GetComponentOrFail<AudioSource>();
    }


    /* IPooledObject interface */

    public void Acquire() {}

    public bool IsInUse()
    {
        return m_AudioSource.isPlaying;
    }

    public void Release()
    {
        m_AudioSource.Stop();
    }


    /* Own methods */

    /// Play defined clip as one-shot
    /// This will not set the clip, so SfxPoolManager "Same Clip Stack Volume Modifier" system will not detect it
    public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        m_AudioSource.PlayOneShot(clip, volumeScale);
    }

    /// Play defined clip, storing clip properly
    /// This is required to use SfxPoolManager "Same Clip Stack Volume Modifier" system
    public void PlayStoringClip(AudioClip clip, float volumeScale = 1f)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.volume = volumeScale;
        m_AudioSource.Play();
    }
}
