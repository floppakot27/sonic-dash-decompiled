using System;
using UnityEngine;

public class CameraTypeTrackSonic : CameraType
{
	[SerializeField]
	private float m_lateralAngle;

	[SerializeField]
	private float m_distance = 10f;

	[SerializeField]
	private float m_upDistance = 3f;

	[SerializeField]
	private float m_lookForwardDistance = 2f;

	[SerializeField]
	private float m_lookUpDistance = 2f;

	[SerializeField]
	private bool m_smoothingEnabled;

	[SerializeField]
	private float m_lookAtSmoothing = 1f;

	private SplineTracker m_tracker;

	private Vector3 m_lookAtVelocity = Vector3.zero;

	public override bool EnableSmoothing => m_smoothingEnabled;

	public void Awake()
	{
		Sonic.OnMovementCallback += OnSonicMovement;
	}

	public override void onActive()
	{
		m_tracker = new SplineTracker(Sonic.Tracker.InternalTracker);
		Spline target = Sonic.Tracker.InternalTracker.Target;
		Spline middleSpline = target.getTrackSegment().MiddleSpline;
		m_tracker.Start(Sonic.Transform.position, middleSpline, 0f, Direction_1D.Forwards);
		base.CachedLookAt = Sonic.Transform.position;
	}

	public override void onInactive()
	{
		m_tracker = null;
	}

	private void LateUpdate()
	{
		if (!(Sonic.Transform == null) && m_tracker != null)
		{
			Vector3 position = Sonic.Transform.position;
			position += m_tracker.CurrentSplineTransform.Forwards * m_lookForwardDistance;
			position += Vector3.up * m_lookUpDistance;
			float f = (float)Math.PI / 180f * m_lateralAngle;
			Vector3 direction = new Vector3(Mathf.Sin(f) * m_distance, m_upDistance, Mathf.Cos(f) * m_distance);
			direction = Sonic.Transform.TransformDirection(direction);
			base.transform.position = m_tracker.CurrentSplineTransform.Location + direction;
			base.CachedLookAt = Vector3.SmoothDamp(base.CachedLookAt, position, ref m_lookAtVelocity, m_lookAtSmoothing);
			base.transform.LookAt(base.CachedLookAt, Vector3.up);
		}
	}

	private void OnSonicMovement(SonicSplineTracker.MovementInfo info)
	{
		if (m_tracker != null && !(info.Delta < 0f))
		{
			m_tracker.UpdatePositionByDelta(info.Delta);
		}
	}
}
