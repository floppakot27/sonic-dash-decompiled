using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Enemies/Chopper")]
internal class Chopper : Enemy
{
	private class ChopperCollisionResolver : CollisionResolver
	{
		public ChopperCollisionResolver()
			: base(ResolutionType.SonicDeath)
		{
		}

		public override void ProcessMotionState(MotionState state, bool heldRings, bool ghosted)
		{
			Type type = state.GetType();
			if (ghosted)
			{
				base.Resolution = ResolutionType.Nothing;
			}
			else if (type == typeof(MotionRollState) || type == typeof(MotionJumpState) || type == typeof(MotionAttackState))
			{
				base.Resolution = ResolutionType.EnemyDeath;
			}
			else if (heldRings)
			{
				base.Resolution = ResolutionType.SonicStumble;
			}
			else
			{
				base.Resolution = ResolutionType.SonicDeath;
			}
		}
	}

	private TargetManager m_targetManager;

	public float m_triggerRange = 30f;

	public float m_flightDuration = 2f;

	public float m_amountToMoveAlongTrack = 10f;

	public float m_halfArcDegrees = 135f;

	public float m_arcRadius = 5f;

	public float m_arcHeightAdjust = 0.6f;

	public float m_attackWindowTime = 0.5f;

	private GameObject m_chopperGO;

	private bool m_animating;

	private SplineTracker m_tracker;

	public AnimationCurve m_animationCurve;

	public AnimationCurve m_jumpCurve;

	private Direction m_direction = Direction.ToPlayersLeft;

	public float m_nearRange = 4.5f;

	public float m_farRange = 20f;

	public float m_modelScale = 2f;

	private static GameObject m_worldGameObject;

	private bool m_activeInTargetManager;

	private Animation m_animComponent;

	private bool m_isPlaced;

	[SerializeField]
	private string m_attackAnimName = string.Empty;

	[SerializeField]
	private string m_swimAnimName = string.Empty;

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public void Awake()
	{
		m_animComponent = GetComponentInChildren<Animation>();
		m_activeInTargetManager = false;
	}

	private void OnSpawned()
	{
		StopAllCoroutines();
		m_animComponent.Stop();
		m_isPlaced = false;
		m_animating = false;
		m_frozen = false;
		m_activeInTargetManager = false;
		base.transform.localScale = new Vector3(m_modelScale, m_modelScale, m_modelScale);
	}

	private void OnEnable()
	{
		if (m_isPlaced)
		{
			StartCoroutine(Patrol());
		}
	}

	public override void Place(OnEvent onDestroy, Track onTrack, Spline onSpline)
	{
		base.Place(onDestroy, onTrack, onSpline);
	}

	public override void Start()
	{
		base.Start();
		base.CollisionResolver = new ChopperCollisionResolver();
		Sonic.OnMovementCallback += OnSonicMovement;
	}

	public void Update()
	{
		if (!m_animating && m_isPlaced && CurrentSpline == null)
		{
			DestroySelf();
		}
	}

	protected override void DestroySelf()
	{
		if (m_activeInTargetManager)
		{
			TargetManager.instance().NotifyChopperInactive();
			m_activeInTargetManager = false;
		}
		m_targetManager.removeTarget(this);
		m_isPlaced = false;
		base.DestroySelf();
	}

	public override void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
		StopAllCoroutines();
		m_animComponent[m_attackAnimName].wrapMode = WrapMode.Loop;
		m_animComponent.CrossFade(m_attackAnimName, 0.25f);
	}

	public override void OnDeath(object[] onDeathParams)
	{
		if (m_activeInTargetManager)
		{
			TargetManager.instance().NotifyChopperInactive();
			m_activeInTargetManager = false;
		}
		m_targetManager.removeTarget(this);
		StopAllCoroutines();
	}

	public override void OnStumble(SonicSplineTracker killer)
	{
	}

	private void findChildGOs()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.gameObject.name == "Chopper")
			{
				m_chopperGO = child.gameObject;
				for (int j = 0; j < child.gameObject.transform.childCount; j++)
				{
					Transform child2 = child.gameObject.transform.GetChild(j);
					if (child2.gameObject.name == "Chopper")
					{
						m_chopperGO = child.gameObject;
					}
				}
			}
			else if (!(child.gameObject.name == "SFX"))
			{
			}
		}
	}

	private void makeInvisible()
	{
		m_chopperGO.SetActive(value: false);
	}

	private void makeVisible()
	{
		m_chopperGO.SetActive(value: true);
	}

	protected override void Place(Track track, Spline spline, Direction dir)
	{
		if (m_worldGameObject == null)
		{
			m_worldGameObject = GameObject.Find("World");
		}
		m_direction = dir;
		CurrentSpline = spline;
		Track = track;
		m_isPlaced = true;
		m_attackable = false;
		findChildGOs();
		StartCoroutine(Patrol());
		m_targetManager = TargetManager.instance();
		m_targetManager.addTarget(this);
	}

	private void OnSonicMovement(SonicSplineTracker.MovementInfo info)
	{
		if (m_animating && m_tracker != null)
		{
			if (!Sonic.MotionMonitor.isMoving())
			{
			}
			if (m_tracker.UpdatePositionByDelta(0f))
			{
				CurrentSpline = m_tracker.Target;
			}
			else
			{
				m_animating = false;
			}
		}
	}

	private IEnumerator Patrol()
	{
		m_activeInTargetManager = false;
		makeInvisible();
		Spline[] localSplines = CurrentSpline.transform.parent.GetComponentsInChildren<Spline>();
		Spline middleSpline = localSplines.ElementAt(1);
		Utils.ClosestPoint closestPoint = middleSpline.EstimateDistanceAlongSpline(base.transform.position);
		GameObject worldGO = GameObject.Find("World");
		WorldTransformLock worldSpawnPosition = new WorldTransformLock(worldGO, middleSpline.GetTransform(closestPoint.LineDistance));
		m_tracker = new SplineTracker();
		m_tracker.TrackerType = SplineTracker.Type.OneShot;
		m_tracker.Target = middleSpline;
		m_tracker.Start(0f, closestPoint.LineDistance, Direction_1D.Forwards);
		m_tracker.OnStop += Sonic.Tracker.Track.OnSplineStop;
		m_animComponent.Play();
		AnimationClip clip = m_animComponent.GetClip(m_swimAnimName);
		clip.wrapMode = WrapMode.Loop;
		m_animComponent.CrossFade(m_swimAnimName, 0f);
		bool finished = false;
		while (!finished)
		{
			finished = !TutorialSystem.instance().isTrackTutorialEnabled();
			while (true)
			{
				if (GameState.GetMode() == GameState.Mode.Game)
				{
					SonicSplineTracker sonicSplineTracker = Sonic.Tracker;
					float sonicTrackPosition = sonicSplineTracker.TrackPosition;
					if (sonicSplineTracker.InternalTracker != null && !sonicSplineTracker.IsDead() && !sonicSplineTracker.IsFalling() && !sonicSplineTracker.InternalTracker.IsReversed && base.TrackDistance - sonicTrackPosition < m_triggerRange)
					{
						break;
					}
				}
				yield return null;
			}
			TargetManager.instance().NotifyChopperActive();
			m_activeInTargetManager = true;
			float time = 0f;
			float animationDuration = m_flightDuration;
			makeVisible();
			m_animating = true;
			while (true)
			{
				float sonicTrackPosition = Sonic.Tracker.TrackPosition;
				if (base.TrackDistance - sonicTrackPosition < -10f && m_activeInTargetManager)
				{
					TargetManager.instance().NotifyChopperInactive();
					m_activeInTargetManager = false;
				}
				if (!worldSpawnPosition.IsValid)
				{
					if (m_activeInTargetManager)
					{
						TargetManager.instance().NotifyChopperInactive();
						m_activeInTargetManager = false;
					}
					yield break;
				}
				if (m_frozen)
				{
					finished = true;
					yield return null;
					continue;
				}
				float progression = time / animationDuration;
				progression = m_animationCurve.Evaluate(progression);
				float attackableStartTime = animationDuration / 2f - m_attackWindowTime * 0.5f;
				float attackableEndTime = animationDuration / 2f + m_attackWindowTime * 0.5f;
				if (time >= attackableStartTime && time <= attackableEndTime)
				{
					m_attackable = true;
				}
				else
				{
					m_attackable = false;
				}
				float waterHeightBlend = 0f;
				if (time < attackableStartTime)
				{
					float initialJumpProgress = time / attackableStartTime;
					waterHeightBlend = 1f - initialJumpProgress;
				}
				else if (time > attackableEndTime)
				{
					float endSectionDuration = animationDuration - attackableEndTime;
					float endJumpProgress = (time - attackableEndTime) / endSectionDuration;
					waterHeightBlend = endJumpProgress;
				}
				waterHeightBlend = m_jumpCurve.Evaluate(waterHeightBlend);
				waterHeightBlend = Mathf.Clamp(waterHeightBlend, 0f, 1f);
				float invWaterHeightBlend = 1f - waterHeightBlend;
				float arcDegrees = m_halfArcDegrees;
				float angle = 0f - arcDegrees + 2f * arcDegrees * progression;
				angle = Mathf.Clamp(angle, -90f, 90f);
				if (m_direction == Direction.ToPlayersRight)
				{
					angle *= -1f;
				}
				Quaternion rotation = Quaternion.AngleAxis(angle, m_tracker.CurrentSplineTransform.Forwards);
				float radius = m_arcRadius;
				Vector3 offset = m_tracker.CurrentSplineTransform.Up * radius;
				offset = rotation * offset;
				if (offset.y > 0f)
				{
					offset.y *= m_arcHeightAdjust;
				}
				Vector3 rotatedRight = rotation * m_tracker.CurrentSplineTransform.Right;
				Vector3 rotatedUp = offset.normalized;
				Vector3 currentPosition = m_tracker.CurrentSplineTransform.Location;
				float submergeAmount = 20f;
				float waterHeight = m_worldGameObject.transform.position.y - 9f - submergeAmount;
				currentPosition.y = waterHeightBlend * waterHeight + invWaterHeightBlend * currentPosition.y;
				if (m_direction == Direction.ToPlayersRight)
				{
					rotatedRight *= -1f;
				}
				base.transform.position = currentPosition + offset;
				base.transform.rotation = Quaternion.LookRotation(-rotatedRight, rotatedUp);
				time += Time.deltaTime;
				if (time > animationDuration)
				{
					break;
				}
				yield return null;
			}
		}
		DestroySelf();
	}

	private WorldTransformLock[] CalculatePatrolWaypoints(ref int currentWaypointIndex)
	{
		WorldTransformLock[] array = new WorldTransformLock[3];
		Spline[] componentsInChildren = CurrentSpline.transform.parent.GetComponentsInChildren<Spline>();
		for (int i = 0; i < 3; i++)
		{
			Spline spline = componentsInChildren.ElementAt(i);
			Utils.ClosestPoint closestPoint = spline.EstimateDistanceAlongSpline(base.transform.position);
			ref WorldTransformLock reference = ref array[i];
			reference = new WorldTransformLock(spline, spline.GetTransform(closestPoint.LineDistance));
			if (spline == CurrentSpline)
			{
				currentWaypointIndex = i;
			}
		}
		return array;
	}

	private static Quaternion CalculateRotationOnSpline(Direction_2D heading, Quaternion splineRotation)
	{
		float angle = 180f;
		return splineRotation * Quaternion.AngleAxis(angle, Vector3.up);
	}

	public override bool isAttackableFromAir()
	{
		return true;
	}

	public override Vector3 getTargetPosition()
	{
		return base.gameObject.transform.position + Vector3.up * 0.5f;
	}

	public override Spline getSpline()
	{
		return CurrentSpline;
	}

	public override void beginAttack()
	{
		m_frozen = true;
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = false;
		}
	}

	public override void endAttack()
	{
		m_frozen = false;
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = true;
		}
	}

	public override bool isLaneValid()
	{
		return true;
	}

	public override Track.Lane getLane()
	{
		return Sonic.Tracker.getLane();
	}

	public override float getNearRange()
	{
		return m_nearRange;
	}

	public override float getFarRange()
	{
		return m_farRange;
	}

	public override bool isSlowdownTarget()
	{
		return true;
	}

	public override bool isChopper()
	{
		return true;
	}

	public bool isAnimating()
	{
		return m_animating;
	}
}
