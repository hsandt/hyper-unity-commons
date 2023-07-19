using System.Collections;
using System.Collections.Generic;
using HyperUnityCommons;
using UnityEngine;
using TMPro;

/// Component to place on label associated to a setting arrow choice, that sets text directly on value label
/// (as opposed to setting localized key for localized text)
/// Requires: AppManager, SettingsManager
/// SEO: (all subclasses) after SettingsManager
public abstract class SettingArrowChoiceLabelSimpleText<T> : SettingArrowChoiceLabel<T>
{
    [Header("Scene references")]

    [Tooltip("Value text widget to update when choice index changes")]
    public TextMeshProUGUI valueTextWidget;


    // Bug IN-10813: when inheriting from Selectable, in the editor:
    // - Awake is not called on Play, only on Stop
    // - Start sometimes work, but is not reliable
    // - OnEnable is called when expected, but also on Application Quit to setup values in the editor
    // => The most reliable is to check and initialize members in OnEnable, after base call, but only if application is
    // not quitting; and also do any required symmetrical operations like even unregistration in OnDisable.
    protected override void OnEnable()
    {
        // Better to assert before base call, because base call eventually calls Setup which will use verified members
        if (AppManager.IsNotQuitting())
        {
            DebugUtil.AssertFormat(valueTextWidget != null, this,
                "[SettingArrowChoiceLabelSimpleText] No valueTextWidget found on {0}.",
                this);
        }

        base.OnEnable();
    }

    protected override void UpdateText(string currentChoiceName)
    {
        valueTextWidget.text = currentChoiceName;
    }
}
