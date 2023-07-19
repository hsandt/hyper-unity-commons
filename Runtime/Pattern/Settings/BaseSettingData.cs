using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Top-most base class for all setting data
    /// It has no generic type so we can use it as container type, allowing to store all types of SettingData together
    /// Therefore it also has only a few members, the ones that are not related to the stored type
    /// All setting data classes should subclass SettingData&lt;TSettingValue&gt;
    public abstract class BaseSettingData : ScriptableObject
    {
        [Tooltip("Name of the setting. When not using localization, this will be used directly as setting label.")]
        public string settingName;

        [Tooltip("Key used to store value in Player Preferences. Compounded settings like Resolution can use this as " +
            "a base key, appending suffixes to it for each setting component.")]
        public string playerPrefKey;

        [Tooltip("If true, any UI displaying the setting should check the engine value with " +
            "SettingData<TSettingValue>.GetDefaultValueOnStart (this only makes sense if GetDefaultValueOnStart " +
            "returns an engine value indeed), and immediately update the setting to be set to this value. This is " +
            "useful for settings that can be modified outside the Settings menu (e.g. a quick toggle fullscreen " +
            "shortcut could change resolution at any time), so they still display the current value. " +
            "Note that this is only used because it is hard for external code to access SettingsManager and find " +
            "which setting could be linked to a given engine value they are changing (possible, but would need " +
            "iteration or clever registration).")]
        public bool alwaysCheckEngineValue = false;

        /// Assert that all parameters have been set properly
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public virtual void AssertIsValid() {}
    }
}
