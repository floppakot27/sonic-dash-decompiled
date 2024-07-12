using System.Collections;
using UnityEngine;

public class BossMine : Hazard
{
	private class BossMineCollisionResolver : CollisionResolver
	{
		public BossMineCollisionResolver()
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

	private Renderer[] m_renderers;

	private SplineTracker m_tracker;

	private float m_distanceTravelled;

	[SerializeField]
	private float m_spawnRotationInDegrees = 360f;

	[SerializeField]
	private float m_spawnRotationDuration = 1f;

	[SerializeField]
	private float m_armingYDistanceFromSpline = 2f;

	[SerializeField]
	private float m_armingZDistanceFromSpawnPoint = -2f;

	[SerializeField]
	private float m_dropDuration = 1f;

	[SerializeField]
	private float m_finalYDistanceFromSpline = 1f;

	[SerializeField]
	private ParticleSystem m_armedParticleEffect;

	[SerializeField]
	private Transform m_objectToRotate;

	public override void Start()
	{
		base.Start();
		base.CollisionResolver = new BossMineCollisionResolver();
		m_renderers = GetComponentsInChildren<Renderer>();
		SetVisible(visible: false);
		Sonic.OnMovementCallback += OnSonicMovement;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		m_tracker = null;
		m_armedParticleEffect.Stop();
		StopAllCoroutines();
	}

	private void SetVisible(bool visible)
	{
		for (int i = 0; i < m_renderers.Length; i++)
		{
			m_renderers[i].enabled = visible;
		}
	}

	public void Setup(Vector3 spawnPoint, Spline spline, float preArmingPause, float preDroppingPause)
	{
		StartCoroutine(performBehaviour(spawnPoint, spline, preArmingPause, preDroppingPause));
	}

	private IEnumerator performBehaviour(Vector3 spawnPoint, Spline spline, float preArmingPause, float preDroppingPause)
	{
		m_distanceTravelled = 0f;
		m_tracker = new SplineTracker(Sonic.Tracker.InternalTracker);
		m_tracker.Start(spawnPoint, spline, 0f, Direction_1D.Forwards);
		m_tracker.UpdatePositionByDelta(m_armingZDistanceFromSpawnPoint);
		Vector3 spawnOffset = spawnPoint - m_tracker.CurrentSplineTransform.Location;
		float preArmingDistanceRequired = Sonic.Handling.StartSpeed * preArmingPause;
		float animationStartDistance = preArmingDistanceRequired;
		yield return null;
		while (m_distanceTravelled < animationStartDistance)
		{
			yield return null;
		}
		SetVisible(visible: true);
		Vector3 desiredArmingOffset = Vector3.up * m_armingYDistanceFromSpline;
		float armingDistanceRequired = Sonic.Handling.StartSpeed * m_spawnRotationDuration;
		float armDistance = animationStartDistance + armingDistanceRequired;
		do
		{
			yield return null;
			m_tracker.CurrentSplineTransform.ApplyTo(base.transform);
			float armingProgress = (m_distanceTravelled - animationStartDistance) / armingDistanceRequired;
			armingProgress = Mathf.Clamp01(armingProgress);
			Vector3 currentOffset = Vector3.Lerp(spawnOffset, desiredArmingOffset, armingProgress);
			Vector3 position = m_tracker.CurrentSplineTransform.Location + currentOffset;
			base.transform.position = position;
			float rotation = Mathf.Lerp(0f, m_spawnRotationInDegrees, armingProgress);
			m_objectToRotate.rotation = base.transform.rotation * Quaternion.AngleAxis(rotation, Vector3.forward);
			float scale = Mathf.Lerp(0f, 1f, armingProgress);
			base.transform.localScale = Vector3.one * scale;
		}
		while (m_distanceTravelled < armDistance);
		float preDroppingDistanceRequired = Sonic.Handling.StartSpeed * preDroppingPause;
		float droppingStartDistance = armDistance + preDroppingDistanceRequired;
		while (m_distanceTravelled < droppingStartDistance)
		{
			Vector3 position = m_tracker.CurrentSplineTransform.Location + desiredArmingOffset;
			base.transform.position = position;
			yield return null;
		}
		float droppingDistanceRequired = Sonic.Handling.StartSpeed * m_dropDuration;
		float droppedDistance = droppingStartDistance + droppingDistanceRequired;
		Vector3 desiredDroppedMineOffset = Vector3.up * m_finalYDistanceFromSpline;
		while (m_distanceTravelled < droppedDistance)
		{
			float dropProgress = (m_distanceTravelled - droppingStartDistance) / droppingDistanceRequired;
			dropProgress = Mathf.Clamp01(dropProgress);
			Vector3 currentOffset = Vector3.Lerp(desiredArmingOffset, desiredDroppedMineOffset, dropProgress);
			Vector3 position = m_tracker.CurrentSplineTransform.Location + currentOffset;
			base.transform.position = position;
			yield return null;
		}
		m_armedParticleEffect.Play();
	}

	private void OnSonicMovement(SonicSplineTracker.MovementInfo info)
	{
		if (m_tracker != null && !(info.Delta < 0f))
		{
			m_tracker.UpdatePositionByDelta(info.Delta);
			m_distanceTravelled += info.Delta;
		}
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
	}
}
