using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Dictionary that maps a SettingData<T> to a setting value of type T
    /// Unity objects are hashed via their instance ID, so in fact we could use int as key type,
    /// but using SettingData<T> ensures that key and value types are consistent.
    public class SettingDictionary<T> : Dictionary<SettingData<T>, T>
    {
    }
}
