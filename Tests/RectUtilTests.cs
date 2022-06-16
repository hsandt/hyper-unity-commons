using UnityEngine;
using System;
using System.Collections;

using NUnit.Framework;

namespace CommonsHelper.Tests
{

	[TestFixture]
	public class RectUtilTests {

	    Rect horizontalRect;
	    Rect verticalRect;
	    Rect verticalRectCenter;
	    Rect farRect;

	    [OneTimeSetUp]
	    public void Init () {
	        horizontalRect = Rect.MinMaxRect(0f, 0f, 3f, 2f);
	        verticalRect = Rect.MinMaxRect(2f, 1f, 4f, 5f);
	        verticalRectCenter = Rect.MinMaxRect(1f, -1f, 2f, 3f);
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
	    public void MBR_HorizontalRect_HorizontalRect_HorizontalRect () {
	        Assert.AreEqual(horizontalRect, RectUtil.MBR(horizontalRect, horizontalRect));
	    }

	    [Test]
	    public void MBR_HorizontalRect_VerticalRect_BigVerticalRectangle () {
	        Assert.AreEqual(Rect.MinMaxRect(0f, 0f, 4f, 5f), RectUtil.MBR(horizontalRect, verticalRect));
	    }

	    [Test]
	    public void MBR_VerticalRect_HorizontalRect_BigVerticalRectangle () {
	        Assert.AreEqual(Rect.MinMaxRect(0f, 0f, 4f, 5f), RectUtil.MBR(verticalRect, horizontalRect));
	    }

	    [Test]
	    public void MBR_HorizontalRect_VerticalRectCenter_BigVerticalRectangleCenter () {
	        Assert.AreEqual(Rect.MinMaxRect(0f, -1f, 3f, 3f), RectUtil.MBR(horizontalRect, verticalRectCenter));
	    }

	    [Test]
	    public void MBR_VerticalRectCenter_HorizontalRect_BigVerticalRectangleCenter () {
	        Assert.AreEqual(Rect.MinMaxRect(0f, -1f, 3f, 3f), RectUtil.MBR(verticalRectCenter, horizontalRect));
	    }

	    [Test]
	    public void MBR_HorizontalRect_PointInside_HorizontalRectangle () {
	        Assert.AreEqual(horizontalRect, RectUtil.MBR(horizontalRect, new Vector2(1f, 1f)));
	    }


	    [Test]
	    public void MBR_HorizontalRect_PointOnEdge_HorizontalRectangle () {
	        Assert.AreEqual(horizontalRect, RectUtil.MBR(horizontalRect, new Vector2(3f, 2f)));
	    }

	    [Test]
	    public void MBR_HorizontalRect_PointOutsideTopLeftSide_HorizontalRectangle () {
	        Assert.AreEqual(Rect.MinMaxRect(-0.1f, -0.1f, 3f, 2f), RectUtil.MBR(horizontalRect, new Vector2(-0.1f, -0.1f)));
	    }

	    [Test]
	    public void MBR_HorizontalRect_PointOutsideBottomRightSide_HorizontalRectangle () {
	        Assert.AreEqual(Rect.MinMaxRect(0f, 0f, 3.1f, 2.1f), RectUtil.MBR(horizontalRect, new Vector2(3.1f, 2.1f)));
	    }

	}

}

