using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Interface meant for sub-classes of SettingData&lt;TSettingValue&gt;
    /// that represent a custom game setting
    public interface ICustomSetting<TSettingValue>
    {
        /// Return a default value for this setting
        /// We must verify IsValueValid(GetDefaultValue())
        public abstract TSettingValue GetDefaultValue();
    }
}
