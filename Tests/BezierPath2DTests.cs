using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CommonsHelper.Tests
{
    [TestFixture]
    public class BezierPath2DTests {

        BezierPath2D path;

        [SetUp]
        public void Init () {
            path = new BezierPath2D();
        }
    
        [Test]
        public void GetControlPointsCount_Initial()
        {
            Assert.AreEqual(4, path.GetControlPointsCount());
        }
        
        [Test]
        public void GetControlPointsCount_3KeyPoints()
        {
            path.AddKeyPoint(new Vector2(5f, 0));

            Assert.AreEqual(7, path.GetControlPointsCount());
        }
        
        [Test]
        public void SetControlPoint_KeyPoint1()
        {
            path.SetControlPoint(0, new Vector2(5f, 0));

            Assert.AreEqual(new Vector2(5f, 0), path.GetControlPoint(0));
        }
        
        [Test]
        public void SetControlPoint_ControlPoint1A()
        {
            path.SetControlPoint(1, new Vector2(0f, 10f));

            Assert.AreEqual(new Vector2(0f, 10), path.GetControlPoint(1));
        }

        [Test]
        public void AddKeyPoint_Add1KeyPointOnTheRight()
        {
            // arrange last control points
            path.SetControlPoint(2, new Vector2(2f, -1f));
            path.SetControlPoint(3, new Vector2(3f, 0f));

            // add new key point
            path.AddKeyPoint(new Vector2(5f, 0));
            
            // verify added control points
            Assert.AreEqual(new[] {
                new Vector2(4f, 1f),
                new Vector2(4f, 1f),
                new Vector2(5f, 0f)
            }, new[] {
                path.GetControlPoint(4),
                path.GetControlPoint(5),
                path.GetControlPoint(6)
            });
        }
        
        [Test]
        public void GetCurvesCount_Initial()
        {
            Assert.AreEqual(1, path.GetCurvesCount());
        }
        
        [Test]
        public void GetCurvesCount_3KeyPoints()
        {
            path.AddKeyPoint(new Vector2(5f, 0));
            
            Assert.AreEqual(2, path.GetCurvesCount());
        }
        
        [Test]
        public void GetCurve_0()
        {
            // arrange control points
            path.SetControlPoint(0, new Vector2(-2f, -1f));
            path.SetControlPoint(1, new Vector2(0f, -1f));
            path.SetControlPoint(2, new Vector2(0f, 1f));
            path.SetControlPoint(3, new Vector2(2f, 1f));
            
            Assert.AreEqual(new[] {
                new Vector2(-2f, -1f),
                new Vector2(0f, -1f),
                new Vector2(0f, 1f),
                new Vector2(2f, 1f)
            }, path.GetCurve(0));
        }
        
        [Test]
        public void GetCurve_1()
        {
            // arrange: prepare last control points and add new key point
            path.SetControlPoint(2, new Vector2(2f, -1f));
            path.SetControlPoint(3, new Vector2(2f, 0f));
            path.AddKeyPoint(new Vector2(5f, 0));
            
            // rearrange new control points in order to reduce the effect of AddKeyPoint on point positions in this test
            path.SetControlPoint(4, new Vector2(2f, 1f));
            path.SetControlPoint(5, new Vector2(5f, 1f));
            
            // verify last curve
            Assert.AreEqual(new[] {
                new Vector2(2f, 0f),
                new Vector2(2f, 1f),
                new Vector2(5f, 1f),
                new Vector2(5f, 0f)
            }, path.GetCurve(1));
        }
        
    }
}
