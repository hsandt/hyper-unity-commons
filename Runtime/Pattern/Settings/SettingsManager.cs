// #define DEBUG_SETTINGS_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using HyperUnityCommons;

/// Settings Manager
/// We recommend creating it and making it DontDestroyOnLoad via some PersistentManagersGenerator (specific to project)
public class SettingsManager : SingletonManager<SettingsManager>
{
	#region New API

	// Note that we need BaseSettingData to add any setting data in the inspector
	// SettingData<object> would not work, as it would show all SettingData<T> in the assignment popup, but only accept
	// SettingData specifically bound to object (not any type), which we never use.
	[Tooltip("List of settings of any type to show in order")]
	public List<BaseSettingData> settings;


	/* State */

	/// Dictionary of settings of any type, using boxing
	private readonly Dictionary<BaseSettingData, object> settingValueDictionary = new();


	protected override void Init()
	{
		DebugUtil.AssertListElementsNotNull(settings, this, nameof(settings));

		foreach (var setting in settings)
		{
			setting.AssertIsValid();
			LoadSettingFromPreferences(setting);
		}

		// AUDIO (SUPERSEDED)
		// LoadAudioPrefs();

		// CINEMATIC
		LoadCinematicPrefs();

		// GRAPHICS (SUPERSEDED)
		// LoadScreenPrefs();
	}

	/// Return current setting value
	/// If settingData.alwaysCheckEngineValue is true, and default (engine) value differs from value in dictionary,
	/// update dictionary value to default (engine) value and use this one instead.
	public TSettingValue GetSettingValue<TSettingValue>(SettingData<TSettingValue> settingData)
	{
		TSettingValue currentSettingValue = GetSettingValue_Internal(settingData);

		if (settingData.alwaysCheckEngineValue)
		{
			// This setting must be an engine setting with a value that can be set outside the Settings menu
			// (e.g. a quick toggle fullscreen shortcut could modify resolution without modifying the corresponding
			// setting), so the dictionary setting value may not be in sync with the actual current engine value.
			// Compare them
			TSettingValue defaultSettingValue = settingData.GetDefaultValueOnStart();
			if (Comparer<TSettingValue>.Default.Compare(currentSettingValue, defaultSettingValue) != 0)
			{
				// The engine value differs from stored dictionary value, engine value should have priority
				// So set the dictionary value to match engine value (without side effect, since the engine value is
				// already set)
				SetSetting_Internal(settingData, defaultSettingValue);

				// And remember to update the current setting value to the engine value, so code below can work
				// with this local variable (cheaper than calling GetSettingValue again)
				currentSettingValue = defaultSettingValue;
			}
		}

		return currentSettingValue;
	}

	private TSettingValue GetSettingValue_Internal<TSettingValue>(SettingData<TSettingValue> settingData)
	{
		if (settingValueDictionary.TryGetValue(settingData, out object value))
		{
			try
			{
				return (TSettingValue) value;
			}
			catch (InvalidCastException e)
			{
				DebugUtil.LogErrorFormat(settingData,
					"[SettingsManager] GetSettingValue: incorrect unboxing of value associated " +
					"to setting data {0}. Should be of type {1}. InvalidCastException message: {2}",
					settingData, typeof(TSettingValue), e.Message);
			}
		}

		#if UNITY_EDITOR
		Debug.LogErrorFormat("[SettingsManager] GetSettingValue: could not get value for setting {0} of type {1}, " +
			"falling back to default ({2}). Make sure to initialize all settings before calling this method, " +
			"by filling settings on the SettingsManager.",
			settingData, typeof(TSettingValue), default(TSettingValue));
		#endif

		return default;
	}

	public TSettingValue GetSettingAsReadableValue<TSettingValue>(SettingData<TSettingValue> settingData)
	{
		// Get stored value (note that it will also retrieve engine value if set to alwaysCheckEngineValue)
		TSettingValue storedValue = GetSettingValue(settingData);
		return settingData.StoredToRepresentedValue(storedValue);
	}

	/// Set setting in dictionary and call OnSetValue to update any engine value accordingly
	public void SetSetting<TSettingValue>(SettingData<TSettingValue> settingData, TSettingValue storedValue)
	{
		SetSetting_Internal(settingData, storedValue);

		// Update any engine value
		// This is mostly for immediate player feedback, such as tuning BGM volume while listening to it
		// For heavier changes such as resolution, consider buffering changes and applying them later
		settingData.OnSetValue(storedValue);

		// Set player preference to new value and save it immediately
		// When we add a buffer system (see comment above), we will also want to delay setting preferences to confirm
		// time, and save them all at once
		SetPreference(settingData, storedValue);
		PlayerPrefs.Save();
	}

	/// Just set setting value in dictionary, without calling OnSetValue nor saving any preference
	private void SetSetting_Internal<TSettingValue>(SettingData<TSettingValue> settingData, TSettingValue storedValue)
	{
		// Boxing TSettingValue to object is implicit, so no need to add anything
		settingValueDictionary[settingData] = storedValue;
	}

	public void SetSettingFromReadableValue<TSettingValue>(SettingData<TSettingValue> settingData, TSettingValue readableValue)
	{
		TSettingValue storedValue = settingData.RepresentedToStoredValue(readableValue);
		SetSetting(settingData, storedValue);
	}

	private void SetDefaultSetting<TSettingValue>(SettingData<TSettingValue> setting)
	{
		SetSetting(setting, setting.GetDefaultValueOnStart());
	}

	/// Load setting from player preferences
	/// If preference value is not valid, fallback to good value based on preference value
	/// If no preference has been saved, set value to default
	private void LoadSettingFromPreferences(BaseSettingData setting)
	{
		// Type check is a little brutal, but it works because we know that all setting data are in fact deriving from
		// SettingData<TSettingValue>.
		// The alternative is to use polymorphism, but that means we must define:
		// BoolDiscreteSettingData, IntDiscreteSettingData, FloatContinuousSettingData that would implement some
		// LoadSettingFromPreferences accordingly, and make sure to subclass them (and not SettingData<bool/int/float>
		// directly to define the final setting data classes.
		if (setting is SettingData<bool> boolSetting)
		{
			LoadSimpleSettingFromPreferences(boolSetting, GetBoolSettingFromPreferences);
		}
		else if (setting is SettingData<int> intSetting)
		{
			LoadSimpleSettingFromPreferences(intSetting, GetIntSettingFromPreferences);
		}
		else if (setting is SettingData<float> floatSetting)
		{
			LoadSimpleSettingFromPreferences(floatSetting, GetFloatSettingFromPreferences);
		}
		else if (setting is SettingData<Resolution> resolutionSetting)
		{
			LoadResolutionSettingFromPreferences(resolutionSetting);
		}
		else
		{
			DebugUtil.LogErrorFormat(setting,
				"[SettingsManager] LoadSettingFromPreferences: Unsupported BaseSettingData subclass for {0}",
				setting);
		}
	}

	/// Load simple (primitive type) setting from player preferences, using callback specific to setting value type
	/// If preference value is not valid, fallback to good value based on preference value
	/// If no preference has been saved, set value to default
	/// getSettingFromPreferencesCallback should have signature:
	/// TSettingValue Method(SettingData&lt;TSettingValue&gt; settingData)
	private void LoadSimpleSettingFromPreferences<TSettingValue>(SettingData<TSettingValue> settingData,
		Func<SettingData<TSettingValue>, TSettingValue> getSettingFromPreferencesCallback)
	{
		if (PlayerPrefs.HasKey(settingData.playerPrefKey))
		{
			TSettingValue playerPrefStoredValue = getSettingFromPreferencesCallback(settingData);
			if (settingData.IsValueValid(playerPrefStoredValue))
			{
				// Valid preference value, use it
				SetSetting(settingData, playerPrefStoredValue);
			}
			else
			{
				// Invalid preference value, use fallback based on it
				TSettingValue fallbackValue = settingData.GetFallbackValueFrom(playerPrefStoredValue);
				DebugUtil.AssertFormat(settingData.IsValueValid(fallbackValue),
					"[SettingsManager] LoadSimpleSettingFromPreferences: GetFallbackValueFrom({0}) returned " +
					"invalid value, but we don't know better in this generic context, so still using it as fallback",
					playerPrefStoredValue);

				SetSetting(settingData, fallbackValue);
			}
		}
		else
		{
			// No preference found at all, use default value
			// (implementation can enforce some default, or find a good default from engine)
			SetDefaultSetting(settingData);
		}
	}

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

	/// Load resolution setting from multiple player preferences (width x height @ refresh rate)
	/// If preference value is not valid, fallback to good value based on preference value
	/// If no preference has been saved, set value to default
	private void LoadResolutionSettingFromPreferences(SettingData<Resolution> resolutionSetting)
	{
		// Implementation is similar to LoadSimpleSettingFromPreferences, but specialized
		// for Resolution which is a compounded type
		// Note that Unity also natively remembers its own resolution info:
		// - Screenmanager Resolution Width/Height
		// - Screenmanager Fullscreen mode
		// etc. but it's easier to work with our own preferences.

		string resolutionWidthPlayerPrefKey = $"{resolutionSetting.playerPrefKey}.Width";
		string resolutionHeightPlayerPrefKey = $"{resolutionSetting.playerPrefKey}.Height";
		string resolutionRefreshRatePlayerPrefKey = $"{resolutionSetting.playerPrefKey}.RefreshRate";

		// The most important preferences are width and height
		// Obviously, if you work with new code, you should have saved refresh rate too, but in case you are using
		// old preferences that don't have refresh rate, we support not knowing it
		if (PlayerPrefs.HasKey(resolutionWidthPlayerPrefKey) && PlayerPrefs.HasKey(resolutionHeightPlayerPrefKey))
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

			if (resolutionSetting.IsValueValid(playerPrefResolution))
			{
				// Valid preference value, use it
				SetSetting(resolutionSetting, playerPrefResolution);
			}
			else
			{
				// Invalid preference value, use fallback based on it
				Resolution fallbackValue = resolutionSetting.GetFallbackValueFrom(playerPrefResolution);
				DebugUtil.AssertFormat(resolutionSetting.IsValueValid(fallbackValue),
					"[SettingsManager] LoadResolutionSettingFromPreferences: GetFallbackValueFrom({0}) returned " +
					"invalid value, but we don't know better in this generic context, so still using it as fallback",
					playerPrefResolution);

				SetSetting(resolutionSetting, fallbackValue);
			}
		}
		else
		{
			// No preference found at all, use default value
			// (implementation can enforce some default, or find a good default from engine)
			SetDefaultSetting(resolutionSetting);
		}
	}

	/// Set player preference to value
	/// ! You must call PlayerPrefs.Save() after setting all the preferences you needed to change, to save them
	private void SetPreference<TSettingValue>(SettingData<TSettingValue> setting, TSettingValue value)
	{
		// Type check is a little brutal, same remark as LoadSettingFromPreferences
		if (setting is SettingData<bool> boolSetting)
		{
			if (value is bool boolValue)
			{
				SetBoolPreference(boolSetting, boolValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(boolSetting,
					"[SettingsManager] SetPreference: boolSetting {0} is a SettingData<bool> but value " +
					"has type {1} instead of bool, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					boolSetting, value.GetType());
			}
		}
		else if (setting is SettingData<int> intSetting)
		{
			if (value is int intValue)
			{
				SetIntPreference(intSetting, intValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(intSetting,
					"[SettingsManager] SetPreference: intSetting {0} is a SettingData<int> but value " +
					"has type {1} instead of int, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					intSetting, value.GetType());
			}
		}
		else if (setting is SettingData<float> floatSetting)
		{
			if (value is float floatValue)
			{
				SetFloatPreference(floatSetting, floatValue);
			}
			else
			{
				DebugUtil.LogErrorFormat(floatSetting,
					"[SettingsManager] SetPreference: floatSetting {0} is a SettingData<float> but value " +
					"has type {1} instead of float, this should not happen since this generic method ensures " +
					"TSettingValue consistency",
					floatSetting, value.GetType());
			}
		}
		else if (setting is SettingData<Resolution> resolutionSetting)
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
			DebugUtil.LogErrorFormat(setting,
				"[SettingsManager] SetPreference: Unsupported SettingData<TSettingValue> subclass, setting {0} " +
				"uses TSettingValue = {1}, we only support bool, int, float and Resolution",
				setting, setting.GetType().GetGenericArguments()[0]);
		}
	}

	/// Set bool player preference
	private static void SetBoolPreference(SettingData<bool> boolSetting, bool value)
	{
		SetPlayerPrefsBool(boolSetting.playerPrefKey, value);
	}

	/// Set PlayerPrefs bool value for key with value encoded as integer (0: false, 1: true)
	private static void SetPlayerPrefsBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}

	/// Set integer player preference
	private static void SetIntPreference(SettingData<int> intSetting, int value)
	{
		PlayerPrefs.SetInt(intSetting.playerPrefKey, value);
	}

	/// Set float player preference
	private static void SetFloatPreference(SettingData<float> floatSetting, float value)
	{
		PlayerPrefs.SetFloat(floatSetting.playerPrefKey, value);
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


	#region Old API

	/* Audio constants */

	// -40 dB of original (max) volume is a good value for a low volume that is still audible
	// When we want to mute volume, we use -80 dB instead
	const float minAudibleVolume = -40;


	[Header("Audio asset references")]

	[Tooltip("OLD Audio mixer used by the game. It should have the following Exposed Parameters:\n" +
		"- BGM Volume: Volume of BGM Group\n" +
		"- SFX Volume: Volume of SFX Group")]
	public AudioMixer audioMixer;


	[Header("Audio parameters")]

	[SerializeField, Tooltip("If checked, use default values")]
	private bool useDecibelDefaultValues = false;

	[SerializeField, Tooltip("Default value for m_BgmVolumeFactor")]
	[Range(0f, 1f)]
	private float defaultBgmVolumeFactor = 1f;

	[SerializeField, Tooltip("Default value for m_SfxVolumeFactor")]
	[Range(0f, 1f)]
	private float defaultSfxVolumeFactor = 1f;

	[SerializeField, Tooltip("Default value for Bgm Volume as dB")]
	[Range(-80f, 0f)]
	private float defaultBgmVolumeDb = 0f;

	[SerializeField, Tooltip("Default value for Sfx Volume as dB")]
	[Range(-80f, 0f)]
	private float defaultSfxVolumeDb = 0f;


	/* Audio state */

	/// Current BGM volume factor (0 to 1)
	private float m_BgmVolumeFactor;

	/// Current SFX volume factor (0 to 1)
	private float m_SfxVolumeFactor;


    [Header("Cinematic parameters")]

    [SerializeField, Tooltip("Default value for m_CinematicAutoAdvance")]
    private bool defaultCinematicAutoAdvance = false;


	/* Cinematic state */

    /// Should we automatically advance in cinematic?
    /// If false, player must press button to advance after text is shown
    private bool m_CinematicAutoAdvance = false;


	/* Graphics state */

	/// Current screen resolution
    Resolution m_ScreenResolution;

	/// Current screen refresh rate
    int m_ScreenRefreshRate;

	/// Is the resolution displayed in fullscreen mode? (Currently, true iff it is the biggest resolution available)
	bool m_Fullscreen;

    /// Current graphics quality
    int m_GraphicsQuality;


    #region Audio

	/// Load audio settings from player preferences
	/// If a key is not present, set value to default
	void LoadAudioPrefs ()
	{
		if (PlayerPrefs.HasKey("Audio.BGMVolumeFactor"))
		{
			float prefBGMVolumeFactor = LoadBGMVolumeFactorPref();
			SetBGMVolumeFactor(prefBGMVolumeFactor);
		}
		else
		{
			SetBGMVolumeToDefault();
		}

        if (PlayerPrefs.HasKey("Audio.SFXVolumeFactor"))
        {
	        float prefSFXVolumeFactor = LoadSFXVolumeFactorPref();
			SetSFXVolumeFactor(prefSFXVolumeFactor);
		}
        else
        {
	        SetSFXVolumeToDefault();
        }
	}

	public static float LoadBGMVolumeFactorPref()
	{
		return PlayerPrefs.GetFloat("Audio.BGMVolumeFactor");
	}

	public static float LoadSFXVolumeFactorPref()
	{
		return PlayerPrefs.GetFloat("Audio.SFXVolumeFactor");
	}

	/// Save current BGM volume in settings
	public void SaveBGMVolume ()
	{
		PlayerPrefs.SetFloat("Audio.BGMVolumeFactor", m_BgmVolumeFactor);
		PlayerPrefs.Save();
	}

	/// Save current SFX volume in settings
	public void SaveSFXVolume ()
	{
		PlayerPrefs.SetFloat("Audio.SFXVolumeFactor", m_SfxVolumeFactor);
		PlayerPrefs.Save();
	}

    public float GetBGMVolumeFactor ()
    {
        return m_BgmVolumeFactor;
    }

    public float GetSFXVolumeFactor ()
    {
	    return m_SfxVolumeFactor;
    }

    public void SetBGMVolumeFactor(float volumeFactor)
    {
        // Volume safety to never go beyond 0 db
        volumeFactor = Mathf.Clamp01(volumeFactor);

        // bookkeeping
        m_BgmVolumeFactor = volumeFactor;

        float bgmVolume = MathUtil.VolumeFactorToDb(volumeFactor);
        audioMixer.SetFloat("BGM Volume", bgmVolume);
    }

    public void SetBGMVolume(float volume)
    {
        // Volume safety to never go beyond 0 db
        volume = Mathf.Clamp(volume, -80f, 0f);

        // bookkeeping
        m_BgmVolumeFactor = MathUtil.VolumeDbToFactor(volume);

        audioMixer.SetFloat("BGM Volume", volume);
    }

    public void SetSFXVolumeFactor(float volumeFactor)
    {
        // Volume safety to never go beyond 0 db
        volumeFactor = Mathf.Clamp01(volumeFactor);

        // bookkeeping
        m_SfxVolumeFactor = volumeFactor;

        float sfxVolume = MathUtil.VolumeFactorToDb(volumeFactor);
        audioMixer.SetFloat("SFX Volume", sfxVolume);
    }

    public void SetSFXVolume(float volume)
    {
	    // Volume safety to never go beyond 0 db
	    volume = Mathf.Clamp(volume, -80f, 0f);

	    // bookkeeping
	    m_SfxVolumeFactor = MathUtil.VolumeDbToFactor(volume);

	    audioMixer.SetFloat("SFX Volume", volume);
    }

    private void SetBGMVolumeToDefault()
    {
	    if (useDecibelDefaultValues)
	    {
		    SetBGMVolume(defaultBgmVolumeDb);
	    }
	    else
	    {
		    SetBGMVolumeFactor(defaultBgmVolumeFactor);
	    }
    }

    private void SetSFXVolumeToDefault()
    {
	    if (useDecibelDefaultValues)
	    {
		    SetSFXVolume(defaultSfxVolumeDb);
	    }
	    else
	    {
		    SetSFXVolumeFactor(defaultSfxVolumeFactor);
	    }
    }

    #endregion


    #region Cinematic

    /// Load the cinematic settings from the player preferences if any
    /// If a key is not present, set value to default
    void LoadCinematicPrefs ()
    {
	    if (PlayerPrefs.HasKey("Cinematic.AutoAdvance"))
	    {
		    bool prefCinematicAutoAdvance = LoadCinematicAutoAdvancePref();
		    SetCinematicAutoAdvance(prefCinematicAutoAdvance);
	    }
	    else
	    {
		    SetCinematicAutoAdvance(defaultCinematicAutoAdvance);
	    }
    }

    public static bool LoadCinematicAutoAdvancePref()
    {
	    return GetPlayerPrefsBool("Cinematic.AutoAdvance");
    }

    /// Set and save current value of cinematic auto advance to settings
    public void SaveCinematicAutoAdvance()
    {
	    SetPlayerPrefsBool("Cinematic.AutoAdvance", m_CinematicAutoAdvance);
	    PlayerPrefs.Save();
    }

    /// Return true if dialogue should auto-continue
    public bool GetCinematicAutoAdvance ()
    {
	    return m_CinematicAutoAdvance;
    }

    /// Set cinematic auto-advance setting
    public void SetCinematicAutoAdvance(bool value)
    {
	    m_CinematicAutoAdvance = value;

	    #if DEBUG_SETTINGS_MANAGER
	    Debug.LogFormat("[SettingsManager] Cinematic Auto Advance set to {0}", m_CinematicAutoAdvance);
	    #endif
    }

    #endregion


    #region Graphics

	/// Load screen settings from player preferences, else default to fullscreen resolution
	void LoadScreenPrefs ()
	{
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions.Length == 0)
		{
			Debug.LogError("No resolutions available. Are you on Android?");
			return;
		}

		// Check if a resolution has been saved in the last session, and check that it is still correct in case the player
		// changed monitor in the meantime. Deduce fullscreen from the resolution instead of using the stored pref for the same reason.
        if (PlayerPrefs.HasKey("Graphics.Resolution.width") && PlayerPrefs.HasKey("Graphics.Resolution.height"))
        {
            float prefResolutionWidth = PlayerPrefs.GetInt("Graphics.Resolution.width");
            float prefResolutionHeight = PlayerPrefs.GetInt("Graphics.Resolution.height");

			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution resolution = resolutions[i];

				// Look for the closest match in the available resolutions. Ignore refresh rate, we'll just pick the refresh rate
				// for the first resolution that matches in size (since there is no refresh rate visible in the Options)
				// (if we selected 1280x720 @ 60Hz last time, it may revert to 1280x720 @ 50Hz next time, but I have found no such monitor yet)
				if (resolution.width == prefResolutionWidth && resolution.height == prefResolutionHeight)
				{
					// we found a matching resolution available, use it (fullscreen iff biggest resolution available)
					SetResolution(resolution, i == resolutions.Length - 1);
					return;
				}
				else if (resolution.width > prefResolutionWidth)
				{
					// Unity guarantees that resolutions are ordered by width ASC, so we just crossed the width we wanted
					if (i == 0)
					{
						SetResolution(resolution, resolutions.Length == 1);  // biggest resolution iff there is only one available
						return;
					}
					else
					{
						// pick the resolution just below, the width will be equal or lower than what we wanted,
						// and it cannot be the biggest one, so no fullscreen
						SetResolution(resolutions[i - 1], false);
						return;
					}
				}
			}
		}

		// If no player prefs or we couldn't find a matching resolution of equal or lower width,
        // keep the default resolution on start. It should be the fullscreen resolution except if you allowed a resolution dialog box on start.
        // Since we don't change the current resolution, we just bookkeep the current value
        this.m_ScreenResolution = Screen.currentResolution;
        // If the Player settings don't work and we need to enforce fullscreen programmatically, uncomment this:
		//SetResolution(resolutions[resolutions.Length - 1], true);

		// Unlike other prefs with default set by game, the default was set from context, so don't save it as an active
		// player preference (player may change monitor later and use the new default there)
	}

	public Resolution GetResolution()
	{
		return m_ScreenResolution;
	}

	/// Set the screen resolution (recommended to use Screen.resolutions to get allowed resolutions)
	public void SetResolution(Resolution resolution, bool fullscreen)
	{
		// bookkeeping
		this.m_ScreenResolution = resolution;

		Screen.SetResolution(resolution.width, resolution.height, fullscreen, resolution.refreshRate);

		#if DEBUG_SETTINGS_MANAGER
		Debug.LogFormat("[SettingsManager] Resolution set to {0}x{1} ({2}) @ {3}Hz", resolution.width, resolution.height, fullscreen ? "fullscreen" : "windowed", resolution.refreshRate);
		#endif
	}

	public void SaveResolution()
	{
		PlayerPrefs.SetInt("Graphics.Resolution.width", m_ScreenResolution.width);
        PlayerPrefs.SetInt("Graphics.Resolution.height", m_ScreenResolution.height);
		// Currently, fullscreen is entirely determined by the resolution size so storing it is optional.
//		PlayerPrefs.SetInt("Resolution.fullscreen", fullscreen ? 0 : 1);
		PlayerPrefs.Save();
	}

    /// Load screen settings from player preferences, else default to fullscreen resolution
    void LoadQualityPrefs()
    {
        if (PlayerPrefs.HasKey("Graphics.Quality"))
        {
            int prefQuality = PlayerPrefs.GetInt("Graphics.Quality");
            SetGraphicsQuality(prefQuality);
        }
        else
        {
            // Keep default settings.
            // We don't have access to the default quality level for this platform directly via Unity's API, so instead we get the current
            // quality settings and bookkeep them without changing them.
            m_GraphicsQuality = QualitySettings.GetQualityLevel();
        }
    }

    public int GetGraphicsQuality ()
    {
        return QualitySettings.GetQualityLevel();
    }

    /// Set the graphics quality by level of preset
    public void SetGraphicsQuality (int level)
    {
        if (level < 0 || level >= QualitySettings.names.Length) {
            Debug.LogErrorFormat("[SettingsManager] Quality level {0} is out of bounds ({1} quality levels available)", level, QualitySettings.names.Length);
            return;
        }

        // bookkeeping
        m_GraphicsQuality = level;

        QualitySettings.SetQualityLevel(level, true);  // we are in the options, so we don't mind applying expensive operations now

        #if DEBUG_SETTINGS_MANAGER
        Debug.LogFormat("[SettingsManager] Quality settings set to level {0} '{1}'", level, QualitySettings.names[level]);
        #endif
    }

    public void SaveGraphicsQuality ()
    {
        PlayerPrefs.SetInt("Graphics.Quality", m_GraphicsQuality);
        PlayerPrefs.Save();
    }

	#endregion


	/// Revert all settings to defaults, without saving to prefs
	public void RevertToDefaults()
	{
		SetBGMVolumeToDefault();
		SetSFXVolumeToDefault();

		SetCinematicAutoAdvance(defaultCinematicAutoAdvance);

		Resolution[] resolutions = Screen.resolutions;
		if (resolutions.Length > 0)
		{
			// Default is fullscreen (biggest) resolution
			Resolution fullscreenResolution = resolutions[resolutions.Length - 1];
			SetResolution(fullscreenResolution, true);
		}
	}

	#endregion
}
