using System;
using UnityEngine;

/// Code Tuning runtime class (required to run the game without knowing editor stuff)
/// Builds will be playable with default values, but we advise to remove any CodeTuning references in the code before building
public class CodeTuning
{

	public int branchIndex;
	public static int BranchIndex { get { return Instance.branchIndex; } }
	public float float1;
	public static float Float1 { get { return Instance.float1; } }

	// singleton instance
	static CodeTuning _instance;
	public static CodeTuning Instance {
		get {
			if (_instance != null) return _instance;
			_instance = new CodeTuning();
			return _instance;
		}
	}

	private CodeTuning () {}

}
