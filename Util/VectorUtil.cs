using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CommonsHelper
{
	public static class VectorUtil
	{
		/// Return vector projected on direction vector (automatically normalized)
		/// UB unless direction is not close to Zero vector
		public static Vector2 ProjectParallel(Vector2 vector, Vector2 direction)
		{
			float directionSqrMagnitude = direction.sqrMagnitude;

			if (directionSqrMagnitude < Mathf.Epsilon)
			{
				throw ExceptionsUtil.CreateExceptionFormat("Cannot project on null direction");
			}

			// p = (<v, e> / ||e||^2) * e
			return Vector2.Dot(vector, direction) / directionSqrMagnitude * direction;
		}

		/// Return vector projected orthogonally to normal (automatically normalized)
		/// UB unless normal is not close to Zero vector
		public static Vector2 ProjectOrthogonal(Vector2 vector, Vector2 normal)
		{
			// q = v - p
			return vector - ProjectParallel(vector, normal);
			// alternative using Unity method, but normal not guaranteed to be normalized:
			// return (Vector2) Vector3.ProjectOnPlane((Vector3) vector, (Vector3) normal);
		}

		/// Return vector mirror about axis (automatically normalized) (opposite of Vector2.Reflect if axis is a unit vector)
		/// UB unless axis is not close to Zero vector
		public static Vector2 Mirror(Vector2 vector, Vector2 axis)
		{
			// s = p - q = v - 2q
			return vector - 2 * ProjectOrthogonal(vector, axis);
		}

		/// Return a 2D vector rotated by angle degrees
		public static Vector2 Rotate(Vector2 vector, float angle)
		{
			return (Vector2) (Quaternion.AngleAxis(angle, Vector3.forward) * vector);
		}

		/// Return a 3D vector rotated by angle degrees
		public static Vector3 Rotate(Vector3 vector, float angle)
		{
			return Quaternion.AngleAxis(angle, Vector3.forward) * vector;
		}

		/// Return vector rotated by 90 degrees clockwise
		public static Vector2 Rotate90CW(Vector2 vector)
		{
			return new Vector2(vector.y, -vector.x);
		}

		/// Return vector rotated by 90 degrees counter-clockwise (same as Vector2.Perpendicular but clearer name)
		public static Vector2 Rotate90CCW(Vector2 vector)
		{
			return new Vector2(-vector.y, vector.x);
		}

		/// Return the index of the point among `points` that is the nearest to passed `position`
		/// This is a helper method that works both with world and GUI coordinates, as long as all
		/// coordinates use the same reference and units.
		public static int IndexOfClosestPointAmongPoints(Vector2 position, List<Vector2> points)
		{
			int nearestKeyPointIndex = -1;
			float nearestKeyPointDistance = float.MaxValue;

			int keyPointsCount = points.Count;
			for (int keyIndex = 0; keyIndex < keyPointsCount; keyIndex++)
			{
				float distance = Vector2.SqrMagnitude(points[keyIndex] - position);
				if (distance < nearestKeyPointDistance)
				{
					nearestKeyPointIndex = keyIndex;
					nearestKeyPointDistance = distance;
				}
			}

			return nearestKeyPointIndex;
		}

		/// Return the closest point to the passed [point] on a segment defined by [segmentStart] and [segmentEnd]
		/// Set out [parameterRatio] to the fraction of the closest point's position along the segment,
		/// from start (0) to end (1).
		public static Vector2 PointToClosestPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, out float parameterRatio)
		{
			Vector2 segmentDelta = segmentEnd - segmentStart;
			float segmentSqrMagnitude = segmentDelta.sqrMagnitude;

			if (Mathf.Approximately(segmentSqrMagnitude, 0f))
			{
				// Segment is reduced to a point, closest point is trivial
				// We must choose a convention here, so say that parameterRatio = 0
				parameterRatio = 0f;
				return segmentStart;
			}

			// Coordinate ratio r of point p on oriented segment e: r = <p - e[0], e> / ||e||^2
			// Also clamp between 0 and 1 to make sure the resulting point is on the segment
			// (it will snap to start if 0, end if 1)
			Vector2 vector = point - segmentStart;
			float normalizedParam = Vector2.Dot(vector, segmentDelta) / segmentSqrMagnitude;
			parameterRatio = Mathf.Clamp01(normalizedParam);

			return Vector2.Lerp(segmentStart, segmentEnd, parameterRatio);
		}

		/// Return the closest point on a segment to another point
		[Obsolete("Use PointToClosestPointOnSegment instead, changing parameter order and passing " +
		          "out float parameterRatio even if you don't use it")]
		public static Vector2 ClosestPointOnSegmentToPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
		{
			return PointToClosestPointOnSegment(point, segmentStart, segmentEnd, out float _);
		}

		/// Return the distance between a point and a segment
		public static float PointToSegmentDistance(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
		{
			return PointToSegmentDistance(point, segmentStart, segmentEnd, out float _);
		}

		/// Return the distance between a point and a segment, and out the parametric distance of the closest position of the point on the segment
		public static float PointToSegmentDistance(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd,
			out float paramDistance)
		{
			Vector2 segmentDelta = segmentEnd - segmentStart;
			float segmentMagnitude = segmentDelta.magnitude;

			if (Mathf.Approximately(segmentMagnitude, 0f))
			{
				// Segment is reduced to a point, distance is trivial
				paramDistance = 0f; // or any number between 0 and 1, they all correspond to the same point
				return Vector2.Distance(point, segmentStart);
			}

			// Affine abscissa, or parametric distance, of point p on oriented line e: r = <p - e[0], e> / ||e|| -> clamp between 0 and segment length
			Vector2 vector = point - segmentStart;
			paramDistance = Mathf.Clamp(Vector2.Dot(vector, segmentDelta) / segmentMagnitude, 0f, segmentMagnitude);
			return Vector2.Distance(point, Vector2.Lerp(segmentStart, segmentEnd, paramDistance / segmentMagnitude));
		}

		/// Return a 2D vector with each coordinate rounded to a multiple of snapValue
		public static Vector2 RoundVector2(Vector2 vector, float snapValue)
		{
			Vector2 roundedPosition;
			roundedPosition.x = MathUtil.Round(vector.x, snapValue);
			roundedPosition.y = MathUtil.Round(vector.y, snapValue);
			return roundedPosition;
		}

		/// Return a 3D vector with each coordinate rounded to a multiple of snapValue
		public static Vector3 RoundVector3(Vector3 vector, float snapValue)
		{
			Vector3 roundedPosition;
			roundedPosition.x = MathUtil.Round(vector.x, snapValue);
			roundedPosition.y = MathUtil.Round(vector.y, snapValue);
			roundedPosition.z = MathUtil.Round(vector.z, snapValue);
			return roundedPosition;
		}

		/// Remap a value with an affine that maps tA => pA, tB => pB, and clamp the result to the segment [pA, pB]
		public static Vector2 Remap(float tA, float tB, Vector2 pA, Vector2 pB, float t)
		{
			float tDelta = tB - tA;

			if (tDelta == 0f)
			{
				#if UNITY_EDITOR
				Debug.LogErrorFormat("[MathUtil] Remap: tA and tB have same value {0}, cannot divide by 0, " +
					"fall back to pA {1}", tA, pA);
				#endif

				return pA;
			}

			return Vector2.Lerp(pA, pB, (t - tA) / tDelta);
		}
	}
}
