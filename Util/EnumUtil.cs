using UnityEngine;
using System;
using System.Collections.Generic;

public static class EnumUtil {
	public static IEnumerable<T> GetValues<T>() {
		// return Enum.GetValues(typeof(T)).Cast<T>();
		return Enum.GetValues(typeof(T)) as T[];
	}
}
