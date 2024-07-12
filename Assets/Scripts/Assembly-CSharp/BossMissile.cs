using UnityEngine;

public class BossMissile : Hazard
{
	private class BossMissileCollisionResolver : CollisionResolver
	{
		public BossMissileCollisionResolver()
			: base(ResolutionType.SonicDeath)
		{
		}

		public override void ProcessMotionState(MotionState state, bool heldRings, bool ghosted)
		{
			if (ghosted)
			{
				base.Resolution = ResolutionType.Nothing;
			}
			else if (heldRings)
			{
				base.Resolution = ResolutionType.SonicStumble;
			}
			else
			{
				base.Resolution = ResolutionType.SonicDieForwards;
			}
		}
	}

	private float m_angle;

	private float m_angularVelocity;

	private float m_speed;

	private SplineTracker m_tracker;

	private Vector3 m_offsetFromSpline = Vector3.zero;

	private Vector3 m_localOffsetToSpawnPoint = Vector3.zero;

	private float m_timer;

	[SerializeField]
	private float m_growDuration = 1f;

	[SerializeField]
	private float m_transitionToSplineDuration = 1f;

	[SerializeField]
	private Transform m_objectToRotate;

	public override void Start()
	{
		base.Start();
		base.CollisionResolver = new BossMissileCollisionResolver();
	}

	private void OnEnable()
	{
		m_angle = 0f;
		m_tracker = null;
	}

	private void OnDisable()
	{
		m_tracker = null;
		StopAllCoroutines();
	}

	public void Fire(Vector3 spawnPoint, SplineTracker tracker, Spline spline, float offsetAboveSpline, float speed, float angularVelocity)
	{
		m_tracker = new SplineTracker(tracker);
		m_tracker.Start(m_tracker.CurrentSplineTransform.Location, spline, speed);
		m_angle = 0f;
		m_angularVelocity = angularVelocity;
		m_offsetFromSpline = new Vector3(0f, offsetAboveSpline, 0f);
		m_localOffsetToSpawnPoint = spawnPoint - m_tracker.CurrentSplineTransform.Location;
		m_timer = 0f;
		UpdateTransform(0f);
	}

	public override void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
		Boss.GetInstance().AudioController.PlayHitCharacterSFX();
	}

	public override void OnDeath(object[] onDeathParams)
	{
	}

	public override void OnStumble(SonicSplineTracker killer)
	{
		Boss.GetInstance().AudioController.PlayHitCharacterSFX();
	}

	private void Update()
	{
		UpdateTransform(Time.deltaTime);
		m_timer += Time.deltaTime;
	}

	private void UpdateTransform(float deltaTime)
	{
		m_speed = m_tracker.TrackSpeed;
		float delta = m_speed * deltaTime;
		m_tracker.UpdatePositionByDelta(delta);
		LightweightTransform currentSplineTransform = m_tracker.CurrentSplineTransform;
		float value = m_timer / Mathf.Max(m_transitionToSplineDuration, 0.001f);
		value = Mathf.Clamp01(value);
		Vector3 vector = Vector3.Lerp(m_localOffsetToSpawnPoint, m_offsetFromSpline, value);
		currentSplineTransform.Location += vector;
		currentSplineTransform.ApplyTo(base.transform);
		if ((bool)m_objectToRotate)
		{
			m_objectToRotate.localRotation = Quaternion.AngleAxis(m_angle, Vector3.forward);
			m_angle += m_angularVelocity * deltaTime;
			float value2 = m_timer / Mathf.Max(m_growDuration, 0.001f);
			value2 = Mathf.Clamp01(value2);
			m_objectToRotate.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, value2);
		}
	}
}
