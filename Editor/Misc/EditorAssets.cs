// based on discussion at http://answers.unity3d.com/questions/286571/can-i-disable-live-recompile.html
// objective: provide a command to ensure the game stops before compiling (refreshing assets)

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HyperUnityCommons.Editor
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
		// Replace with Edit/, etc. to test placement in any menu
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
		[MenuItem( "Assets/P90", false, 90)]
		private static void P90() {}
		[MenuItem( "Assets/P100", false, 100)]
		private static void P100() {}
		[MenuItem( "Assets/P110", false, 110)]
		private static void P110() {}
		[MenuItem( "Assets/P120", false, 120)]
		private static void P120() {}
		[MenuItem( "Assets/P130", false, 130)]
		private static void P130() {}
		[MenuItem( "Assets/P140", false, 140)]
		private static void P140() {}
		[MenuItem( "Assets/P150", false, 150)]
		private static void P150() {}
		[MenuItem( "Assets/P160", false, 160)]
		private static void P160() {}
		[MenuItem( "Assets/P170", false, 170)]
		private static void P170() {}
		[MenuItem( "Assets/P180", false, 180)]
		private static void P180() {}
		[MenuItem( "Assets/P190", false, 190)]
		private static void P190() {}
		[MenuItem( "Assets/P200", false, 200)]
		private static void P200() {}
		[MenuItem( "Assets/P210", false, 210)]
		private static void P210() {}
		[MenuItem( "Assets/P220", false, 220)]
		private static void P220() {}
		[MenuItem( "Assets/P230", false, 230)]
		private static void P230() {}
		[MenuItem( "Assets/P240", false, 240)]
		private static void P240() {}
		[MenuItem( "Assets/P250", false, 250)]
		private static void P250() {}
		[MenuItem( "Assets/P260", false, 260)]
		private static void P260() {}
		[MenuItem( "Assets/P270", false, 270)]
		private static void P270() {}
		[MenuItem( "Assets/P280", false, 280)]
		private static void P280() {}
		[MenuItem( "Assets/P290", false, 290)]
		private static void P290() {}
		[MenuItem( "Assets/P300", false, 300)]
		private static void P300() {}
		[MenuItem( "Assets/P310", false, 310)]
		private static void P310() {}
		[MenuItem( "Assets/P320", false, 320)]
		private static void P320() {}
		[MenuItem( "Assets/P330", false, 330)]
		private static void P330() {}
		[MenuItem( "Assets/P340", false, 340)]
		private static void P340() {}
		[MenuItem( "Assets/P350", false, 350)]
		private static void P350() {}
		[MenuItem( "Assets/P360", false, 360)]
		private static void P360() {}
		[MenuItem( "Assets/P370", false, 370)]
		private static void P370() {}
		[MenuItem( "Assets/P380", false, 380)]
		private static void P380() {}
		[MenuItem( "Assets/P390", false, 390)]
		private static void P390() {}
		[MenuItem( "Assets/P400", false, 400)]
		private static void P400() {}
		[MenuItem( "Assets/P410", false, 410)]
		private static void P410() {}
		[MenuItem( "Assets/P420", false, 420)]
		private static void P420() {}
		[MenuItem( "Assets/P430", false, 430)]
		private static void P430() {}
		[MenuItem( "Assets/P440", false, 440)]
		private static void P440() {}
		[MenuItem( "Assets/P450", false, 450)]
		private static void P450() {}
		[MenuItem( "Assets/P460", false, 460)]
		private static void P460() {}
		[MenuItem( "Assets/P470", false, 470)]
		private static void P470() {}
		[MenuItem( "Assets/P480", false, 480)]
		private static void P480() {}
		[MenuItem( "Assets/P490", false, 490)]
		private static void P490() {}
		[MenuItem( "Assets/P500", false, 500)]
		private static void P500() {}
		[MenuItem( "Assets/P510", false, 510)]
		private static void P510() {}
		[MenuItem( "Assets/P520", false, 520)]
		private static void P520() {}
		[MenuItem( "Assets/P530", false, 530)]
		private static void P530() {}
		[MenuItem( "Assets/P540", false, 540)]
		private static void P540() {}
		[MenuItem( "Assets/P550", false, 550)]
		private static void P550() {}
		[MenuItem( "Assets/P560", false, 560)]
		private static void P560() {}
		[MenuItem( "Assets/P570", false, 570)]
		private static void P570() {}
		[MenuItem( "Assets/P580", false, 580)]
		private static void P580() {}
		[MenuItem( "Assets/P590", false, 590)]
		private static void P590() {}
		[MenuItem( "Assets/P600", false, 600)]
		private static void P600() {}
		[MenuItem( "Assets/P610", false, 610)]
		private static void P610() {}
		[MenuItem( "Assets/P620", false, 620)]
		private static void P620() {}
		[MenuItem( "Assets/P630", false, 630)]
		private static void P630() {}
		[MenuItem( "Assets/P640", false, 640)]
		private static void P640() {}
		[MenuItem( "Assets/P650", false, 650)]
		private static void P650() {}
		[MenuItem( "Assets/P660", false, 660)]
		private static void P660() {}
		[MenuItem( "Assets/P670", false, 670)]
		private static void P670() {}
		[MenuItem( "Assets/P680", false, 680)]
		private static void P680() {}
		[MenuItem( "Assets/P690", false, 690)]
		private static void P690() {}
		[MenuItem( "Assets/P700", false, 700)]
		private static void P700() {}
		[MenuItem( "Assets/P999", false, 999)]
		private static void P999() {}
		*/

	}

}
