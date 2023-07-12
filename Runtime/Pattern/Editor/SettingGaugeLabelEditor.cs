using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace HyperUnityCommons.Editor
{
    [CustomEditor(typeof(SettingGaugeLabel), isFallback = true)]
    [CanEditMultipleObjects]
    public class SettingGaugeLabelEditor : SelectableEditor
    {
    }
}
