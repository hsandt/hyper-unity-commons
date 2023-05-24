using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonsHelper
{
    /// Base class for setting data
    /// Define a child class for each game setting.
    /// The child class must store typed data for default value as well as non-serialized runtime values,
    /// and define behaviour on get/set via overridden methods.
    public abstract class SettingData<T> : ScriptableObject
    {
        [Tooltip("Name of the setting. When not using localization, this will be used directly as setting label.")]
        public string settingName;

        [Tooltip("Key used to store value in Player Preferences")]
        public string playerPrefKey;

        [Tooltip("Default value set when no Player Preference is found on game start")]
        public T defaultValue;

        /// Initialization method
        /// Mostly for checking that all parameters have been set properly
        public virtual void Init() {}

        /// Called when this setting value is set
        /// Default implementation is empty, as some settings do nothing on value set, but will be queried later
        /// by code that needs it. Others (e.g. BGM volume) need to change project values immediately when changed.
        public virtual void OnSetValue(T storedValue) {}

        /// Convert stored value (in the settings and player preferences)
        /// into value as read on value field as either text or gauge.
        /// For gauge, we pick the convention to always return a ratio (0: empty, 1: filled).
        /// Make sure that ReadableToStoredValue is implemented as the reciprocal (at least in valid ranges of values)
        /// Default implementation: use stored value as readable value directly
        public virtual T StoredToReadableValue(T storedValue)
        {
            return storedValue;
        }

        /// Convert readable value (read on value field as either text or gauge)
        /// into value as stored in the settings and player preferences
        /// For gauge, we pick the convention to always take a ratio (0: empty, 1: filled).
        /// Make sure that StoredToReadableValue is implemented as the reciprocal (at least in valid ranges of values)
        /// Default implementation: use readable value as stored value directly
        public virtual T ReadableToStoredValue(T readableValue)
        {
            return readableValue;
        }
    }
}
