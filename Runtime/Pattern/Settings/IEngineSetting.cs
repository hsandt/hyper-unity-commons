using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Interface meant for sub-classes of SettingData&lt;TSettingValue&gt;
    /// that represent an engine-driven setting
    public interface IEngineSetting<TSettingValue>
    {
        /// Return engine setting value for this setting data
        /// We must always verify IsValueValid(GetValue())
        TSettingValue GetValue();

        /// Set value for this setting data in engine
        /// The passed value must verify IsValueValid(storedValue)
        void SetValue(TSettingValue storedValue);
    }
}
