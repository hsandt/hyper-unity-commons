using System.Collections;
using System.Collections.Generic;
using HyperUnityCommons;
using UnityEngine;
using TMPro;

/// Component to place on label associated to a setting arrow choice, that sets text directly on value label
/// (as opposed to setting localized key for localized text)
/// Requires: AppManager, SettingsManager
/// SEO: (all subclasses) after SettingsManager
public abstract class SettingArrowChoiceLabelSimpleText<TSettingValue> : SettingArrowChoiceLabel<TSettingValue>
{
    [Header("Scene references")]

    [Tooltip("Value text widget to update when choice index changes")]
    public TextMeshProUGUI valueTextWidget;


    protected override void OnInit()
    {
        base.OnInit();

        DebugUtil.AssertFormat(valueTextWidget != null, this,
            "[SettingArrowChoiceLabelSimpleText] No valueTextWidget found on {0}.",
            this);
    }

    protected override void UpdateText(string currentChoiceName)
    {
        valueTextWidget.text = currentChoiceName;
    }
}
