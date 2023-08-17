using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    // This is a binary setting for fullscreen: on or off
    // Note that to set exact full-screen mode, we'd need some SettingData<FullScreenMode> displayed
    // via some setting UI entry showing each mode as text. But since exact FullScreenModes are only useful
    // for specific PC platforms, a simple flag is enough for most use cases.
    [CreateAssetMenu(fileName = "FullScreenSettingData", menuName = "Settings/FullScreen Setting Data")]
    public class FullScreenSettingData : DiscreteSettingData<bool>, IEngineSetting<bool>
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


        /* IEngineSetting<bool> */

        public bool GetValue()
        {
            return Screen.fullScreen;
        }

        public void SetValue(bool storedValue)
        {
            #if UNITY_EDITOR
            DebugUtil.LogFormat("[FullScreenSettingData] Set Screen.fullScreen = {0} (ignored in Editor)", storedValue);
            #endif

            // Preserve resolution (set via another setting), and just set fullScreen flag
            Screen.fullScreen = storedValue;
        }
    }
}
