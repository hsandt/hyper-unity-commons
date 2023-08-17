using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for setting data with continuous choices
    /// Good TSettingValues are float, double
    public abstract class ContinuousSettingData<TSettingValue> : SettingData<TSettingValue>
    {
        [SerializeField, Tooltip("Minimum value allowed")]
        protected TSettingValue rangeMin;

        [SerializeField, Tooltip("Maximum value allowed")]
        protected TSettingValue rangeMax;


        /* SettingData<TSettingValue> */

        public override bool IsValueValid(TSettingValue value)
        {
            var defaultComparer = Comparer<TSettingValue>.Default;
            return defaultComparer.Compare(rangeMin, value) <= 0 &&
                defaultComparer.Compare(value, rangeMax) <= 0;
        }
    }
}
