using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonsHelper
{
    /// Base class for setting data
    /// Define a child class for each game setting.
    /// The child class must store typed data for default value as well as non-serialized runtime values,
    /// and define behaviour on get/set via overridden methods.
    public class SettingData : ScriptableObject
    {
    }
}
