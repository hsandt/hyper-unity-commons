using UnityEngine;
using System.Collections;

public static class ColorUtil {

	public static Color invisibleWhite = new Color(1f, 1f, 1f, 0f);
	public static Color invisibleBlack = new Color(0f, 0f, 0f, 0f);  // DEPRECATED: use Color.clear
	public static Color orange = new Color32(233, 85, 0, 255); // HexToColor("e95500");
	public static Color gold = new Color(1f, 0.843f, 0f);
	public static Color pink = new Color(1f, 0.753f, 0.796f);
	public static Color purple = new Color(0.5f, 0f, 0.5f);

	// copied from Vexe Framework file /Vexe/Runtime/Libs/Helpers/RuntimeHelper.cs
	public static Color HexToColor(string hex)
	{
	    byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
	    byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
	    byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
	    return new Color32(r, g, b, 255);
	}

	// copied from Vexe Framework file /Vexe/Runtime/Libs/Helpers/RuntimeHelper.cs
	public static string ColorToHex(Color32 color)
	{
	    string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	    return hex;
	}

}
