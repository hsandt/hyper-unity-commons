using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for setting data with discrete choices
    /// Good TSettingValues are bool, int, Resolution, string
    /// Prefer specialized sub-classes for the following types:
    /// - bool => BoolSettingData
    /// - int for custom setting => IntegerRangeSettingData
    public abstract class DiscreteSettingData<TSettingValue> : SettingData<TSettingValue>
    {
        public override bool IsValueValid(TSettingValue value)
        {
            // This default implementation will check that value is among available values
            // While this works for all discrete settings, you may still override this in subclasses to define
            // a more performant implementation (e.g. when dealing with a numerical range, compare value to boundaries)
            return GetAvailableValues().Contains(value);
        }

        /// Return list of available choice values (stored format)
        /// (we assume it doesn't change during the game)
        public abstract List<TSettingValue> GetAvailableValues();
    }
}
