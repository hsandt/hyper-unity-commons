using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using HyperUnityCommons;

/// Base class for Setting Label widgets
/// SEO: (all subclasses) after SettingsManager
public abstract class BaseSettingLabel : Selectable
{
    [Header("Scene references (Base Setting Label)")]

    [Tooltip("Label text widget that displays the setting name")]
    public TextMeshProUGUI labelTextWidget;


    /// Return setting data, for this setting label, using sub-class-specific field
    public abstract BaseSettingData GetSettingData();

    /// Set setting data for this setting label, using sub-class-specific field
    /// This must be called before Init, unless setting data was set in the inspector
    /// (for setting labels already setup in the scene)
    public abstract void SetSettingData(BaseSettingData settingData);

    /// Initialize content for setting data: set label text and call OnInit
    /// Must be called by outside code e.g. in SettingsMenu
    public void Init()
    {
        DebugUtil.AssertFormat(labelTextWidget != null, this,
            "[BaseSettingLabel] No labelTextWidget found on {0}.", this);

        BaseSettingData settingData = GetSettingData();
        if (settingData != null)
        {
            if (labelTextWidget != null)
            {
                labelTextWidget.text = settingData.settingName;
            }

            OnInit();
        }
        else
        {
            DebugUtil.LogErrorFormat("[BaseSettingLabel] Init: GetSettingData returned null. Setting data " +
                "should be set before calling Init (either in inspector or at runtime via SetSettingData). " +
                "Not setting label text nor calling OnInit.");
        }
    }

    /// Sub-class-specific initialization
    /// Requires setting data to be set
    protected virtual void OnInit() {}

    /// Setup view to match model i.e. current setting
    /// This must be called after Init, when the widget is shown, and only works after SettingsManager has been
    /// initialized (so basically after Awake time, at Start time or later)
    /// Must be called by outside code e.g. in SettingsMenu
    public abstract void Setup();
}
