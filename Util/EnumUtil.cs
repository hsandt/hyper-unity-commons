using UnityEngine;
using System;
using System.Collections.Generic;

public static class EnumUtil {
	public static T[] GetValues<T>() {
		return Enum.GetValues(typeof(T)) as T[];
	}
}
