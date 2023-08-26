using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperUnityCommons
{
    /// SO class for settings whose value evolves in a fixed float range
    /// This is useful to define custom float settings such as input sensitivity
    /// For dynamic float settings that need more complex behaviour, such as Volume Setting,
    /// prefer subclassing ContinuousSettingData&lt;float&gt; and implement GetValue/SetValue yourself.
    /// See VolumeSettingData as an example.
    [CreateAssetMenu(fileName = "CustomFloatSettingData", menuName = "Settings/Custom Float Setting Data")]
    public class CustomFloatSettingData : ContinuousSettingData<float>, ICustomSetting<float>
    {
        [Tooltip("Initial value used when no player preference is set")]
        public float defaultValue;


        /* SettingData<float> */

        public override float GetFallbackValueFrom(float referenceValue)
        {
            // Return closest value in range
            return Mathf.Clamp(referenceValue, rangeMin, rangeMax);
        }


        /* ICustomSetting<float> */

        public float GetDefaultValue()
        {
            return defaultValue;
        }
    }
}
