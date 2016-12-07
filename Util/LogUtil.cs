using UnityEngine;
using System.Collections;

static public class LogUtil {

	public static string VectorToString(Vector2 vector)
	{
		return string.Format("({0}, {1})", vector.x, vector.y);
	}
}
