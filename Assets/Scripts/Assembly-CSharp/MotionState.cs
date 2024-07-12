using System.Runtime.InteropServices;
using UnityEngine;

public class MotionState
{
	public enum InterruptCode
	{
		SuspendMe,
		KillMe
	}

	public enum GestureType
	{
		Strafe,
		Roll,
		Dive,
		Jump,
		Tap
	}

	[StructLayout(0, Size = 1)]
	public struct TransformParameters
	{
		public SplineTracker Tracker { get; set; }

		public MotionStateMachine StateMachine { get; set; }

		public LightweightTransform CurrentTransform { get; set; }

		public SonicPhysics Physics { get; set; }

		public Track Track { get; set; }

		public bool OverGap { get; set; }

		public bool OverSmallIsland { get; set; }
	}

	public virtual void Enter()
	{
	}

	public virtual void Exit()
	{
	}

	public virtual void Execute()
	{
	}

	public virtual InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}

	public virtual LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		return tParams.CurrentTransform;
	}

	public virtual bool IsFalling()
	{
		return false;
	}

	public virtual bool IsFlying()
	{
		return false;
	}

	public virtual bool IsReadyForDash()
	{
		return true;
	}

	public virtual bool IsDead()
	{
		return false;
	}

	public virtual bool IsSpringing()
	{
		return false;
	}

	public virtual MotionState OnJump(SonicPhysics physics, float initialGroundHeight, GameObject animatingObject, SonicHandling handling)
	{
		return null;
	}

	public virtual MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		return null;
	}

	public virtual MotionState OnKnockedSideways(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		return null;
	}

	public virtual MotionState OnFall()
	{
		return null;
	}

	public virtual MotionState OnSplat(SonicHandling handling, SplineTracker tracker, SonicAnimationControl.SplatType splatType, Hazard hazard)
	{
		string reason = ((!(hazard == null)) ? hazard.GetType().ToString() : "NULL");
		GameAnalytics.PlayerDeath(reason);
		return new MotionSplatState(handling, tracker, splatType);
	}

	public virtual MotionState OnStumble(GameObject animatingObject, SonicHandling handling)
	{
		return null;
	}

	public virtual MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		return null;
	}

	public virtual MotionState OnDive(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling)
	{
		return null;
	}

	public virtual MotionState OnAttack(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, float tapX, float tapY)
	{
		return null;
	}

	public virtual MotionState OnSpring(Track track, SonicHandling handling, SonicPhysics physics, SpringTV.Type springType, SpringTV.Destination destination, SpringTV.CreateFlags createFlags)
	{
		if (springType == SpringTV.Type.Boss)
		{
			return new MotionSpringBossAscentState(handling, physics);
		}
		return new MotionSpringAscentState(handling, physics, springType, destination, createFlags);
	}

	public virtual MotionState OnSetPiece()
	{
		return new MotionSetPieceState();
	}

	public virtual void OnSetPieceEnd(MotionStateMachine stateMachien)
	{
	}
}
