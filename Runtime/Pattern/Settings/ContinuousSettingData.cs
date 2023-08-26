using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for setting data with continuous choices
    /// Good TSettingValues are float, double
    public abstract class ContinuousSettingData<TSettingValue> : SettingData<TSettingValue>
    {
        [Tooltip("Minimum value allowed")]
        public TSettingValue rangeMin;

        [Tooltip("Maximum value allowed")]
        public TSettingValue rangeMax;


        /* BaseSettingData */

        public override void AssertIsValid()
        {
            base.AssertIsValid();

            Debug.AssertFormat(Comparer<TSettingValue>.Default.Compare(rangeMin, rangeMax) <= 0,
                "[ContinuousSettingData<{0}>] Invalid parameters: rangeMin ({1}) > rangeMax ({2})",
                typeof(TSettingValue), rangeMin, rangeMax);
        }


        /* SettingData<TSettingValue> */

        public override bool IsValueValid(TSettingValue value)
        {
            var defaultComparer = Comparer<TSettingValue>.Default;
            return defaultComparer.Compare(rangeMin, value) <= 0 &&
                defaultComparer.Compare(value, rangeMax) <= 0;
        }
    }
}
