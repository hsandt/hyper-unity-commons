using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for setting data
    /// Define a child class for each game setting.
    /// The child class must store typed data for default value as well as non-serialized runtime values,
    /// and define behaviour on get/set via overridden methods.
    public abstract class SettingData<TSettingValue> : BaseSettingData
    {
        /// Return true is the passed value is valid
        /// This is used to detect if values loaded from preferences can actually be used,
        /// or if we should fall back to GetFallbackInitialValue()
        public abstract bool IsValueValid(TSettingValue value);

        /// Return a valid default value for the setting
        /// When initially loading settings, if no matching Player Preference has been found, this is used to find a
        /// good initial value.
        /// a. Custom settings such as gameplay difficulty level should return a custom value (e.g. some defaultValue field
        /// defined on SettingData subclass)
        /// b. Engine settings like Graphics Quality or Audio Volume should return the current value, so this will use
        /// the value set in the editor, or the best value auto-detected for player hardware.
        public abstract TSettingValue GetDefaultValueOnStart();

        /// Return a valid fallback value from an existing setting (loaded from player prefs, or read in current
        /// settings dictionary)
        /// When loading player preferences, or displaying a list of choices (e.g. in SettingArrowChoiceLabel), and we
        /// realize that this value is not valid, we use this to find a good fallback value.
        /// We recommend that implementations pick a value the closest to the passed value, but a standard default also
        /// works.
        public abstract TSettingValue GetFallbackValueFrom(TSettingValue referenceValue);

        /// Called when this setting value is set
        /// Default implementation is empty, as some settings do nothing on value set, but will be queried later
        /// by code that needs it. Others (e.g. BGM volume) need to change project values immediately when changed.
        public virtual void OnSetValue(TSettingValue storedValue) {}

        /// Convert stored value (in the settings and player preferences)
        /// into value as represented on value field as either text (via RepresentedValueToText) or gauge.
        /// For gauge, we pick the convention to always return a ratio (0: empty, 1: filled).
        /// Make sure that ReadableToStoredValue is implemented as the reciprocal (at least in valid ranges of values)
        /// Default implementation: use stored value as represented value directly
        public virtual TSettingValue StoredToRepresentedValue(TSettingValue storedValue)
        {
            return storedValue;
        }

        /// Convert represented value (read on value field as either text or gauge)
        /// into value as stored in the settings and player preferences
        /// For gauge, we pick the convention to always take a ratio (0: empty, 1: filled).
        /// Make sure that StoredToReadableValue is implemented as the reciprocal (at least in valid ranges of values)
        /// Default implementation: use represented value as stored value directly
        public virtual TSettingValue RepresentedToStoredValue(TSettingValue representedValue)
        {
            return representedValue;
        }

        /// Convert represented value (read on value field as either text or gauge)
        /// into text to be displayed on the UI (alone, or along some gauge).
        /// Default implementation: just convert to string
        public virtual string RepresentedValueToText(TSettingValue representedValue)
        {
            return representedValue.ToString();
        }

        /// Convert stored value (in the settings and player preferences)
        /// into represented value as text to be displayed on the UI (alone, or along some gauge).
        /// Default implementation: just convert to string
        public string StoredValueToRepresentedText(TSettingValue storedValue)
        {
            TSettingValue representedValue = StoredToRepresentedValue(storedValue);
            return RepresentedValueToText(representedValue);
        }
    }
}
