using UnityEngine;
using System.Collections;

public static class ColorUtil {

	public static Color invisible = new Color(1f, 1f, 1f, 0f);
	public static Color invisibleBlack = new Color(0f, 0f, 0f, 0f);
	public static Color invisibleYellow = new Color(1f, 0.92f, 0.016f, 0f);  // Unity custom yellow, with alpha 0
	public static Color visible = new Color(1f, 1f, 1f, 1f);  // alias for white
	public static Color orange = new Color(1f, 168f / 255f, 0f);
	public static Color yellow = new Color(1f, 0.843f, 0f);
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
