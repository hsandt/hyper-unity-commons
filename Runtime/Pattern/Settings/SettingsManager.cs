// #define DEBUG_SETTINGS_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using HyperUnityCommons;
using UnityEngine.Serialization;

/// Settings Manager
/// A manager that handles two types of settings:
/// 1. Engine settings: they must inherit from (Continuous|Discrete)SettingData&lt;TSettingValue&gt; and implement IEngineSetting
///    They are not stored in the SettingsManager. Instead, we use IEngineSetting.GetValue/SetValue to directly get/set
///    values from engine
/// 2. Custom settings: they must inherit from (Continuous|Discrete)SettingData&lt;TSettingValue&gt; (but not implement
///    IEngineSetting). They are stored in the SettingsManager dictionary.
/// We recommend creating it and making it DontDestroyOnLoad via some PersistentManagersGenerator (specific to project)
/// SEO: (possibly via PersistentManagersGenerator) before any setting widget accessing settings
public class SettingsManager : SingletonManager<SettingsManager>
{
	[Tooltip("Asset containing list of settings of any type to show in order")]
	public SettingDataList settingDataList;

	[Tooltip("OLD field kept so existing projects can transition to the new settingDataList")]
	[FormerlySerializedAs("settings")]
	[FormerlySerializedAs("settingDataList")]
	public List<BaseSettingData> OLD_settingDataList;


	/* State */

	/// Dictionary of engine setting default value, using boxing, per engine setting data
	/// It is filled on start from actual engine values set in project, before loading preferences
	/// (default resolution may vary as Unity remembers the last resolution on its own)
	private readonly Dictionary<BaseSettingData, object> defaultEngineSettingValueDictionary = new();

	/// Dictionary of current custom setting values of any type, using boxing, per custom setting data
	private readonly Dictionary<BaseSettingData, object> currentCustomSettingValueDictionary = new();


	#region Initialization

	protected override void Init()
	{
		if (settingDataList == null)
		{
			DebugUtil.LogErrorFormat(this, "[SettingsManager] settingDataList not set on {0}", this);
			return;
		}

		settingDataList.AssertIsValid();

		foreach (var setting in settingDataList.entries)
		{
			setting.AssertIsValid();
		}
	}

	private void Start()
	{
		// Make sure to initialize all settings on Start so everything has been setup and ready to react to your changes
		// In practice, only Audio settings needed this, as AudioMixer is not ready at Awake time, and setting an
		// AudioMixer parameter will silently fail.
		// Thread: https://forum.unity.com/threads/audiomixer-setfloat-doesnt-work-on-awake.323880/
		// Issue: https://issuetracker.unity3d.com/issues/audiomixer-dot-setfloat-ignores-new-value-when-it-is-changed-and-saved-during-previous-play-scene-session
		// closed as "By design"
		InitializeAllSettings();
	}

	private void InitializeAllSettings()
	{
		foreach (var setting in settingDataList.entries)
		{
			InitializeSetting(setting);
		}
	}


	/// Initialize setting depending on its value type, retrieving player preference value if possible,
	/// else using default value
	private void InitializeSetting(BaseSettingData settingData)
	{
		// Type check is a little brutal, but it works because we know that all setting data classes are in fact
		// deriving from SettingData<TSettingValue>.
		// The alternative is to use polymorphism, but that means we should define:
		// BoolDiscreteSettingData, IntDiscreteSettingData, FloatContinuousSettingData that would implement some
		// InitializeSetting method accordingly, and make sure to subclass them (and not
		// SettingData<bool/int/float>) directly to define the final setting data classes.
		switch (settingData)
		{
			case SettingData<bool> boolSettingData:
				InitializeSimpleSetting(boolSettingData, GetBoolSettingFromPreferences);
				break;
			case SettingData<int> intSettingData:
				InitializeSimpleSetting(intSettingData, GetIntSettingFromPreferences);
				break;
			case SettingData<float> floatSettingData:
				InitializeSimpleSetting(floatSettingData, GetFloatSettingFromPreferences);
				break;
			case SettingData<Resolution> resolutionSettingData:
				InitializeResolutionSetting(resolutionSettingData);
				break;
			default:
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] InitializeSetting: {0} has unsupported BaseSettingData subclass {1}, " +
					"as it doesn't have a TSettingValue of type bool, int, float nor Resolution",
					settingData, settingData.GetType());
				break;
		}
	}

	/// Initialize simple (primitive type) setting, retrieving player preference value if possible with callback
	/// specific to setting value type if possible
	/// If preference value is not valid, fallback to good close value, then default value
	/// If no preference has been saved, set value to default (do nothing for engine setting)
	/// getSettingFromPreferencesCallback should have signature:
	/// TSettingValue Method(SettingData&lt;TSettingValue&gt; settingData)
	private void InitializeSimpleSetting<TSettingValue>(SettingData<TSettingValue> settingData,
		Func<SettingData<TSettingValue>, TSettingValue> getSettingFromPreferencesCallback)
	{
		IEngineSetting<TSettingValue> engineSetting = null;
		ICustomSetting<TSettingValue> customSetting = null;

		switch (settingData)
		{
			case IEngineSetting<TSettingValue> engineSettingData:
				engineSetting = engineSettingData;
				break;
			case ICustomSetting<TSettingValue> customSettingData:
				customSetting = customSettingData;
				break;
			default:
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] InitializeSimpleSetting: settingData ({0}) has unsupported " +
					"BaseSettingData subclass {1}, as it doesn't implement IEngineSetting<> nor ICustomSetting<>. STOP.",
					settingData, settingData.GetType());
				return;
		}

		if (engineSetting != null)
		{
			// Store engine setting default value as the initial value read from engine,
			// before loading any preference
			defaultEngineSettingValueDictionary[settingData] = engineSetting.GetValue();
		}

		if (!settingData.ignorePreferences && PlayerPrefs.HasKey(settingData.playerPrefKey))
		{
			TSettingValue playerPrefStoredValue = getSettingFromPreferencesCallback(settingData);
			if (settingData.IsValueValid(playerPrefStoredValue))
			{
				// We got a valid preference value, set setting value to it
				// It comes from the preferences, so no need to save the preference again.
				SetSettingValue(settingData, playerPrefStoredValue, immediatelySavePreference: false);
			}
			else
			{
				// Invalid preference value, use fallback based on it
				TSettingValue fallbackValue = settingData.GetFallbackValueFrom(playerPrefStoredValue);

				DebugUtil.LogWarningFormat("[SettingsManager] InitializeSimpleSetting: " +
					"getSettingFromPreferencesCallback({0}) returned invalid value {1}, falling back with " +
					"GetFallbackValueFrom to {2}",
					settingData, playerPrefStoredValue, fallbackValue);

				if (!settingData.IsValueValid(fallbackValue))
				{
					if (engineSetting != null)
					{
						DebugUtil.LogErrorFormat(settingData,
							"[SettingsManager] InitializeSimpleSetting: GetFallbackValueFrom({0}) returned " +
							"invalid value {1}. {2} is an engine setting, so just do nothing to keep the default value.",
							playerPrefStoredValue, fallbackValue, engineSetting);
						// Note the return to do nothing
						return;
					}
					else  // customSetting != null
					{
						DebugUtil.LogErrorFormat(settingData,
							"[SettingsManager] InitializeSimpleSetting: GetFallbackValueFrom({0}) returned " +
							"invalid value {1}, {2} is a custom setting, so falling back a second time to " +
							"default value as ultimate resort. Make sure to fix GetFallbackValueFrom to return a " +
							"valid value.",
							playerPrefStoredValue, fallbackValue, customSetting);
						fallbackValue = customSetting.GetDefaultValue();
					}
				}

				// Set setting value to fallback value
				// Whether we want to resave the fallback value in preferences is up to design. In this case, player
				// has not actively changed setting yet, so following the same logic as when player keeps the default
				// value, we decide not to save it immediately.
				SetSettingValue(settingData, fallbackValue, immediatelySavePreference: false);
			}
		}
		else
		{
			// No preference found at all, use default value
			// Engine setting: nothing to do, default value is the one already set in engine on start
			// (note that in some cases, e.g. Resolution, it may remember last session setting on its own without
			// preferences, but if you enabled preference save from the beginning, then this should only be reached on
			// fresh start)
			// Custom setting: set default setting from implementation
			if (customSetting != null)
			{
				// Note that this will not save any preference
				// The player did not actively change setting to default, so this is what we want
				InitializeCustomSettingToDefaultValue(customSetting);
			}
		}
	}

	/// Load resolution setting from multiple player preferences (width x height @ refresh rate)
	/// If preference value is not valid, fallback to good close value, then default value
	/// If no preference has been saved, set value to default (do nothing for engine setting, as common for resolution)
	private void InitializeResolutionSetting(SettingData<Resolution> resolutionSettingData)
	{
		// Implementation is similar to LoadSimpleSettingFromPreferences, but specialized
		// for Resolution which is a compounded type

		IEngineSetting<Resolution> engineSetting = null;
		ICustomSetting<Resolution> customSetting = null;

		switch (resolutionSettingData)
		{
			case IEngineSetting<Resolution> engineSettingData:
				engineSetting = engineSettingData;
				break;
			// ! Resolution setting is very likely an IEngineSetting, but for completion we support the case of
			// ! ICustomSetting. You'll notice how your IDE may even warn you that it never happens.
			case ICustomSetting<Resolution> customSettingData:
				customSetting = customSettingData;
				break;
			default:
				DebugUtil.LogErrorFormat(resolutionSettingData,
					"[SettingsManager] InitializeResolutionSetting: settingData ({0}) has unsupported " +
					"BaseSettingData subclass {1}, as it doesn't implement IEngineSetting<> nor ICustomSetting<>. STOP.",
					resolutionSettingData, resolutionSettingData.GetType());
				return;
		}

		if (engineSetting != null)
		{
			// Store engine setting default value as the initial value read from engine,
			// before loading any preference
			defaultEngineSettingValueDictionary[resolutionSettingData] = engineSetting.GetValue();
		}

		// Preference keys are split in 3 for Resolution
		// Note that Unity also natively remembers its own resolution info:
		// - Screenmanager Resolution Width/Height
		// - Screenmanager Fullscreen mode
		// etc. but it's easier to work with our own preferences.

		string resolutionWidthPlayerPrefKey = $"{resolutionSettingData.playerPrefKey}.Width";
		string resolutionHeightPlayerPrefKey = $"{resolutionSettingData.playerPrefKey}.Height";
		string resolutionRefreshRatePlayerPrefKey = $"{resolutionSettingData.playerPrefKey}.RefreshRate";

		// The most important preferences are width and height
		// Obviously, if you work with new code, you should have saved refresh rate too, but in case you are using
		// old preferences that don't have refresh rate, we support not knowing it
		if (!resolutionSettingData.ignorePreferences && PlayerPrefs.HasKey(resolutionWidthPlayerPrefKey) && PlayerPrefs.HasKey(resolutionHeightPlayerPrefKey))
		{
			int playerPrefResolutionWidth = PlayerPrefs.GetInt(resolutionWidthPlayerPrefKey);
			int playerPrefResolutionHeight = PlayerPrefs.GetInt(resolutionHeightPlayerPrefKey);

			// To support not knowing preferred refresh rate, if preference is not present, fall back to 0
			// IsValueValid and GetFallbackValueFrom below will handle this, detecting invalid refresh rate
			// and finding a matching resolution at different refresh rate if needed
			int playerPrefResolutionRefreshRate = PlayerPrefs.HasKey(resolutionRefreshRatePlayerPrefKey)
				? PlayerPrefs.GetInt(resolutionRefreshRatePlayerPrefKey)
				: 0;

			Resolution playerPrefResolution = new Resolution
			{
				width = playerPrefResolutionWidth,
				height = playerPrefResolutionHeight,
				refreshRate = playerPrefResolutionRefreshRate
			};

			if (resolutionSettingData.IsValueValid(playerPrefResolution))
			{
				// Valid preference value, use it
				// We've just loaded this setting from preferences, but if it's an engine setting, it has not been
				// applied to engine yet (unless it is coincidentally the default), so we must call OnSetValue.
				// It comes from the preferences though, so no need to save the preference again.
				SetSettingValue(resolutionSettingData, playerPrefResolution, immediatelySavePreference: false);
			}
			else
			{
				// Invalid preference value, use fallback based on it
				Resolution fallbackValue = resolutionSettingData.GetFallbackValueFrom(playerPrefResolution);

				DebugUtil.LogWarningFormat("[SettingsManager] InitializeResolutionSettingFromPreferences: " +
					"playerPrefResolution({0}) returned invalid value {1}, falling back with " +
					"GetFallbackValueFrom to {2}",
					resolutionSettingData, playerPrefResolution, fallbackValue);

				if (!resolutionSettingData.IsValueValid(fallbackValue))
				{
					if (engineSetting != null)
					{
						DebugUtil.LogErrorFormat(resolutionSettingData,
							"[SettingsManager] InitializeSimpleSetting: GetFallbackValueFrom({0}) returned " +
							"invalid value {1}. {2} is an engine setting, so just do nothing to keep the default value.",
							playerPrefResolution, fallbackValue, engineSetting);
						// Note the return to do nothing
						return;
					}
					else  // customSetting != null
					{
						// ! Resolution setting is very likely an IEngineSetting, but for completion we support the case of
						// ! ICustomSetting. You'll notice how your IDE may even warn you that it never happens.
							DebugUtil.LogErrorFormat(resolutionSettingData,
								"[SettingsManager] InitializeSimpleSetting: GetFallbackValueFrom({0}) returned " +
								"invalid value {1}, {2} is a custom setting, so falling back a second time to " +
								"default value as ultimate resort. Make sure to fix GetFallbackValueFrom to return a " +
								"valid value.",
								playerPrefResolution, fallbackValue, customSetting);
							fallbackValue = customSetting.GetDefaultValue();
					}
				}

				// Set setting value to fallback value
				// Whether we want to resave the fallback value in preferences is up to design. In this case, player
				// has not actively changed setting yet, so following the same logic as when player keeps the default
				// value, we decide not to save it immediately.
				SetSettingValue(resolutionSettingData, fallbackValue, immediatelySavePreference: false);
			}
		}
		else
		{
			// No preference found at all, use default value as in InitializeSimpleSetting
			// ! Resolution setting is very likely an IEngineSetting, but for completion we support the case of
			// ! ICustomSetting. You'll notice how your IDE may even warn you that it never happens.
			if (customSetting != null)
			{
				// Note that this will not save any preference
				// The player did not actively change setting to default, so this is what we want
				InitializeCustomSettingToDefaultValue(customSetting);
			}
		}
	}

	private void InitializeCustomSettingToDefaultValue<TSettingValue>(ICustomSetting<TSettingValue> customSetting)
	{
		// Set custom setting to default value at low level
		// This will not save any preference, which is what we want when initializing to default, with no player action
		TSettingValue defaultValue = customSetting.GetDefaultValue();
		SetCustomSettingValue(customSetting, defaultValue);
	}

	#endregion


	#region SettingValue

	private TSettingValue GetDefaultSettingValue<TSettingValue>(SettingData<TSettingValue> settingData)
	{
		switch (settingData)
		{
			case IEngineSetting<TSettingValue> engineSetting:
				return GetDefaultEngineSettingValue(engineSetting);
			case ICustomSetting<TSettingValue> customSetting:
				return customSetting.GetDefaultValue();
			default:
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] GetDefaultSettingValue: {0} has unsupported BaseSettingData subclass {1}, " +
					"as it doesn't implement IEngineSetting<> or ICustomSetting<>. Falling back to default.",
					settingData, settingData.GetType());
				return default;
		}
	}

	/// Return default setting value for engine setting data
	private TSettingValue GetDefaultEngineSettingValue<TSettingValue>(IEngineSetting<TSettingValue> engineSettingData)
	{
		var settingData = (SettingData<TSettingValue>) engineSettingData;
		if (defaultEngineSettingValueDictionary.TryGetValue(settingData, out object value))
		{
			try
			{
				return (TSettingValue) value;
			}
			catch (InvalidCastException e)
			{
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] GetEngineSettingDefaultValue: incorrect unboxing of value associated " +
					"to setting data {0}. Should be of type {1}. Falling back to default ({2}). " +
					"InvalidCastException message: {3}",
					engineSettingData, typeof(TSettingValue), default, e.Message);
			}
		}
		else
		{
			DebugUtil.LogErrorFormat(settingData,
				"[SettingsManager] GetEngineSettingDefaultValue: could not get value for setting key {0} " +
				"(with setting value of type ({1})) in customSettingValueDictionary. " +
				"Make sure to add that setting data to SettingsManager's Setting Data List and to call this method " +
				"after Start > InitializeAllSettings. Falling back to default ({2}). ",
				engineSettingData, typeof(TSettingValue), default);
		}

		return default;
	}

	/// Return current setting value for setting data
	/// Engine setting: return value from engine
	/// Custom setting: return value from dictionary
	public TSettingValue GetSettingValue<TSettingValue>(SettingData<TSettingValue> settingData)
	{
		switch (settingData)
		{
			case IEngineSetting<TSettingValue> engineSetting:
				return engineSetting.GetValue();
			case ICustomSetting<TSettingValue> customSetting:
				return GetCustomSettingValue(customSetting);
			default:
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] GetSettingValue: {0} has unsupported BaseSettingData subclass {1}, " +
					"as it doesn't implement IEngineSetting<> or ICustomSetting<>. " +
					"Return default value as fallback.",
					settingData, settingData.GetType());
				return default;
		}
	}

	/// Return custom setting value for setting data
	public TSettingValue GetCustomSettingValue<TSettingValue>(ICustomSetting<TSettingValue> customSettingData)
	{
		var settingData = (SettingData<TSettingValue>) customSettingData;
		if (currentCustomSettingValueDictionary.TryGetValue(settingData, out object value))
		{
			try
			{
				return (TSettingValue) value;
			}
			catch (InvalidCastException e)
			{
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] GetCustomSettingValue: incorrect unboxing of value associated " +
					"to setting data {0}. Should be of type {1}. Falling back to default ({2}). " +
					"InvalidCastException message: {3}",
					customSettingData, typeof(TSettingValue), default, e.Message);
			}
		}
		else
		{
			DebugUtil.LogErrorFormat(settingData,
				"[SettingsManager] GetCustomSettingValue: could not get value for setting key {0} " +
				"(with setting value of type ({1})) in customSettingValueDictionary. " +
				"Make sure to add that setting data to SettingsManager's Setting Data List and to call this method " +
				"after Start > InitializeAllSettings. Falling back to default ({2}). ",
				customSettingData, typeof(TSettingValue), default);
		}

		return default;
	}

	/// Return current setting value for setting data, converting it to readable scale/format for display on UI
	public TSettingValue GetSettingAsReadableValue<TSettingValue>(SettingData<TSettingValue> settingData)
	{
		TSettingValue storedValue = GetSettingValue(settingData);
		return settingData.StoredToRepresentedValue(storedValue);
	}

	/// Set value for setting data
	/// Engine setting: set value in engine
	/// Custom setting: set value in dictionary
	/// If immediatelySavePreference is true, also set and save preference immediately
	public void SetSettingValue<TSettingValue>(SettingData<TSettingValue> settingData, TSettingValue storedValue,
		bool immediatelySavePreference)
	{
		switch (settingData)
		{
			case IEngineSetting<TSettingValue> engineSetting:
				engineSetting.SetValue(storedValue);
				break;
			case ICustomSetting<TSettingValue> customSetting:
				SetCustomSettingValue(customSetting, storedValue);
				break;
			default:
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] SetSettingValue: {0} has unsupported BaseSettingData subclass {1}, " +
					"as it doesn't implement IEngineSetting<> or ICustomSetting<>. Do nothing.",
					settingData, settingData.GetType());
				break;
		}

		if (!settingData.ignorePreferences && immediatelySavePreference)
		{
			SetPreference(settingData, storedValue);
			PlayerPrefs.Save();
		}
	}

	/// Set custom setting value in dictionary
	private void SetCustomSettingValue<TSettingValue>(ICustomSetting<TSettingValue> settingData, TSettingValue storedValue)
	{
		// Boxing TSettingValue to object is implicit, so no need to cast
		currentCustomSettingValueDictionary[(SettingData<TSettingValue>) settingData] = storedValue;
	}

	/// Set setting value from a readable value received from UI
	public void SetSettingFromReadableValue<TSettingValue>(SettingData<TSettingValue> settingData, TSettingValue readableValue)
	{
		TSettingValue storedValue = settingData.RepresentedToStoredValue(readableValue);
		SetSettingValue(settingData, storedValue, immediatelySavePreference: true);
	}

	/// Reset all settings to default values, optionally saving preferences
	public void ResetAllSettingsToDefaultValues(bool immediatelySavePreference)
	{
		foreach (var setting in settingDataList.entries)
		{
			ResetSettingToDefaultValue(setting, immediatelySavePreference);
		}
	}

	/// Reset passed setting (as base type) to default values, optionally saving preference
	private void ResetSettingToDefaultValue(BaseSettingData settingData, bool immediatelySavePreference)
	{
		// This is just bridging code to work from non-generic to generic setting data
		switch (settingData)
		{
			case SettingData<bool> boolSettingData:
				ResetSettingToDefaultValue(boolSettingData, immediatelySavePreference);
				break;
			case SettingData<int> intSettingData:
				ResetSettingToDefaultValue(intSettingData, immediatelySavePreference);
				break;
			case SettingData<float> floatSettingData:
				ResetSettingToDefaultValue(floatSettingData, immediatelySavePreference);
				break;
			case SettingData<Resolution> resolutionSettingData:
				ResetSettingToDefaultValue(resolutionSettingData, immediatelySavePreference);
				break;
			default:
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] ResetSettingToDefaultValue: {0} has unsupported BaseSettingData subclass {1}, " +
					"as it doesn't have a TSettingValue of type bool, int, float nor Resolution",
					settingData, settingData.GetType());
				break;
		}
	}

	/// Reset passed setting (as generic type) to default value, optionally saving preference
	private void ResetSettingToDefaultValue<TSettingValue>(SettingData<TSettingValue> settingData, bool immediatelySavePreference)
	{
		SetSettingValue(settingData, GetDefaultSettingValue(settingData), immediatelySavePreference);
	}

	#endregion


	#region Preferences

	/// Return bool setting from player preferences
	private static bool GetBoolSettingFromPreferences(SettingData<bool> boolSetting)
	{
		return GetPlayerPrefsBool(boolSetting.playerPrefKey);
	}

	/// Return PlayerPrefs bool value for key encoded as integer (0: false, 1: true) if any
	/// If key is not present, return false
	private static bool GetPlayerPrefsBool(string key)
	{
		return PlayerPrefs.GetInt(key) == 1;
	}

	/// Return integer setting from player preferences
	private static int GetIntSettingFromPreferences(SettingData<int> intSetting)
	{
		return PlayerPrefs.GetInt(intSetting.playerPrefKey);
	}

	/// Return float setting from player preferences
	private static float GetFloatSettingFromPreferences(SettingData<float> intSetting)
	{
		return PlayerPrefs.GetFloat(intSetting.playerPrefKey);
	}

	/// Set player preference to value
	/// ! You must call PlayerPrefs.Save() after setting all the preferences you needed to change, to save them
	private void SetPreference<TSettingValue>(SettingData<TSettingValue> settingData, TSettingValue value)
	{
		// Type check is a little brutal, same remark as InitializeSetting
		if (settingData is SettingData<bool> boolSettingData)
		{
			if (value is bool boolValue)
			{
				SetBoolPreference(boolSettingData, boolValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(boolSettingData,
					"[SettingsManager] SetPreference: boolSettingData {0} is a SettingData<bool> but value " +
					"has type {1} instead of bool, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					boolSettingData, value.GetType());
			}
		}
		else if (settingData is SettingData<int> intSettingData)
		{
			if (value is int intValue)
			{
				SetIntPreference(intSettingData, intValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(intSettingData,
					"[SettingsManager] SetPreference: intSettingData {0} is a SettingData<int> but value " +
					"has type {1} instead of int, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					intSettingData, value.GetType());
			}
		}
		else if (settingData is SettingData<float> floatSettingData)
		{
			if (value is float floatValue)
			{
				SetFloatPreference(floatSettingData, floatValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(floatSettingData,
					"[SettingsManager] SetPreference: floatSettingData {0} is a SettingData<float> but value " +
					"has type {1} instead of float, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					floatSettingData, value.GetType());
			}
		}
		else if (settingData is SettingData<Resolution> resolutionSetting)
		{
			if (value is Resolution resolutionValue)
			{
				SetResolutionPreference(resolutionSetting, resolutionValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(resolutionSetting,
					"[SettingsManager] SetPreference: resolutionSetting {0} is a SettingData<Resolution> but value " +
					"has type {1} instead of Resolution, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					resolutionSetting, value.GetType());
			}
		}
		else
		{
			DebugUtil.LogErrorFormat(settingData,
				"[SettingsManager] SetPreference: Unsupported SettingData<TSettingValue> subclass, setting {0} " +
				"uses TSettingValue = {1}, we only support bool, int, float and Resolution",
				settingData, settingData.GetType().GetGenericArguments()[0]);
		}
	}

	/// Set bool player preference
	private static void SetBoolPreference(SettingData<bool> boolSettingData, bool value)
	{
		SetPlayerPrefsBool(boolSettingData.playerPrefKey, value);
	}

	/// Set PlayerPrefs bool value for key with value encoded as integer (0: false, 1: true)
	private static void SetPlayerPrefsBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}

	/// Set integer player preference
	private static void SetIntPreference(SettingData<int> intSettingData, int value)
	{
		PlayerPrefs.SetInt(intSettingData.playerPrefKey, value);
	}

	/// Set float player preference
	private static void SetFloatPreference(SettingData<float> floatSettingData, float value)
	{
		PlayerPrefs.SetFloat(floatSettingData.playerPrefKey, value);
	}

	/// Set Resolution player preference
	private static void SetResolutionPreference(SettingData<Resolution> resolutionSetting, Resolution resolution)
	{
		// Use same convention as LoadResolutionSettingFromPreferences for the key suffixes
		string resolutionWidthPlayerPrefKey = $"{resolutionSetting.playerPrefKey}.Width";
		string resolutionHeightPlayerPrefKey = $"{resolutionSetting.playerPrefKey}.Height";
		string resolutionRefreshRatePlayerPrefKey = $"{resolutionSetting.playerPrefKey}.RefreshRate";

		PlayerPrefs.SetInt(resolutionWidthPlayerPrefKey, resolution.width);
		PlayerPrefs.SetInt(resolutionHeightPlayerPrefKey, resolution.height);
		PlayerPrefs.SetInt(resolutionRefreshRatePlayerPrefKey, resolution.refreshRate);
	}

	#endregion
}
