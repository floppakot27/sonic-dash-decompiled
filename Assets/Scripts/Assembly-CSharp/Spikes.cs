using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Enemies/Spikes")]
internal class Spikes : Enemy
{
	private class SpikesCollisionResolver : CollisionResolver
	{
		public SpikesCollisionResolver()
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
			else if (type == typeof(MotionRollState))
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

	public float m_movementDisallowDistance;

	private bool m_lockedInPlace;

	private Animation m_animComponent;

	private bool m_isPlaced;

	private Direction m_initialDir;

	[SerializeField]
	private string m_turnLeftAnimName = "Spk_TurnLeft90";

	[SerializeField]
	private string m_turnRightAnimName = "Spk_TurnRight90";

	[SerializeField]
	private string m_attackAnim = string.Empty;

	[SerializeField]
	private string m_crawlAnim = string.Empty;

	public AnimationCurve m_movementCurve;

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public override void Start()
	{
		base.Start();
		InitAnimations();
		base.CollisionResolver = new SpikesCollisionResolver();
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

	public void OnSpawn()
	{
	}

	public void OnEnable()
	{
		if (m_isPlaced)
		{
			StartBehaviour();
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
		m_lockedInPlace = false;
	}

	private void InitAnimations()
	{
		m_animComponent = GetComponentInChildren<Animation>();
	}

	private void StartBehaviour()
	{
		if (m_initialDir == Direction.Stationary)
		{
			m_animComponent.Stop();
			m_lane = Track.GetLaneOfSpline(CurrentSpline);
			StationaryPatrol();
		}
		else
		{
			m_animComponent.Stop();
			m_lane = Track.GetLaneOfSpline(CurrentSpline);
			StartCoroutine(Patrol(m_initialDir));
		}
	}

	private void StationaryPatrol()
	{
		string attackAnim = m_attackAnim;
		m_animComponent[attackAnim].wrapMode = WrapMode.Loop;
		m_animComponent.Play(attackAnim);
		int currentWaypointIndex = 0;
		WorldTransformLock[] array = CalculatePatrolWaypoints(ref currentWaypointIndex);
		WorldTransformLock worldTransformLock = array[currentWaypointIndex];
		LightweightTransform currentTransform = worldTransformLock.CurrentTransform;
		base.transform.rotation = CalculateRotationOnSpline(Direction_2D.Forwards, currentTransform.Orientation);
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
		Action<WorldTransformLock> followTracker = delegate(WorldTransformLock worldLock)
		{
			if (worldLock.IsValid)
			{
				LightweightTransform currentTransform = worldLock.CurrentTransform;
				base.transform.position = currentTransform.Location;
				base.transform.rotation = CalculateRotationOnSpline(currentDir, currentTransform.Orientation);
			}
		};
		WorldTransformLock[] patrolWaypoints = CalculatePatrolWaypoints(ref currentWaypointIndex);
		m_animComponent[m_attackAnim].wrapMode = WrapMode.Loop;
		m_animComponent.Play(m_attackAnim);
		while (!BeatGenerator.instance().beatThisFrame() || !m_movementPermitted)
		{
			if (!m_movementPermitted && !m_lockedInPlace)
			{
				string animName = m_attackAnim;
				m_animComponent[animName].wrapMode = WrapMode.Loop;
				m_animComponent.Play(animName);
				float timer = 0.6f;
				while (timer > 0f)
				{
					float progress = 1f - timer / 0.6f;
					progress = Mathf.Clamp(progress, 0f, 1f);
					WorldTransformLock currentWorldLock = patrolWaypoints[currentWaypointIndex];
					Quaternion destinationRotation = CalculateRotationOnSpline(Direction_2D.Forwards, currentWorldLock.CurrentTransform.Orientation);
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, destinationRotation, progress);
					timer -= Time.deltaTime;
					yield return null;
				}
				m_lockedInPlace = true;
			}
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
			Direction_2D newDir = ((nextWaypointIndex >= currentWaypointIndex) ? Direction_2D.Right : Direction_2D.Left);
			m_animComponent.Stop();
			if (newDir != currentDir)
			{
				string animName = GetAnimName(currentDir, newDir);
				m_animComponent[animName].wrapMode = WrapMode.Once;
				m_animComponent.Play(animName);
				m_animComponent.Sample();
				while (m_animComponent.isPlaying)
				{
					followTracker(currentWorldLock);
					yield return null;
				}
				currentDir = newDir;
			}
			string idleAnimationName = m_attackAnim;
			m_animComponent[idleAnimationName].wrapMode = WrapMode.Loop;
			m_animComponent.Play(idleAnimationName);
			while (!BeatGenerator.instance().beatThisFrame() || !m_movementPermitted)
			{
				if (!m_movementPermitted && !m_lockedInPlace)
				{
					string animName = m_attackAnim;
					m_animComponent[animName].wrapMode = WrapMode.Loop;
					m_animComponent.Play(animName);
					float timer = 0.6f;
					while (timer > 0f)
					{
						float progress = 1f - timer / 0.6f;
						progress = Mathf.Clamp(progress, 0f, 1f);
						Quaternion destinationRotation = CalculateRotationOnSpline(Direction_2D.Forwards, currentWorldLock.CurrentTransform.Orientation);
						base.transform.rotation = Quaternion.Lerp(base.transform.rotation, destinationRotation, progress);
						timer -= Time.deltaTime;
						yield return null;
					}
					m_lockedInPlace = true;
				}
				yield return null;
			}
			yield return StartCoroutine(Patrol_MoveToSpline(toLock: patrolWaypoints[nextWaypointIndex], currentDir: currentDir, fromLock: currentWorldLock, nextWaypointIndex: nextWaypointIndex));
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
		m_animComponent.Play(m_crawlAnim);
		float lerpPos = 0f;
		float timer = 0f;
		while (timer < m_laneChangeDuration)
		{
			if (m_frozen || !fromLock.IsValid || !toLock.IsValid)
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
		return splineRotation * Quaternion.AngleAxis(heading switch
		{
			Direction_2D.Forwards => 180f, 
			Direction_2D.Left => -90f, 
			Direction_2D.Right => 90f, 
			_ => 0f, 
		}, Vector3.up);
	}

	private string GetAnimName(Direction_2D fromDir, Direction_2D toDir)
	{
		if (fromDir == Direction_2D.Left && toDir == Direction_2D.Right)
		{
			return m_turnRightAnimName;
		}
		if (fromDir == Direction_2D.Right && toDir == Direction_2D.Left)
		{
			return m_turnLeftAnimName;
		}
		if (fromDir == Direction_2D.Forwards && toDir == Direction_2D.Left)
		{
			return m_turnLeftAnimName;
		}
		if (fromDir == Direction_2D.Forwards && toDir == Direction_2D.Right)
		{
			return m_turnRightAnimName;
		}
		return null;
	}

	private float CalculateMaxHeight()
	{
		Collider component = GetComponent<Collider>();
		return component.bounds.size.y;
	}

	public override bool isAttackableFromAir()
	{
		return false;
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
