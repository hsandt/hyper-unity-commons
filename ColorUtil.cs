using UnityEngine;
using System.Collections;

public static class ColorUtil {

	public static Color invisible = new Color(1f, 1f, 1f, 0f);
	public static Color invisibleBlack = new Color(0f, 0f, 0f, 0f);
	public static Color invisibleYellow = new Color(1f, 0.92f, 0.016f, 0f);  // Unity custom yellow, with alpha 0
	public static Color visible = new Color(1f, 1f, 1f, 1f);  // alias for white
	public static Color orange = new Color(1f, 0.843f, 0f);
	public static Color pink = new Color(1f, 0.753f, 0.796f);
	public static Color purple = new Color(0.5f, 0f, 0.5f);

}
