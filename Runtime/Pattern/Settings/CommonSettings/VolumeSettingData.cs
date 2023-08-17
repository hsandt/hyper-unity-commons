using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace HyperUnityCommons
{
    [CreateAssetMenu(fileName = "VolumeSettingData", menuName = "Settings/Volume Setting Data", order = 1)]
    public class VolumeSettingData : ContinuousSettingData<float>, IEngineSetting<float>
    {
        [Header("Audio asset references")]

        [Tooltip("Audio mixer responsible for the associated volume")]
        public AudioMixer audioMixer;

        [Tooltip("Name of Exposed Parameter on Audio Mixer that controls the associated volume")]
        public string audioParameterName;


        private void Reset()
        {
            // For volume, the standard range is -80dB to 0dB
            rangeMin = -80f;
            rangeMax = 0f;
        }


        /* BaseSettingData */

        public override void AssertIsValid()
        {
            base.AssertIsValid();

            Debug.AssertFormat(audioMixer, this,
                "[VolumeSettingData] Audio Mixer not set on Volume Setting Data {0}", this);
            Debug.AssertFormat(!string.IsNullOrEmpty(audioParameterName), this,
                "[VolumeSettingData] Audio Parameter Name not set on Volume Setting Data {0}", this);
        }


        /* SettingData<float> */

        public override float GetFallbackValueFrom(float referenceValue)
        {
            // Return closest value in valid range
            return Mathf.Clamp(referenceValue, rangeMin, rangeMax);
        }

        public override float StoredToRepresentedValue(float storedValue)
        {
            return MathUtil.VolumeDbToFactor(storedValue);
        }

        public override float RepresentedToStoredValue(float readableValue)
        {
            return MathUtil.VolumeFactorToDb(readableValue);
        }


        /* IEngineSetting<float> */

        public void SetValue(float storedValue)
        {
            // Standard clamping for volume safety (assuming rangeMax is 0 db or less)
            float volume = Mathf.Clamp(storedValue, rangeMin, rangeMax);

            audioMixer.SetFloat(audioParameterName, volume);
        }

        public float GetValue()
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
    }
}
