using System.Runtime.CompilerServices;
using UnityEngine;

public class SplineBouncer
{
	public delegate void OnEvent(SplineBouncer bouncer);

	private SplineTracker m_tracker = new SplineTracker();

	private float m_yPosition;

	private float m_yVelocity;

	private float m_gravity;

	private float m_bounceRestitution;

	public float Speed
	{
		get
		{
			return m_tracker.TrackSpeed;
		}
		set
		{
			m_tracker.TrackSpeed = value;
		}
	}

	public float ObjectHeight { get; set; }

	public int BounceCount { get; private set; }

	public LightweightTransform CurrentTransform
	{
		get
		{
			LightweightTransform currentSplineTransform = m_tracker.CurrentSplineTransform;
			return new LightweightTransform(currentSplineTransform.Location + currentSplineTransform.Up * m_yPosition, currentSplineTransform.Orientation);
		}
	}

	[method: MethodImpl(32)]
	public event OnEvent OnBounce;

	public SplineBouncer(Track track, SplineTracker tracker, float speed, float initialForce, float gravity, float bounceRestitution)
		: this(track, initialForce, gravity, bounceRestitution)
	{
		m_tracker = new SplineTracker(tracker);
		m_tracker.TrackSpeed = speed;
	}

	public SplineBouncer(Track track, Spline targetSpline, float splinePos, float speed, float initialForce, float gravity, float bounceRestitution)
		: this(track, initialForce, gravity, bounceRestitution)
	{
		m_tracker.Target = targetSpline;
		m_tracker.Start(speed, splinePos, Direction_1D.Forwards);
	}

	private SplineBouncer(Track track, float initialForce, float gravity, float bounceRestitution)
	{
		m_tracker.OnStop += track.OnSplineStop;
		m_yPosition = 0f;
		m_yVelocity = initialForce * Time.smoothDeltaTime;
		m_gravity = ((!(gravity > 0f)) ? gravity : (0f - gravity));
		m_bounceRestitution = bounceRestitution;
		BounceCount = 0;
	}

	public void Update(float deltaTime)
	{
		UpdateBounce();
		m_tracker.UpdatePosition();
	}

	private void UpdateBounce()
	{
		m_yVelocity += m_gravity * Time.deltaTime;
		m_yPosition += m_yVelocity * Time.deltaTime;
		if (m_yPosition < ObjectHeight && m_yVelocity < 0f)
		{
			m_yVelocity *= 0f - m_bounceRestitution;
			m_yPosition = ObjectHeight + (ObjectHeight - m_yPosition);
			BounceCount++;
			if (this.OnBounce != null)
			{
				this.OnBounce(this);
			}
		}
	}
}
