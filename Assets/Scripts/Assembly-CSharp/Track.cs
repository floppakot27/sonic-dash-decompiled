using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Track/Track")]
public class Track : MonoBehaviour
{
	public enum Lane
	{
		Left,
		Middle,
		Right
	}

	public static readonly int LaneCount = Utils.GetEnumCount<Lane>();

	[SerializeField]
	private Spline m_startSpline;

	private TrackInfo m_trackInfo;

	public virtual bool IsAvailable => true;

	public virtual Spline StartSpline => m_startSpline;

	public TrackInfo Info => m_trackInfo;

	public virtual void Awake()
	{
		WorldCollector.MarkAsMovable(base.gameObject);
		RegisterEvents();
		EnsureTrackInfoAvailable();
	}

	public virtual SplineUtils.SplineParameters GetSplineToSideOf(Spline fromSpline, LightweightTransform fromTransform, SideDirection toDirection)
	{
		IEnumerable<Spline> candidateSplines = from spline in GetParallelSplineCandidates(fromSpline)
			where spline != fromSpline
			select spline;
		Vector3 strafeDirection = fromTransform.Right * ((toDirection != SideDirection.Right) ? (-1f) : 1f);
		SplineUtils.TransformValidator customNarrowPhase = (LightweightTransform newSplineTransform) => Vector3.Dot(newSplineTransform.Location - fromTransform.Location, strafeDirection) >= 0.05f;
		return SplineUtils.FindNewSpline(candidateSplines, fromTransform.Location, 144f, customNarrowPhase, fromTransform.Forwards);
	}

	public SplineUtils.SplineParameters GetNearestSpline(Vector3 position, Vector3 forwards)
	{
		IEnumerable<Spline> parallelSplineCandidates = GetParallelSplineCandidates(null);
		SplineUtils.TransformValidator customNarrowPhase = (LightweightTransform newSplineTransform) => true;
		return SplineUtils.FindNewSpline(parallelSplineCandidates, position, 144f, customNarrowPhase, forwards);
	}

	public SplineUtils.SplineParameters GetSplineStartNearest(Vector3 position, float maxSqrError)
	{
		return GetSplineStartNearest(position, maxSqrError, Direction_1D.Forwards);
	}

	public SplineUtils.SplineParameters GetSplineStartNearest(Vector3 position, float maxSqrError, Direction_1D direction)
	{
		Spline[] componentsInChildren = GetComponentsInChildren<Spline>();
		return SplineUtils.FindBestSplineStart(componentsInChildren, position, maxSqrError, direction);
	}

	public void OnSplineStop(SplineTracker stoppedTracker)
	{
		if (stoppedTracker.IsVIP)
		{
			stoppedTracker.Target.OnEndTracking();
		}
		Spline nextSpline = GetNextSpline(stoppedTracker);
		if (nextSpline != null)
		{
			if (stoppedTracker.IsReversed)
			{
				stoppedTracker.Start(stoppedTracker.CurrentSplineTransform.Location, nextSpline, stoppedTracker.TrackSpeed, Direction_1D.Backwards);
			}
			else
			{
				stoppedTracker.Target = nextSpline;
				stoppedTracker.Start(stoppedTracker.TrackSpeed);
			}
			if (stoppedTracker.IsVIP)
			{
				stoppedTracker.Target.OnStartTracking();
			}
		}
	}

	protected virtual Spline GetNextSpline(SplineTracker stoppedTracker)
	{
		Direction_1D direction = (stoppedTracker.IsReversed ? Direction_1D.Backwards : Direction_1D.Forwards);
		SplineUtils.SplineParameters splineStartNearest = GetSplineStartNearest(stoppedTracker.CurrentSplineTransform.Location, 1f, direction);
		if (splineStartNearest.IsValid && splineStartNearest.Target != stoppedTracker.Target)
		{
			return splineStartNearest.Target;
		}
		return null;
	}

	public virtual Lane GetLaneOfSpline(Spline s)
	{
		return Lane.Middle;
	}

	public static float CalculateTrackPositionOfTracker(SplineTracker tracker)
	{
		Spline target = tracker.Target;
		TrackSegment trackSegment = target.getTrackSegment();
		if (target == trackSegment.MiddleSpline)
		{
			return trackSegment.TrackPosition + tracker.CurrentDistance;
		}
		Utils.ClosestPoint closestPoint = trackSegment.MiddleSpline.EstimateDistanceAlongSpline(tracker.CurrentSplineTransform.Location);
		return trackSegment.TrackPosition + closestPoint.LineDistance;
	}

	protected void EnsureTrackInfoAvailable()
	{
		m_trackInfo = new TrackInfo();
	}

	protected virtual IEnumerable<Spline> GetParallelSplineCandidates(Spline s)
	{
		return GetComponentsInChildren<Spline>();
	}

	protected virtual void RegisterEvents()
	{
	}
}
