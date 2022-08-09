// #define DEBUG_SETTINGS_MANAGER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using CommonsHelper;
using CommonsPattern;

/// Settings Manager
/// We recommend creating it and making it DontDestroyOnLoad via some PersistentManagersGenerator (specific to project)
public class SettingsManager : SingletonManager<SettingsManager>
{
	#region New API

	// Limitation: we can only show all bool settings together, then all float settings together,
	// as we cannot declare a member of generic type List<SettingData<T>>.
	// This could work if we define a non-generic base class for SettingData though
	// (we could also use List<ScriptableObject> then cast, but it's too vague and will allow
	// too many assets to be set in the inspector)

	[Tooltip("List of boolean settings to show in order")]
	public List<SettingData<bool>> boolSettings;

	[Tooltip("List of float settings to show in order")]
	public List<SettingData<float>> floatSettings;

	/* State */

	/// Dictionary of all boolean settings
	private readonly SettingDictionary<bool> boolSettingDictionary = new();

	/// Dictionary of all float settings
	private readonly SettingDictionary<float> floatSettingDictionary = new();


	public bool GetBoolSetting(SettingData<bool> boolSettingData)
	{
		return GetSettingInDictionary(boolSettingData, boolSettingDictionary);
	}

	public float GetFloatSetting(SettingData<float> floatSettingData)
	{
		return GetSettingInDictionary(floatSettingData, floatSettingDictionary);
	}

	public bool GetBoolSettingAsReadableValue(SettingData<bool> boolSettingData)
	{
		bool storedValue = GetSettingInDictionary(boolSettingData, boolSettingDictionary);
		return boolSettingData.StoredToReadableValue(storedValue);
	}

	public float GetFloatSettingAsReadableValue(SettingData<float> floatSettingData)
	{
		float storedValue = GetSettingInDictionary(floatSettingData, floatSettingDictionary);
		return floatSettingData.StoredToReadableValue(storedValue);
	}

	private static T GetSettingInDictionary<T>(SettingData<T> settingData, SettingDictionary<T> settingDictionary)
	{
		if (settingDictionary.TryGetValue(settingData, out T value))
		{
			return value;
		}

		#if UNITY_EDITOR
		Debug.LogErrorFormat("[SettingsManager] GetSetting: could not get value for setting {0} of type {1}, " +
			"falling back to default ({2}). Make sure to initialize all settings before calling this method",
			settingData, typeof(T), default(T));
		#endif

		return default;
	}

	public void SetBoolSetting(SettingData<bool> boolSettingData, bool storedValue)
	{
		SetSettingInDictionary(boolSettingData, boolSettingDictionary, storedValue);
	}

	public void SetFloatSetting(SettingData<float> floatSettingData, float storedValue)
	{
		SetSettingInDictionary(floatSettingData, floatSettingDictionary, storedValue);
	}

	public void SetBoolSettingFromReadableValue(SettingData<bool> boolSettingData, bool readableValue)
	{
		bool storedValue = boolSettingData.ReadableToStoredValue(readableValue);
		SetBoolSetting(boolSettingData, storedValue);
	}

	public void SetFloatSettingFromReadableValue(SettingData<float> floatSettingData, float readableValue)
	{
		float storedValue = floatSettingData.ReadableToStoredValue(readableValue);
		SetFloatSetting(floatSettingData, storedValue);
	}

	private void SetDefaultBoolSetting(SettingData<bool> boolSettingData)
	{
		SetBoolSetting(boolSettingData, boolSettingData.defaultValue);
	}

	private void SetDefaultFloatSetting(SettingData<float> floatSettingData)
	{
		SetFloatSetting(floatSettingData, floatSettingData.defaultValue);
	}

	private static void SetSettingInDictionary<T>(SettingData<T> settingData, SettingDictionary<T> settingDictionary,
		T storedValue)
	{
		settingDictionary[settingData] = storedValue;
		settingData.OnSetValue(storedValue);
	}

	/// Load settings from player preferences
	/// If a key is not present, set value to parameterized default
	private void LoadBoolSettingFromPreferences(SettingData<bool> boolSettingData)
	{
		if (PlayerPrefs.HasKey(boolSettingData.playerPrefKey))
		{
			bool playerPrefStoredValue = GetPlayerPrefsBool(boolSettingData.playerPrefKey);
			SetBoolSetting(boolSettingData, playerPrefStoredValue);
		}
		else
		{
			SetDefaultBoolSetting(boolSettingData);
		}
	}

	/// Load settings from player preferences
	/// If a key is not present, set value to parameterized default
	private void LoadFloatSettingFromPreferences(SettingData<float> floatSettingData)
	{
		if (PlayerPrefs.HasKey(floatSettingData.playerPrefKey))
		{
			float playerPrefStoredValue = PlayerPrefs.GetFloat(floatSettingData.playerPrefKey);
			SetFloatSetting(floatSettingData, playerPrefStoredValue);
		}
		else
		{
			SetDefaultFloatSetting(floatSettingData);
		}
	}

	#endregion


	#region Old API

	/* Audio constants */

	// -40 dB of original (max) volume is a good value for a low volume that is still audible
	// When we want to mute volume, we use -80 dB instead
	const float minAudibleVolume = -40;


	[Header("Audio asset references")]

	[Tooltip("Audio mixer used by the game. It should have the following Exposed Parameters:\n" +
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


    protected override void Init()
    {
		foreach (var boolSetting in boolSettings)
		{
			LoadBoolSettingFromPreferences(boolSetting);
		}

		foreach (var floatSetting in floatSettings)
		{
			LoadFloatSettingFromPreferences(floatSetting);
		}

        // AUDIO
        LoadAudioPrefs();

		// CINEMATIC
        LoadCinematicPrefs();

		// GRAPHICS
		LoadScreenPrefs();
	}

	/// Return PlayerPrefs bool value for key encoded as integer (0: false, 1: true) if any
	/// If key is not present, return false
	private static bool GetPlayerPrefsBool(string key)
	{
		return PlayerPrefs.GetInt(key) == 1;
	}

	/// Set PlayerPrefs bool value for key with value encoded as integer (0: false, 1: true)
	private static void SetPlayerPrefsBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}


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
