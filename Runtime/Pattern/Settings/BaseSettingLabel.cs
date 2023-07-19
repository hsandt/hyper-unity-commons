using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Base class for Setting Label widgets
/// SEO: (all subclasses) before SettingsManager
public abstract class BaseSettingLabel : Selectable
{
    /// Setup view to match model i.e. current setting
    /// This must be called when the widget is shown, and only works after SettingsManager as been initialized
    /// (so basically after Awake time, at Start time or later)
    public abstract void Setup();
}
