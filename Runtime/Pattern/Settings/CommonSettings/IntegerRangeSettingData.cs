using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for settings whose value evolves in a fixed integer range
    /// This is useful to define custom integer settings such as a difficulty level (override RepresentedValueToText
    /// to display the level names).
    /// For dynamic integer settings that need more context to define their range, such as Graphics Quality,
    /// prefer subclassing DiscreteSettingData&lt;int&gt; and overriding GetAvailableValues yourself.
    /// See GraphicsQualitySettingData as an example.
    [CreateAssetMenu(fileName = "IntegerRangeSettingData", menuName = "Settings/Integer Range Setting Data")]
    public abstract class IntegerRangeSettingData : DiscreteSettingData<int>
    {
        [SerializeField, Tooltip("Initial value used when no player preference is set")]
        private int defaultValue;

        [SerializeField, Tooltip("Minimum integer range")]
        private int rangeMin;

        [SerializeField, Tooltip("Maximum integer range")]
        private int rangeMax;


        /* BaseSettingData */

        public override void AssertIsValid()
        {
            DebugUtil.AssertFormat(rangeMin <= rangeMax,
                "[IntegerRangeSettingData] Invalid parameters: rangeMin ({0}) > rangeMax ({1})",
                rangeMin, rangeMax);
        }


        /* SettingData<int> */

        public override bool IsValueValid(int value)
        {
            // Base implementation works in O(range count), but this one is more performant
            // Note that rangeMax is inclusive
            return rangeMin <= value && value <= rangeMax;
        }

        public override int GetDefaultValueOnStart()
        {
            return defaultValue;
        }

        public override int GetFallbackValueFrom(int referenceValue)
        {
            // Return closest value in range
            return Mathf.Clamp(referenceValue, rangeMin, rangeMax);
        }


        /* DiscreteSettingData<int> override */

        public override List<int> GetAvailableValues()
        {
            return Enumerable.Range(rangeMin, rangeMax - rangeMin + 1).ToList();
        }
    }
}
