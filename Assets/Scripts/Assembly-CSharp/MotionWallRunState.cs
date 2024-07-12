using UnityEngine;

public class MotionWallRunState : MotionState
{
	private bool m_runningOnWall = true;

	public MotionWallRunState()
	{
		m_runningOnWall = true;
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.SuspendMe;
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		tParams.Physics.ClearJump();
		LightweightTransform result = tParams.Tracker.Update(tParams.CurrentTransform);
		Vector3 vector = result.Location + result.Up * 3f;
		Vector3 end = vector - result.Up * 4f;
		Debug.DrawLine(vector, end, Color.red);
		if (Physics.Linecast(vector, end, out var hitInfo, 1048576))
		{
			return new LightweightTransform(hitInfo.point + result.Up * 0.1f, result.Orientation);
		}
		float num = 0.8f;
		vector += result.Forwards * (0f - num);
		end += result.Forwards * (0f - num);
		Debug.DrawLine(vector, end, Color.magenta);
		if (Physics.Linecast(vector, end, 1048576))
		{
			return result;
		}
		if (!m_runningOnWall)
		{
			tParams.StateMachine.PopTopState();
			tParams.StateMachine.RequestState(new MotionStumbleState(Sonic.Handling.gameObject, Sonic.Handling));
		}
		m_runningOnWall = false;
		return result;
	}
}
