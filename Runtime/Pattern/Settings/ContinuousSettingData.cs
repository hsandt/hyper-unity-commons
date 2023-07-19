using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Base class for setting data with continuous choices
    /// Good TSettingValues are float, double
    public abstract class ContinuousSettingData<TSettingValue> : SettingData<TSettingValue>
    {
    }
}
