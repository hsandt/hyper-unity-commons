using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Custom boolean setting data
    /// This is a concrete class as it doesn't need any specific behaviour override, since BoolSettingData already
    /// overrides methods with very simple behavior that will work for any bool setting. You can just create an SO
    /// instance in your project and fill the fields to start using it, since custom settings are auto-managed.
    [CreateAssetMenu(fileName = "CustomBoolSettingData", menuName = "Settings/Custom Bool Setting Data")]
    public class CustomBoolSettingData : BoolSettingData, ICustomSetting<bool>
    {
        [Tooltip("Default value")]
        public bool defaultValue = false;


        /* ICustomSetting<bool> */

        public bool GetDefaultValue()
        {
            return defaultValue;
        }
    }
}
