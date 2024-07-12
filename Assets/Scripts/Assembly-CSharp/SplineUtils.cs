using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SplineUtils
{
	[StructLayout(0, Size = 1)]
	public struct SplineParameters
	{
		public Spline Target { get; set; }

		public Direction_1D TravelDirection { get; set; }

		public float StartPosition { get; set; }

		public bool IsValid => Target != null;

		public void ApplyTo(SplineTracker tracker)
		{
			tracker.Target = Target;
			tracker.Start(tracker.TrackSpeed, StartPosition, TravelDirection);
		}
	}

	public delegate bool TransformValidator(LightweightTransform suggestedNewTransform);

	public static SplineParameters FindNewSpline(IEnumerable<Spline> candidateSplines, Vector3 currentPosition, float broadCutoffSqrDistance, TransformValidator customNarrowPhase, Vector3 parallelTestDirection)
	{
		Utils.ClosestPoint closestPoint = default(Utils.ClosestPoint);
		Spline spline = null;
		bool flag = false;
		foreach (Spline candidateSpline in candidateSplines)
		{
			Utils.ClosestPoint closestPoint2 = candidateSpline.EstimateDistanceAlongSpline(currentPosition);
			if (closestPoint2.SqrError > broadCutoffSqrDistance)
			{
				continue;
			}
			LightweightTransform transform = candidateSpline.GetTransform(closestPoint2.LineDistance);
			if (customNarrowPhase != null && !customNarrowPhase(transform))
			{
				continue;
			}
			float num = Vector3.Dot(parallelTestDirection, transform.Orientation * Vector3.forward);
			if (Mathf.Abs(num) <= 0.85f)
			{
				Debug.DrawRay(transform.Location + Vector3.up, parallelTestDirection, Color.red, 9999f);
				Debug.DrawRay(transform.Location + Vector3.up, transform.Orientation * Vector3.forward, Color.green, 9999f);
				continue;
			}
			bool flag2 = num < 0f;
			float num2 = ((!flag2) ? (candidateSpline.Length - closestPoint2.LineDistance) : closestPoint2.LineDistance);
			if (!(num2 < 0.5f) && (spline == null || closestPoint.SqrError > closestPoint2.SqrError))
			{
				closestPoint = closestPoint2;
				spline = candidateSpline;
				flag = flag2;
			}
		}
		SplineParameters result;
		if (spline != null)
		{
			SplineParameters splineParameters = default(SplineParameters);
			splineParameters.Target = spline;
			splineParameters.StartPosition = closestPoint.LineDistance;
			splineParameters.TravelDirection = (flag ? Direction_1D.Backwards : Direction_1D.Forwards);
			result = splineParameters;
		}
		else
		{
			result = default(SplineParameters);
		}
		return result;
	}

	public static SplineParameters FindBestSplineStart(IEnumerable<Spline> candidateSplines, Vector3 currentPosition, float maxSqrError)
	{
		return FindBestSplineStart(candidateSplines, currentPosition, maxSqrError, Direction_1D.Forwards);
	}

	public static SplineParameters FindBestSplineStart(IEnumerable<Spline> candidateSplines, Vector3 currentPosition, float maxSqrError, Direction_1D direction)
	{
		float num = 0f;
		Spline spline = null;
		Vector3 vector = currentPosition;
		vector.y = 0f;
		foreach (Spline candidateSpline in candidateSplines)
		{
			if (!candidateSpline.calculateIsDirty())
			{
				Vector3 vector2 = ((direction != 0) ? candidateSpline.GetTransform(candidateSpline.Length).Location : candidateSpline.GetTransform(0f).Location);
				Vector3 vector3 = vector2;
				vector3.y = 0f;
				float sqrMagnitude = (vector - vector3).sqrMagnitude;
				if (!(sqrMagnitude > maxSqrError) && (spline == null || num > sqrMagnitude))
				{
					spline = candidateSpline;
					num = sqrMagnitude;
				}
			}
		}
		SplineParameters result;
		if (spline == null)
		{
			result = default(SplineParameters);
		}
		else
		{
			SplineParameters splineParameters = default(SplineParameters);
			splineParameters.Target = spline;
			splineParameters.StartPosition = 0f;
			splineParameters.TravelDirection = Direction_1D.Forwards;
			result = splineParameters;
		}
		return result;
	}
}
