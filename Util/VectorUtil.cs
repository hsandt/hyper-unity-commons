using UnityEngine;
using System.Collections;

namespace Commons.Helper
{

	public static class VectorUtil
	{

		/// Return vector projected on direction vector
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

		/// Return vector projected orthogonally to normal
		public static Vector2 ProjectOrthogonal(Vector2 vector, Vector2 normal)
		{
			// q = v - p
			return vector - ProjectParallel(vector, normal);
			// alternative using Unity method:
			// return (Vector2) Vector3.ProjectOnPlane((Vector3) vector, (Vector3) normal);
		}

		/// Return vector rotated by 90 clockwise
		public static Vector2 Rotate(Vector2 vector, float angle)
		{
			return (Vector2) (Quaternion.AngleAxis(angle, Vector3.forward) * vector);
		}

		/// Return vector rotated by 90 clockwise
		public static Vector2 Rotate90CW(Vector2 vector)
		{
			return new Vector2(vector.y, -vector.x);
		}

		/// Return vector rotated by 90 counter-clockwise
		public static Vector2 Rotate90CCW(Vector2 vector)
		{
			return new Vector2(-vector.y, vector.x);
		}

		/// Return the closest point on a segment to another point
		public static Vector2 ClosestPointOnSegmentToPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
		{
			Vector2 segmentDelta = segmentEnd - segmentStart;
			float segmentSqrMagnitude = segmentDelta.sqrMagnitude;

			if (Mathf.Approximately(segmentSqrMagnitude, 0f))
			{
				// Segment is reduced to a point, closest point is trivial
				return segmentStart;
			}

			// Coordinate ratio r of point p on oriented segment e: r = <p - e[0], e> / ||e||^2 -> clamp between 0 and 1
			Vector2 vector = point - segmentStart;
			float ratio =
				Mathf.Clamp01(Vector2.Dot(vector, segmentDelta) /
								segmentSqrMagnitude); // immediately clamp to get segment start/end if point is "sided"
			return Vector2.Lerp(segmentStart, segmentEnd, ratio);
		}

		/// Return the distance between a point and a segment
		public static float PointToSegmentDistance(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
		{
			return Vector2.Distance(point, ClosestPointOnSegmentToPoint(segmentStart, segmentEnd, point));
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

			// Curvilinear abscissa, or parametric distance, of point p on oriented line e: r = <p - e[0], e> / ||e|| -> clamp between 0 and segment length
			Vector2 vector = point - segmentStart;
			paramDistance = Mathf.Clamp(Vector2.Dot(vector, segmentDelta) / segmentMagnitude, 0f, segmentMagnitude);
			return Vector2.Distance(point, Vector2.Lerp(segmentStart, segmentEnd, paramDistance / segmentMagnitude));
		}

	}

}
