using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMissileChase : MonoBehaviour
{
	private BossMovementController m_movementController;

	private BossAnimationController m_animationController;

	private BossVisualController m_visualController;

	private BossAudioController m_audioController;

	private List<BossBattleSystem.AttackSettings> m_attackSettings;

	private SpawnPool m_spawnPool;

	[SerializeField]
	private float m_splineDistanceFromSonic;

	[SerializeField]
	private float m_yDistanceFromSpline;

	[SerializeField]
	private float m_laneSwitchDuration = 0.25f;

	[SerializeField]
	private float m_laneSwitchPauseBeforeFiring = 0.5f;

	[SerializeField]
	private float m_distanceChangeDuration = 1f;

	[SerializeField]
	private Transform m_spawnPrefab;

	[SerializeField]
	private float m_missileOffsetAboveSpline = 1.3f;

	[SerializeField]
	private float m_missileSpeed = 50f;

	[SerializeField]
	private float m_missileAngularVelocity = 360f;

	[SerializeField]
	private float m_missileDespawnTime = 10f;

	[SerializeField]
	private Transform m_missileSpawnPoint;

	public static float s_trackerDistance;

	public static float s_trackerAttackDistance;

	public static int s_trackerAttackIndex;

	public static int s_trackerAttackCount;

	private void Awake()
	{
		m_movementController = GetComponent<BossMovementController>();
		m_animationController = GetComponent<BossAnimationController>();
		m_visualController = GetComponent<BossVisualController>();
		m_audioController = GetComponent<BossAudioController>();
	}

	private void OnEnable()
	{
	}

	public void SetGameplay(List<BossBattleSystem.AttackSettings> attackSettings, SpawnPool spawnPool)
	{
		m_attackSettings = attackSettings;
		m_spawnPool = spawnPool;
		if (m_attackSettings.Count > 0)
		{
			StartCoroutine(performAttackBehaviour());
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		m_animationController.ClearAllFiring();
	}

	private IEnumerator performAttackBehaviour()
	{
		BossMovementController.MovementParameters parameters = new BossMovementController.MovementParameters
		{
			m_splineDistanceFromSonic = m_splineDistanceFromSonic,
			m_destination = Vector3.up * m_yDistanceFromSpline,
			m_moveWithTracker = true,
			m_faceMovementDirection = false,
			m_lane = Track.Lane.Middle
		};
		m_movementController.MoveToDestination(parameters, useDrift: false, snapToDestination: true);
		yield return null;
		while (m_movementController.GetTracker() == null)
		{
			yield return null;
		}
		m_movementController.SetOrientation(m_movementController.GetTracker().CurrentSplineTransform.Forwards);
		m_movementController.UseDrift = true;
		int attackCount = m_attackSettings.Count;
		if (attackCount <= 0)
		{
			yield break;
		}
		int attackIndex = 0;
		float maxSpeed = Sonic.Handling.StartSpeed;
		float laneChangeTime = m_laneSwitchPauseBeforeFiring + m_laneSwitchDuration;
		float laneChangeDistance = laneChangeTime * maxSpeed;
		for (; attackIndex < attackCount; attackIndex++)
		{
			BossBattleSystem.AttackSettings attackSettings = m_attackSettings[attackIndex];
			if (m_movementController.SplineDistanceFromSonic != attackSettings.Distance)
			{
				parameters.m_duration = m_distanceChangeDuration;
				parameters.m_splineDistanceFromSonic = attackSettings.Distance;
				m_movementController.MoveToDestination(parameters, useDrift: true, snapToDestination: false);
				float distanceRequiredForMove = m_distanceChangeDuration * maxSpeed;
				float distanceToConinue = Sonic.Tracker.DistanceTravelled + distanceRequiredForMove;
				while (Sonic.Tracker.DistanceTravelled < distanceToConinue)
				{
					yield return null;
				}
			}
			float startDistance = Sonic.Tracker.DistanceTravelled;
			Track.Lane attackLane = ((!attackSettings.LeftLane) ? (attackSettings.MiddleLane ? Track.Lane.Middle : Track.Lane.Right) : Track.Lane.Left);
			bool laneChangeRequired = attackLane != parameters.m_lane;
			float timeToShot = attackSettings.Delay;
			if (attackIndex == 0)
			{
				timeToShot += m_animationController.GetCharacterAnimationLength(BossAnim.PreFireLong);
			}
			if (laneChangeRequired && timeToShot < laneChangeTime)
			{
				timeToShot = laneChangeTime;
			}
			m_animationController.StartFiring(timeToShot);
			float nextAttackDistance = startDistance + maxSpeed * timeToShot;
			if (laneChangeRequired)
			{
				float laneTransitionDistanceStart = nextAttackDistance - laneChangeDistance;
				while (Sonic.Tracker.DistanceTravelled < laneTransitionDistanceStart)
				{
					yield return null;
				}
			}
			parameters.m_lane = attackLane;
			parameters.m_duration = m_laneSwitchDuration;
			m_movementController.MoveToDestination(parameters, useDrift: true, snapToDestination: false);
			while (Sonic.Tracker.DistanceTravelled < nextAttackDistance)
			{
				yield return null;
			}
			SpawnMissile((int)attackLane);
		}
	}

	private void SpawnMissile(int lane)
	{
		SplineTracker tracker = m_movementController.GetTracker();
		Spline spline = tracker.Target.getTrackSegment().GetSpline(lane);
		Transform transform = m_spawnPool.Spawn(m_spawnPrefab);
		m_spawnPool.Despawn(transform, m_missileDespawnTime);
		BossMissile component = transform.GetComponent<BossMissile>();
		component.Fire(m_missileSpawnPoint.position, tracker, spline, m_missileOffsetAboveSpline, m_missileSpeed, m_missileAngularVelocity);
	}
}
