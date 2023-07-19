using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if COM_E7_INTROLOOP
using E7.Introloop;
#endif

namespace HyperUnityCommons
{
    /// <summary>
    /// Wrapper for Introloop (if installed) and native audio assets, for compatibility with projects
    /// with and without Introloop plugin
    /// </summary>
    [CreateAssetMenu(fileName = "AudioAssetWrapper", menuName = "Audio/Audio Asset Wrapper")]
    public class AudioAssetWrapper : ScriptableObject
    {
        #if COM_E7_INTROLOOP
        [Tooltip("Introloop Audio, played in priority if present")]
        public IntroloopAudio introloopAudio;
        #endif

        [Tooltip("Native Audio Clip, played if no plugin-specific audio asset is set")]
        public AudioClip nativeAudioClip;
    }
}
