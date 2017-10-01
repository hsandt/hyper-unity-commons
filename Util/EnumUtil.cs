using System;

public static class EnumUtil {
    
	public static T[] GetValues<T>() {
		return Enum.GetValues(typeof(T)) as T[];
	}

}