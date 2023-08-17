using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperUnityCommons
{
    // REFACTOR: consider creating a DynamicIntegerRangeSettingData where you can subclass methods that return
    // a range, so you can benefit from methods predefined as in IntegerRangeSettingData, but with rangeMin/rangeMax
    // computed from context
    [CreateAssetMenu(fileName = "GraphicsQualitySettingData", menuName = "Settings/Graphics Quality Setting Data")]
    public class GraphicsQualitySettingData : DiscreteSettingData<int>, IEngineSetting<int>
    {
        /* SettingData<bool> */

        public override bool IsValueValid(int value)
        {
            // Base implementation works in O(QualitySettings.names.Length), but this one is more performant
            int qualityLevelCount = QualitySettings.names.Length;
            return 0 <= value && value < qualityLevelCount;
        }

        public override int GetFallbackValueFrom(int referenceValue)
        {
            // Return closest value in quality level range
            int qualityLevelCount = QualitySettings.names.Length;
            return Mathf.Clamp(referenceValue, 0, qualityLevelCount - 1);
        }

        public override string RepresentedValueToText(int representedValue)
        {
            return QualitySettings.names[representedValue];
        }

        public override List<int> GetAvailableValues()
        {
            int qualityLevelCount = QualitySettings.names.Length;
            return Enumerable.Range(0, qualityLevelCount).ToList();
        }


        /* IEngineSetting<int> */

        public int GetValue()
        {
            // This is called during SettingsManager.Init, so default value is the one set in engine on start
            return QualitySettings.GetQualityLevel();
        }

        public void SetValue(int storedValue)
        {
            QualitySettings.SetQualityLevel(storedValue);
        }
    }
}
