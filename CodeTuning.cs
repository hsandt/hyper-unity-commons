using System;
using UnityEngine;

/// Code Tuning runtime class (required to run the game without knowing editor stuff)
/// Builds will be playable with default values, but we advise to remove any CodeTuning references in the code before building
public class CodeTuning
{
	#if UNITY_EDITOR
	/// Is code tuning active? If not, use default values.
	public bool active;
	#endif

	public int branchIndex;
	public static int GetBranchIndex (int defaultValue) {
		return TryGetValue<int> (Instance.branchIndex, defaultValue);
	}

	public bool bool1;
	public static bool GetBool1 (bool defaultValue) {
		return TryGetValue<bool> (Instance.bool1, defaultValue);
	}

	public bool bool2;
	public static bool GetBool2 (bool defaultValue) {
		return TryGetValue<bool> (Instance.bool2, defaultValue);
	}

	public float float1;
	public static float GetFloat1 (float defaultValue) {
		return TryGetValue<float> (Instance.float1, defaultValue);
	}

	public float float2;
	public static float GetFloat2 (float defaultValue) {
		return TryGetValue<float> (Instance.float2, defaultValue);
	}

	// singleton instance
	static CodeTuning _instance;
	public static CodeTuning Instance {
		get {
			if (_instance != null) return _instance;
			Debug.Log ("[DEBUG] Creating CodeTuning instance singleton");
			_instance = new CodeTuning();
			return _instance;
		}
	}

	private CodeTuning () {}

	static T TryGetValue<T>(T tuningValue, T defaultValue) {
		#if UNITY_EDITOR
		return Instance.active ? tuningValue : defaultValue;
		#else
		Debug.LogErrorFormat("CodeTuning cannot be used to get tuning values outside Editor, returning default value {0}", defaultValue);
		return defaultValue;
		#endif
	}

}
