using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace CommonsPattern.Editor
{
    [CustomEditor(typeof(SettingGaugeLabel))]
    [CanEditMultipleObjects]
    public class SettingGaugeLabelEditor : SelectableEditor
    {
    }
}
