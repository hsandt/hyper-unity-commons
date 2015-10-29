using UnityEngine;
using System.Collections;

static public class StringUtil {

	// http://www.dotnetperls.com/uppercase-first-letter
	/// Make the first character of a string upper case
	public static string UppercaseFirst(string s)
	{
		if (string.IsNullOrEmpty(s)) return string.Empty;
		char[] a = s.ToCharArray();
		a[0] = char.ToUpper(a[0]);
		return new string(a);
	}
}
