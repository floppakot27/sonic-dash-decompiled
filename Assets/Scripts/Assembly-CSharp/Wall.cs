using System;
using UnityEngine;

[AddComponentMenu("Dash/Obstacles/Wall")]
public class Wall : Obstacle
{
	private class WallCollisionResolver : CollisionResolver
	{
		private Obstacle m_obstacle;

		public WallCollisionResolver(Obstacle obstacle)
			: base(ResolutionType.SonicDeath)
		{
			m_obstacle = obstacle;
		}

		public override void ProcessMotionState(MotionState state, bool heldRings, bool ghosted)
		{
			Type type = state.GetType();
			if (type == typeof(MotionWallRunState))
			{
				base.Resolution = ResolutionType.Nothing;
			}
			else if (type == typeof(MotionJumpState))
			{
				Vector3 position = m_obstacle.transform.position;
				Vector3 vector = Quaternion.Inverse(m_obstacle.transform.rotation) * position;
				vector = Quaternion.Euler(-90f, 0f, 0f) * vector;
				float num = 0.3f;
				Bounds bounds = m_obstacle.collider.bounds;
				if (position.y < 0f - bounds.size.y + bounds.center.y + num || vector.z < 0f)
				{
					base.Resolution = ResolutionType.SonicDieForwards;
				}
				else
				{
					base.Resolution = ResolutionType.SonicDeath;
				}
			}
			else if (type == typeof(MotionGroundStrafeState))
			{
				Vector3 position2 = m_obstacle.transform.position;
				Vector3 vector2 = Quaternion.Inverse(m_obstacle.transform.rotation) * position2;
				vector2 = Quaternion.Euler(-90f, 0f, 0f) * vector2;
				Vector3 vector3 = Quaternion.Inverse(m_obstacle.transform.rotation) * m_obstacle.collider.bounds.extents;
				float num2 = 0.4f - Sonic.Handling.StumblePenetrationDistance;
				if (vector2.x > vector3.x + num2)
				{
					base.Resolution = ResolutionType.SonicDieForwards;
				}
				else if (vector2.x < 0f - (vector3.x + num2))
				{
					base.Resolution = ResolutionType.SonicDieForwards;
				}
				else
				{
					base.Resolution = ResolutionType.SonicDeath;
				}
			}
			else
			{
				base.Resolution = ResolutionType.SonicDeath;
			}
		}
	}

	private bool m_isPlaced;

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public override void Start()
	{
		base.Start();
		base.CollisionResolver = new WallCollisionResolver(this);
	}

	public void Update()
	{
		if (m_isPlaced && CurrentSpline == null)
		{
			DestroySelf();
		}
	}

	public override void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
	}

	public override void OnDeath(object[] onDeathParams)
	{
		if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
		{
			EventDispatch.GenerateEvent("OnSmashThrough");
		}
		HazardAudioControl.Singleton.PlayWallDeathSfx();
		DestroySelf();
	}

	public override void OnStumble(SonicSplineTracker killer)
	{
	}

	public override Spline getSpline()
	{
		return CurrentSpline;
	}

	protected override void Place(Track track, Spline spline)
	{
		CurrentSpline = spline;
		Track = track;
		m_isPlaced = true;
	}
}
