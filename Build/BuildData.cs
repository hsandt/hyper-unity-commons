using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildData", menuName = "Data/Build Data", order = 1)]
public class BuildData : ScriptableObject {

	// Example: My App 3.0.27 -> appName: "My App", majorVersion: 3, minorVersion: 0, stageVersion: 27
	public string appName = "My App";
	public int majorVersion = 0;
	public int minorVersion = 0;
	public int stageVersion = 1;

}
