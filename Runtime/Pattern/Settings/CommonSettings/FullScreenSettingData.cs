using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// This is a binary setting for fullscreen: on or off
    /// Note that to set exact full-screen mode, we'd need some SettingData&lt;FullScreenMode&gt; displayed
    /// via some setting UI entry showing each mode as text. But since exact FullScreenModes are only useful
    /// for specific PC platforms, a simple flag is enough for most use cases.
    [CreateAssetMenu(fileName = "FullScreenSettingData", menuName = "Settings/FullScreen Setting Data")]
    public class FullScreenSettingData : BoolSettingData, IEngineSetting<bool>
    {
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
