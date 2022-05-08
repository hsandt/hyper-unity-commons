// based on discussion at http://answers.unity3d.com/questions/286571/can-i-disable-live-recompile.html
// objective: provide a command to ensure the game stops before compiling (refreshing assets)

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CommonsEditor
{

	public static class EditorAssets
	{

		/// Mark all selected assets dirty
		/// Use this to force resave and version upgrade
		/// (e.g. after removing a member or renaming a member with FormerlySerializedAs)
		[MenuItem("Assets/Mark Dirty", false, 38)]
		private static void MarkDirty()
		{
			Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets | SelectionMode.Editable);
			foreach (Object selectedAsset in selectedAssets)
			{
				MarkDirty(selectedAsset);
			}
		}

		private static void MarkDirty(Object selectedAsset)
		{
			if (AssetDatabase.IsMainAsset(selectedAsset))
			{
				EditorUtility.SetDirty(selectedAsset);
			}
			else
			{
				Debug.LogErrorFormat("[EditorAssets] MarkDirty: selected object {0} is not a Main Asset, this tool is meant to re-save main assets only", selectedAsset);
			}
		}

		/// Safe Refresh: stop playing if needed, then refresh assets
		/// This is now superseded by Unity's native Preferences > General >> Script Changes While Playing
		/// with mode "Stop Playing And Recompile", then just do a normal Refresh.
		/// Priority 39: just above Refresh
		[MenuItem("Assets/Safe Refresh %&a", false, 39)]
		private static void RefreshSafe()
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
		private static void P0() {}
		[MenuItem( "Assets/P10", false, 10)]
		private static void P10() {}
		[MenuItem( "Assets/P15", false, 15)]
		private static void P15() {}
		[MenuItem( "Assets/P20", false, 20)]
		private static void P20() {}
		[MenuItem( "Assets/P25", false, 25)]
		private static void P25() {}
		[MenuItem( "Assets/P30", false, 30)]
		private static void P30() {}
		[MenuItem( "Assets/P35", false, 35)]
		private static void P35() {}
		[MenuItem( "Assets/P36", false, 36)]
		private static void P36() {}
		[MenuItem( "Assets/P37", false, 37)]
		private static void P37() {}
		[MenuItem( "Assets/P38", false, 38)]
		private static void P38() {}
		[MenuItem( "Assets/P39", false, 39)]
		private static void P39() {}
		[MenuItem( "Assets/P40", false, 40)]
		private static void P40() {}
		[MenuItem( "Assets/P41", false, 41)]
		private static void P41() {}
		[MenuItem( "Assets/P42", false, 42)]
		private static void P42() {}
		[MenuItem( "Assets/P60", false, 60)]
		private static void P60() {}
		[MenuItem( "Assets/P80", false, 80)]
		private static void P80() {}
		[MenuItem( "Assets/P999", false, 999)]
		private static void P999() {}
		*/

	}

}
