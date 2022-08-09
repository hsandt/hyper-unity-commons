using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CommonsHelper
{
    [CreateAssetMenu(fileName = "BgmVolumeSettingData", menuName = "Settings/Bgm Volume Setting Data", order = 1)]
    public class BgmVolumeSettingData : SettingData<float>
    {
        /* Audio constants */

        // -40 dB of original (max) volume is a good value for a low volume that is still audible
        // When we want to mute volume, we use -80 dB instead
        const float minAudibleVolume = -40;


        [Header("Audio asset references")]

        [Tooltip("Audio mixer used by the game. It should have the following Exposed Parameters:\n" +
            "- BGM Volume: Volume of BGM Group\n" +
            "- SFX Volume: Volume of SFX Group")]
        public AudioMixer audioMixer;


        public override void Init()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.AssertFormat(audioMixer, this,
                "[BgmVolumeSettingData] Audio Mixer not set on Bgm Volume Setting Data {0}", this);
            #endif
        }

        public override void OnSetValue(float storedValue)
        {
            // Volume safety to never go beyond 0 db
            float volume = Mathf.Clamp(storedValue, -80f, 0f);

            audioMixer.SetFloat("BGM Volume", volume);
        }

        public override float StoredToReadableValue(float storedValue)
        {
            return MathUtil.VolumeDbToFactor(storedValue);
        }

        public override float ReadableToStoredValue(float readableValue)
        {
            return MathUtil.VolumeFactorToDb(readableValue);
        }
    }
}
