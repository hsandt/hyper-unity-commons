using UnityEngine;
using System;
using System.Collections;

using NUnit.Framework;

namespace Commons.Helper
{

	[TestFixture]
	public class RectUtilTests {

	    Rect horizontalRect;
	    Rect verticalRect;
	    Rect farRect;

	    [OneTimeSetUp]
	    public void Init () {
	        horizontalRect = Rect.MinMaxRect(0f, 0f, 3f, 2f);
	        verticalRect = Rect.MinMaxRect(2f, 1f, 4f, 5f);
	        farRect = Rect.MinMaxRect(10f, 10f, 11f, 11f);
	    }

	    [Test]
	    public void Intersection_HorizontalRect_HorizontalRect_Square () {
	        Assert.AreEqual(horizontalRect, RectUtil.Intersection(horizontalRect, horizontalRect));
	    }

	    [Test]
	    public void Intersection_HorizontalRect_VerticalRect_Square () {
	        Assert.AreEqual(Rect.MinMaxRect(2f, 1f, 3f, 2f), RectUtil.Intersection(horizontalRect, verticalRect));
	    }

	    [Test]
	    public void Intersection_HorizontalRect_FarRect_Square () {
	        Rect invalidIntersection = RectUtil.Intersection(horizontalRect, farRect);
	        Assert.AreEqual(-1f, invalidIntersection.width);
	        Assert.AreEqual(-1f, invalidIntersection.height);
	    }

	    [Test]
	    public void MBR_HorizontalRect_HorizontalRect_BigVerticalRectangle () {
	        Assert.AreEqual(horizontalRect, RectUtil.MBR(horizontalRect, horizontalRect));
	    }

	    [Test]
	    public void MBR_HorizontalRect_VerticalRect_BigVerticalRectangle () {
	        Assert.AreEqual(Rect.MinMaxRect(0f, 0f, 4f, 5f), RectUtil.MBR(horizontalRect, verticalRect));
	    }

	}

}

