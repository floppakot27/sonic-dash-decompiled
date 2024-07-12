using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMineDeployment : MonoBehaviour
{
	private BossMovementController m_movementController;

	private BossAnimationController m_animationController;

	private BossAudioController m_audioController;

	private BossVisualController m_visualController;

	private List<BossBattleSystem.AttackSettings> m_attackSettings;

	private SpawnPool m_spawnPool;

	private bool m_behaviourRunning;

	[SerializeField]
	private Transform m_mineSpawnPrefab;

	[SerializeField]
	private Transform m_mineSpawnPoint;

	[SerializeField]
	private float m_despawnTime = 5f;

	[SerializeField]
	private float m_splineDistanceFromSonic;

	[SerializeField]
	private float m_yDistanceFromSpline;

	[SerializeField]
	private float m_timeBetweenMineSpawn = 0.25f;

	[SerializeField]
	private float m_transitionAnimationTriggerTime = 1f;

	[SerializeField]
	private AnimationCurve m_directionCurveMovementToSonic;

	private void Awake()
	{
		m_movementController = GetComponent<BossMovementController>();
		m_animationController = GetComponent<BossAnimationController>();
		m_audioController = GetComponent<BossAudioController>();
		m_visualController = GetComponent<BossVisualController>();
	}

	private void OnEnable()
	{
	}

	public void StartBehaviour(float timeToGetIntoPosition)
	{
		StartCoroutine(performMineBehaviour(timeToGetIntoPosition));
		StartCoroutine(TriggerAnimation());
	}

	public void SetGameplay(List<BossBattleSystem.AttackSettings> attackSettings, SpawnPool spawnPool)
	{
		m_attackSettings = attackSettings;
		m_spawnPool = spawnPool;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private IEnumerator performMineBehaviour(float timeToGetIntoPosition)
	{
		m_behaviourRunning = true;
		BossMovementController.MovementParameters parameters = new BossMovementController.MovementParameters
		{
			m_splineDistanceFromSonic = m_splineDistanceFromSonic,
			m_destination = Vector3.up * m_yDistanceFromSpline,
			m_moveWithTracker = true,
			m_duration = timeToGetIntoPosition,
			m_faceMovementDirection = false,
			m_lane = Track.Lane.Middle
		};
		m_movementController.MoveToDestination(parameters, useDrift: true, snapToDestination: false);
		do
		{
			yield return null;
			float progress = m_movementController.MovementProgress;
			float lerpValue = m_directionCurveMovementToSonic.Evaluate(progress);
			Vector3 targetDirection = -m_movementController.GetTracker().CurrentSplineTransform.Forwards;
			Vector3 targetRotEuler = Quaternion.LookRotation(targetDirection, Vector3.up).eulerAngles;
			Vector3 movementDirection = m_movementController.GetTracker().CurrentSplineTransform.Forwards;
			Vector3 movementRotEuler = Quaternion.LookRotation(movementDirection, Vector3.up).eulerAngles;
			Vector3 currentEuler = Vector3.Lerp(movementRotEuler, targetRotEuler, lerpValue);
			m_movementController.SetOrientation(Quaternion.Euler(currentEuler) * Vector3.forward, trackerSpace: false);
		}
		while (m_attackSettings == null || m_movementController.MovementProgress < 1f);
		m_movementController.SetOrientation(-m_movementController.GetTracker().CurrentSplineTransform.Forwards);
		int attackCount = m_attackSettings.Count;
		if (attackCount > 0)
		{
			int attackIndex = 0;
			float maxSpeed = Sonic.Handling.StartSpeed;
			float startDistance = m_movementController.SplineDistanceTravelled;
			float preFireAnimDistanceRequired = maxSpeed * m_animationController.GetCharacterAnimationLength(BossAnim.PreFire);
			for (; attackIndex < attackCount; attackIndex++)
			{
				BossBattleSystem.AttackSettings attackSettings = m_attackSettings[attackIndex];
				int projectileCount = attackSettings.getProjectileCount();
				for (int ii = 0; ii < projectileCount; ii++)
				{
					m_animationController.StartFiring(attackSettings.Delay + m_timeBetweenMineSpawn * (float)ii);
				}
				float delayDistance = maxSpeed * attackSettings.Delay;
				float mineLayDistance = startDistance + delayDistance;
				float preFireAnimDistance = mineLayDistance - preFireAnimDistanceRequired;
				while (m_movementController.SplineDistanceTravelled < preFireAnimDistance)
				{
					yield return null;
				}
				while (m_movementController.SplineDistanceTravelled < mineLayDistance)
				{
					yield return null;
				}
				int numberOfMinesSpawnedThisFrame = 0;
				if (attackSettings.LeftLane)
				{
					SpawnMine(0, delayDistance, numberOfMinesSpawnedThisFrame++);
				}
				if (attackSettings.MiddleLane)
				{
					SpawnMine(1, delayDistance, numberOfMinesSpawnedThisFrame++);
				}
				if (attackSettings.RightLane)
				{
					SpawnMine(2, delayDistance, numberOfMinesSpawnedThisFrame++);
				}
				startDistance = mineLayDistance;
			}
		}
		m_behaviourRunning = false;
	}

	private IEnumerator TriggerAnimation()
	{
		yield return new WaitForSeconds(m_transitionAnimationTriggerTime);
		m_animationController.PlayAnimation(BossAnim.Taunt);
		m_audioController.PlayMineTransitionSFX();
	}

	private void SpawnMine(int lane, float mineLayDistance, int mineIndex)
	{
		SplineTracker tracker = m_movementController.GetTracker();
		Spline spline = tracker.Target.getTrackSegment().GetSpline(lane);
		Transform transform = m_spawnPool.Spawn(m_mineSpawnPrefab);
		m_spawnPool.Despawn(transform, m_despawnTime);
		BossMine component = transform.GetComponent<BossMine>();
		component.Setup(m_mineSpawnPoint.position, spline, (float)mineIndex * m_timeBetweenMineSpawn, (float)(2 - mineIndex) * m_timeBetweenMineSpawn);
	}
}
