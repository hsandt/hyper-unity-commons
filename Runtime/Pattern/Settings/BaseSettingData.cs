using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HyperUnityCommons
{
    /// Top-most base class for all setting data
    /// It has no generic type so we can use it as container type, allowing to store all types of SettingData together
    /// Therefore it also has only a few members, the ones that are not related to the stored type
    /// All setting data classes should subclass (Continuous|Discrete)SettingData&lt;TSettingValue&gt;
    /// and implement either IEngineSetting&lt;TSettingValue&gt; or ICustomSetting&lt;TSettingValue&gt;
    public abstract class BaseSettingData : ScriptableObject
    {
        [Tooltip("Name of the setting. When not using localization, this will be used directly as setting label.")]
        public string settingName;

        [Tooltip("Key used to store and read value in Player Preferences (if ignorePreferences is false). " +
            "Compounded settings like Resolution can use this as a base key, appending suffixes to it for each " +
            "setting component.")]
        public string playerPrefKey;

        [Tooltip("If true, do not load/save preferences at all. This can be checked for settings that we don't want " +
            "to save, or exceptionally for engine settings already saved by engine, like resolution (although we " +
            "recommend using the custom settings preferences for more flexibility, such as saving Refresh Rate too)")]
        public bool ignorePreferences = false;

        [Tooltip("Prefab used to represent the setting entry in the settings menu")]
        public GameObject viewPrefab;


        /// Assert that all parameters have been set properly
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public virtual void AssertIsValid()
        {
            Debug.AssertFormat(viewPrefab != null, this, "[BaseSettingData] viewPrefab is not set on {0}", this);
        }
    }
}
