// based on discussion at http://answers.unity3d.com/questions/286571/can-i-disable-live-recompile.html
// objective: provide a command to ensure the game stops before compiling (refreshing assets)

using UnityEngine;
using UnityEditor;
using System.Collections;

public static class SafeRefresh
{

	[MenuItem( "Assets/Safe Refresh #&r", false, 39)]
	static void RefreshSafe()
	{
		if (EditorApplication.isPlaying) {
		    Debug.Log("Exiting play mode to compile scripts.");
		    EditorApplication.isPlaying = false;
		}
		AssetDatabase.Refresh();
	}

	// DEBUG: find priority location in Assets menu
	/*
	[MenuItem( "Assets/P0", false, 0)]
	static void P0() {}
	[MenuItem( "Assets/P10", false, 10)]
	static void P10() {}
	[MenuItem( "Assets/P15", false, 15)]
	static void P15() {}
	[MenuItem( "Assets/P20", false, 20)]
	static void P20() {}
	[MenuItem( "Assets/P25", false, 25)]
	static void P25() {}
	[MenuItem( "Assets/P30", false, 30)]
	static void P30() {}
	[MenuItem( "Assets/P35", false, 35)]
	static void P35() {}
	[MenuItem( "Assets/P36", false, 36)]
	static void P36() {}
	[MenuItem( "Assets/P37", false, 37)]
	static void P37() {}
	[MenuItem( "Assets/P38", false, 38)]
	static void P38() {}
	[MenuItem( "Assets/P39", false, 39)]
	static void P39() {}
	[MenuItem( "Assets/P40", false, 40)]
	static void P40() {}
	[MenuItem( "Assets/P41", false, 41)]
	static void P41() {}
	[MenuItem( "Assets/P42", false, 42)]
	static void P42() {}
	[MenuItem( "Assets/P60", false, 60)]
	static void P60() {}
	[MenuItem( "Assets/P80", false, 80)]
	static void P80() {}
	[MenuItem( "Assets/P999", false, 999)]
	static void P999() {}
	*/

}
