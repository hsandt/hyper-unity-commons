using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for boolean setting data (subclass must implement either IEngineSetting&lt;bool&gt; or
    /// ICustomSetting&lt;bool&gt;)
    public abstract class BoolSettingData : DiscreteSettingData<bool>
    {
        [Tooltip("Text to display to represent value when true")]
        public string trueText = "On";

        [Tooltip("Text to display to represent value when false")]
        public string falseText = "Off";


        /* SettingData<bool> */

        public override bool IsValueValid(bool value)
        {
            // Base implementation works with iteration, but we know both bool values are valid, so just return true
            return true;
        }

        public override bool GetFallbackValueFrom(bool referenceValue)
        {
            // IsValueValid always returns true, so what we return here doesn't matter
            return true;
        }

        public override string RepresentedValueToText(bool representedValue)
        {
            return representedValue ? trueText : falseText;
        }


        /* DiscreteSettingData<int> override */

        public override List<bool> GetAvailableValues()
        {
            return new List<bool>{true, false};
        }
    }
}
