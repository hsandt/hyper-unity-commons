using UnityEngine;
using System;
using System.Collections;

namespace CommonsHelper
{

	public static class RectUtil {

	    /// Return the intersection (biggest contained rectangle) of two rectangles if not empty. Else, return a Rect of size (-1f, -1f).
	    /// UB unless the passed rectangles have a non-negative width and height.
	    public static Rect Intersection (Rect rect1, Rect rect2) {
	        #if UNITY_EDITOR || DEVELOPMENT_BUILD
	        if (!(rect1.width >= 0 && rect1.height >= 0 && rect2.width >= 0 && rect2.height >= 0)) {
	            throw new ArgumentException($"Passed rects are invalid: {rect1}, {rect2}");
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
	    /// Non-negative width and height are tolerated because we sometimes want to chain MBR to get the minimum
	    /// bounding box of 3+ rectangles, and we often initialize the MBR with reverse infinity values, i.e.
	    /// min = Vector2.positiveInfinity, max = Vector2.negativeInfinity (similarly to float min/max algorithms)
		/// This means that if you are handling non-infinite rectangles with negative width/height, the MBR
		/// may not even contain the "negative rect" as we won't flip the min and max coordinates to make them positive.
	    public static Rect MBR (Rect rect1, Rect rect2) {
	        Vector2 boundingRectMin = Vector2.Min(rect1.min, rect2.min);
	        Vector2 boundingRectMax = Vector2.Max(rect1.max, rect2.max);
	        Rect boundingRect = new Rect
	        {
		        min = boundingRectMin,
		        max = boundingRectMax
	        };
	        return boundingRect;
	    }

	    /// Return the minimum rectangle that contains both the passed rectangle and the passed point.
	    /// Equivalent to calling MBR with the passed rectangle and a second rectangle reduced to a point with
	    /// min = max = passed point.
	    /// UB unless the passed rectangle has a non-negative width and height.
	    public static Rect MBR (Rect rect, Vector2 point) {
	        Vector2 boundingRectMin = Vector2.Min(rect.min, point);
	        Vector2 boundingRectMax = Vector2.Max(rect.max, point);
	        Rect boundingRect = new Rect
	        {
		        min = boundingRectMin,
		        max = boundingRectMax
	        };
	        return boundingRect;
	    }

	}

}
