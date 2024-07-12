using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Enemies/Crabmeat")]
internal class Crabmeat : Enemy
{
	private class CrabmeatCollisionResolver : CollisionResolver
	{
		public CrabmeatCollisionResolver()
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
			else if (type == typeof(MotionRollState) || type == typeof(MotionAttackState) || type == typeof(MotionJumpState))
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

	public float m_laneChangeDuration = 3f;

	private Track.Lane m_lane = Track.Lane.Middle;

	public float m_nearRange = 4.5f;

	public float m_farRange = 20f;

	private Direction m_initialDir;

	public float m_movementDisallowDistance;

	private Animation m_animComponent;

	private bool m_isPlaced;

	[SerializeField]
	private string m_walkAnim = string.Empty;

	[SerializeField]
	private string m_attackAnim = string.Empty;

	[SerializeField]
	private string m_idleAnimationName = string.Empty;

	[SerializeField]
	private string m_preMovementAnimationName = string.Empty;

	public AnimationCurve m_movementCurve;

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public override void Start()
	{
		base.Start();
		InitAnimations();
		base.CollisionResolver = new CrabmeatCollisionResolver();
	}

	public void OnEnable()
	{
		if (m_isPlaced)
		{
			StartBehaviour();
		}
	}

	public void Update()
	{
		if (m_isPlaced && CurrentSpline == null)
		{
			DestroySelf();
		}
		else
		{
			updateMovementPermission(m_movementDisallowDistance);
		}
	}

	public override void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
		StopAllCoroutines();
		m_animComponent[m_attackAnim].wrapMode = WrapMode.Loop;
		m_animComponent.CrossFade(m_attackAnim, 0.25f);
	}

	public override void OnDeath(object[] onDeathParams)
	{
		StopAllCoroutines();
	}

	public override void OnStumble(SonicSplineTracker killer)
	{
	}

	protected override void DestroySelf()
	{
		m_isPlaced = false;
		base.DestroySelf();
	}

	protected override void Place(Track track, Spline spline, Direction initialDir)
	{
		InitAnimations();
		CurrentSpline = spline;
		Track = track;
		m_isPlaced = true;
		m_initialDir = initialDir;
		StartBehaviour();
	}

	private void StartBehaviour()
	{
		if (m_initialDir == Direction.Stationary)
		{
			string attackAnim = m_attackAnim;
			m_animComponent[attackAnim].wrapMode = WrapMode.Loop;
			m_animComponent.Play(attackAnim);
			m_lane = Track.GetLaneOfSpline(CurrentSpline);
		}
		else
		{
			m_animComponent.Stop();
			StartCoroutine(Patrol(m_initialDir));
			m_lane = Track.GetLaneOfSpline(CurrentSpline);
		}
	}

	private void InitAnimations()
	{
		m_animComponent = GetComponentInChildren<Animation>();
	}

	private IEnumerator Patrol(Direction initialDir)
	{
		Direction_2D currentDir = initialDir switch
		{
			Direction.ToPlayersRight => Direction_2D.Right, 
			Direction.ToPlayersLeft => Direction_2D.Left, 
			_ => Direction_2D.Forwards, 
		};
		int currentWaypointIndex = 0;
		Func<int> findNextWaypoint = () => (currentDir == Direction_2D.Right) ? ((currentWaypointIndex >= 2) ? 1 : (currentWaypointIndex + 1)) : ((currentWaypointIndex <= 0) ? 1 : (currentWaypointIndex - 1));
		WorldTransformLock[] patrolWaypoints = CalculatePatrolWaypoints(ref currentWaypointIndex);
		m_animComponent[m_idleAnimationName].wrapMode = WrapMode.Loop;
		m_animComponent.Play(m_idleAnimationName);
		while (!BeatGenerator.instance().prebeatThisFrame() || !m_movementPermitted)
		{
			yield return null;
		}
		m_animComponent[m_preMovementAnimationName].wrapMode = WrapMode.Loop;
		m_animComponent.Play(m_preMovementAnimationName);
		while (!BeatGenerator.instance().beatThisFrame() || !m_movementPermitted)
		{
			yield return null;
		}
		while (true)
		{
			if (m_frozen)
			{
				yield return null;
				continue;
			}
			if (CurrentSpline == null)
			{
				break;
			}
			int nextWaypointIndex = findNextWaypoint();
			if (!patrolWaypoints[nextWaypointIndex].IsValid || !patrolWaypoints[currentWaypointIndex].IsValid)
			{
				break;
			}
			WorldTransformLock currentWorldLock = patrolWaypoints[currentWaypointIndex];
			m_animComponent.Stop();
			m_animComponent[m_idleAnimationName].wrapMode = WrapMode.Loop;
			m_animComponent.Play(m_idleAnimationName);
			while (!BeatGenerator.instance().prebeatThisFrame() || !m_movementPermitted)
			{
				yield return null;
			}
			m_animComponent[m_preMovementAnimationName].wrapMode = WrapMode.Loop;
			m_animComponent.Play(m_preMovementAnimationName);
			while (!BeatGenerator.instance().beatThisFrame() || !m_movementPermitted)
			{
				yield return null;
			}
			WorldTransformLock targetWorldLock = patrolWaypoints[nextWaypointIndex];
			yield return StartCoroutine(Patrol_MoveToSpline(currentDir, currentWorldLock, targetWorldLock, nextWaypointIndex));
			currentWaypointIndex = nextWaypointIndex;
		}
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

	private IEnumerator Patrol_MoveToSpline(Direction_2D currentDir, WorldTransformLock fromLock, WorldTransformLock toLock, int nextWaypointIndex)
	{
		m_animComponent.Stop();
		m_animComponent.Play(m_walkAnim);
		float lerpPos = 0f;
		float timer = 0f;
		while (timer < m_laneChangeDuration)
		{
			if (m_frozen)
			{
				yield return null;
				continue;
			}
			if (!fromLock.IsValid || !toLock.IsValid)
			{
				yield break;
			}
			LightweightTransform fromTransform = fromLock.CurrentTransform;
			LightweightTransform toTransform = toLock.CurrentTransform;
			timer += Time.deltaTime;
			lerpPos = timer / m_laneChangeDuration;
			lerpPos = Mathf.Clamp(lerpPos, 0f, 1f);
			if (lerpPos > 0.5f)
			{
				switch (nextWaypointIndex)
				{
				case 0:
					m_lane = Track.Lane.Left;
					break;
				case 1:
					m_lane = Track.Lane.Middle;
					break;
				case 2:
					m_lane = Track.Lane.Right;
					break;
				}
			}
			float adjustedLerp = m_movementCurve.Evaluate(lerpPos);
			base.transform.position = Vector3.Lerp(fromTransform.Location, toTransform.Location, adjustedLerp);
			base.transform.rotation = CalculateRotationOnSpline(currentDir, fromTransform.Orientation);
			yield return null;
		}
		lerpPos = 0f;
	}

	private static Quaternion CalculateRotationOnSpline(Direction_2D heading, Quaternion splineRotation)
	{
		float angle = 180f;
		return splineRotation * Quaternion.AngleAxis(angle, Vector3.up);
	}

	private float CalculateMaxHeight()
	{
		Collider component = GetComponent<Collider>();
		return component.bounds.size.y;
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
		if (null != CurrentSpline)
		{
			return true;
		}
		return false;
	}

	public override Track.Lane getLane()
	{
		return m_lane;
	}

	public override float getNearRange()
	{
		return m_nearRange;
	}

	public override float getFarRange()
	{
		return m_farRange;
	}
}
