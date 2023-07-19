using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace HyperUnityCommons
{
    [CreateAssetMenu(fileName = "VolumeSettingData", menuName = "Settings/Volume Setting Data", order = 1)]
    public class VolumeSettingData : ContinuousSettingData<float>
    {
        [Header("Audio asset references")]

        [Tooltip("Audio mixer responsible for the associated volume")]
        public AudioMixer audioMixer;

        [Tooltip("Name of Exposed Parameter on Audio Mixer that controls the associated volume")]
        public string audioParameterName;


        /* BaseSettingData */

        public override void AssertIsValid()
        {
            Debug.AssertFormat(audioMixer, this,
                "[VolumeSettingData] Audio Mixer not set on Volume Setting Data {0}", this);
            Debug.AssertFormat(!string.IsNullOrEmpty(audioParameterName), this,
                "[VolumeSettingData] Audio Parameter Name not set on Volume Setting Data {0}", this);
        }


        /* SettingData<float> */

        public override bool IsValueValid(float value)
        {
            // Use standard clamping for safety, although technically value can be above 0dB
            return -80f <= value && value <= 0f;
        }

        public override float GetDefaultValueOnStart()
        {
            // This is called during SettingsManager.Init, so default value is the one set in engine on start
            if (audioMixer.GetFloat(audioParameterName, out float volume))
            {
                return volume;
            }

            DebugUtil.LogErrorFormat(this,
                "[VolumeSettingData] GetDefaultValue: could not get float on audio mixer {0} for parameter '{1}', " +
                "there seem to be no exposed parameter with that name",
                audioMixer, audioParameterName);
            return 0f;
        }

        public override float GetFallbackValueFrom(float referenceValue)
        {
            // Return closest value in valid range
            return Mathf.Clamp(referenceValue, -80f, 0f);
        }

        public override void OnSetValue(float storedValue)
        {
            // Standard clamping for volume safety to never go beyond 0 db
            float volume = Mathf.Clamp(storedValue, -80f, 0f);

            audioMixer.SetFloat(audioParameterName, volume);
        }

        public override float StoredToRepresentedValue(float storedValue)
        {
            return MathUtil.VolumeDbToFactor(storedValue);
        }

        public override float RepresentedToStoredValue(float readableValue)
        {
            return MathUtil.VolumeFactorToDb(readableValue);
        }
    }
}
