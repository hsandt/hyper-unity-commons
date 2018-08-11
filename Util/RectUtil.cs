using UnityEngine;
using System;
using System.Collections;

namespace Commons.Helper
{

	public static class RectUtil {

	    /// Return the intersection (biggest contained rectangle) of two rectangles if not empty. Else, return a Rect of size (-1f, -1f).
	    /// UB unless the passed rectangles have a non-negative width and height.
	    public static Rect Intersection (Rect rect1, Rect rect2) {
	        #if UNITY_EDITOR || DEVELOPMENT_BUILD
	        if (!(rect1.width >= 0 && rect1.height >= 0 && rect2.width >= 0 && rect2.height >= 0)) {
	            throw new ArgumentException(string.Format("Passed rects are invalid: {0}, {1}", rect1, rect2));
	        }
	        #endif
	        float xMin = Mathf.Max(rect1.xMin, rect2.xMin);
	        float xMax = Mathf.Min(rect1.xMax, rect2.xMax);
			float yMin = Mathf.Max(rect1.yMin, rect2.yMin);
	        float yMax = Mathf.Min(rect1.yMax, rect2.yMax);
	        if (xMin <= xMax && yMin <= yMax)
	            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
	        else
	            return new Rect(Vector2.zero, - Vector2.one);
	    }

	    /// Return the Minimum Bounding Rectangle of two rectangles.
	    /// UB unless the passed rectangles have a non-negative width and height.
	    public static Rect MBR (Rect rect1, Rect rect2) {
	        #if UNITY_EDITOR || DEVELOPMENT_BUILD
	        if (!(rect1.width >= 0 && rect1.height >= 0 && rect2.width >= 0 && rect2.height >= 0)) {
	            throw new ArgumentException(string.Format("Passed rects are invalid: {0}, {1}", rect1, rect2));
	        }
	        #endif
	        float xMin = Mathf.Min(rect1.xMin, rect2.xMin);
	        float xMax = Mathf.Max(rect1.xMax, rect2.xMax);
	        float yMin = Mathf.Min(rect1.yMin, rect2.yMin);
	        float yMax = Mathf.Max(rect1.yMax, rect2.yMax);
	        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
	    }

	}

}
