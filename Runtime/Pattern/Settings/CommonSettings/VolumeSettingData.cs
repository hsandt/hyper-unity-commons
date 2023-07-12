using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace HyperUnityCommons
{
    [CreateAssetMenu(fileName = "VolumeSettingData", menuName = "Settings/Volume Setting Data", order = 1)]
    public class VolumeSettingData : SettingData<float>
    {
        [Header("Audio asset references")]

        [Tooltip("Audio mixer responsible for the associated volume")]
        public AudioMixer audioMixer;

        [Tooltip("Name of Exposed Parameter on Audio Mixer that controls the associated volume")]
        public string audioParameterName;

        public override void AssertIsValid()
        {
            Debug.AssertFormat(audioMixer, this,
                "[VolumeSettingData] Audio Mixer not set on Volume Setting Data {0}", this);
            Debug.AssertFormat(!string.IsNullOrEmpty(audioParameterName), this,
                "[VolumeSettingData] Audio Parameter Name not set on Volume Setting Data {0}", this);
        }

        public override void OnSetValue(float storedValue)
        {
            // Volume safety to never go beyond 0 db
            float volume = Mathf.Clamp(storedValue, -80f, 0f);

            audioMixer.SetFloat(audioParameterName, volume);
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
