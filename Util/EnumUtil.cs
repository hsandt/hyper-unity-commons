using System;

namespace CommonsHelper
{

	public static class EnumUtil {
    
		public static T[] GetValues<T>() {
			return Enum.GetValues(typeof(T)) as T[];
		}

	}
}

