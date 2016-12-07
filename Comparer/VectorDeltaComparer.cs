using UnityEngine;
using System.Collections.Generic;

public class VectorDeltaEqualityComparer : IEqualityComparer<Vector2>
{
	private readonly float _epsilon;

	public VectorDeltaEqualityComparer(float epsilon)
	{
		_epsilon = epsilon;
	}

	public bool Equals(Vector2 x, Vector2 y)
	{
		var a = (Vector2) x;
		var b = (Vector2) y;

		// ALTERNATIVE: imitate VectorComparerBase.cs in Unity Test Tools
		float delta = Vector2.Distance(a, b);
		return delta < _epsilon;
	}

	// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
	public int GetHashCode(Vector2 vector)
	{
	    unchecked // Overflow is fine, just wrap
	    {
	        int hash = 17;
	        hash = hash * 23 + vector.x.GetHashCode();
	        hash = hash * 23 + vector.y.GetHashCode();
	        return hash;
	    }
	}
}

public class VectorDeltaComparer : IComparer<Vector2>
{
	private readonly float _epsilon;

	public VectorDeltaComparer(float epsilon)
	{
		_epsilon = epsilon;
	}

	public int Compare(Vector2 x, Vector2 y)
	{
		var a = (Vector2) x;
		var b = (Vector2) y;

		float delta = Vector2.Distance(a, b);
		if (delta < _epsilon)
		{
			return 0;
		}

	// arbitrary comparison (use this class when you need to use an IComparer
	// instead of an IEqualityComparer but only need to test equality)
		if (a.x != b.x)
		return a.x.CompareTo(b.x);
		else
		return a.y.CompareTo(b.y);
	}
}
