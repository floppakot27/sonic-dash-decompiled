using System.Collections;
using UnityEngine;

public class MotionSpringAscentState : MotionSpringState
{
	private enum TrackState
	{
		NotYetRequested,
		Requested,
		Completed
	}

	private bool m_doRegeneration = true;

	private TrackState m_trackState;

	private float m_forwardsSpeed;

	private float m_highestHeight = float.MinValue;

	private float m_previousHeight;

	private SpringTV.Type m_springType;

	private SpringTV.Destination m_destination;

	private SpringTV.CreateFlags m_createFlags;

	public MotionSpringAscentState(SonicHandling handling, SonicPhysics physics, SpringTV.Type springType, SpringTV.Destination newTrackType, SpringTV.CreateFlags createFlags)
		: base(physics, handling)
	{
		m_springType = springType;
		m_destination = newTrackType;
		m_createFlags = createFlags;
	}

	public override void Enter()
	{
		base.Enter();
		JumpAnimationCurve newSpringJumpCurve = Sonic.Handling.GetNewSpringJumpCurve(Sonic.Tracker.HeightAboveLowGround);
		m_previousHeight = newSpringJumpCurve.CalculateHeight(0f);
		base.Physics.StartJump(newSpringJumpCurve);
		EventDispatch.GenerateEvent("OnSpringStart", m_springType);
		Sonic.AudioControl.PlaySpringSFX();
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		float jumpHeight = tParams.Physics.JumpHeight;
		bool flag = m_highestHeight > jumpHeight;
		if (m_trackState == TrackState.NotYetRequested && flag)
		{
			m_trackState = TrackState.Requested;
			CreateTrackForLanding();
			m_forwardsSpeed = 0f;
		}
		m_highestHeight = Mathf.Max(jumpHeight, m_highestHeight);
		tParams.Physics.TargetSpeed = 0f;
		float num = m_forwardsSpeed * Time.deltaTime;
		float num2 = jumpHeight - m_previousHeight;
		Vector3 vector = base.FlatForward * num + Vector3.up * num2;
		LightweightTransform result = new LightweightTransform(tParams.CurrentTransform.Location + vector, tParams.CurrentTransform.Orientation);
		tParams.Physics.UpdateJump(pauseHalfway: false);
		if (flag)
		{
			GoToState(tParams.StateMachine);
		}
		m_previousHeight = jumpHeight;
		return result;
	}

	private MotionState GetNextState()
	{
		if (m_springType == SpringTV.Type.Bank)
		{
			MotionSpringDescentState.TrackState trackState = ((m_trackState != TrackState.Requested) ? MotionSpringDescentState.TrackState.Ready : MotionSpringDescentState.TrackState.NotReady);
			return new MotionSpringDescentState(trackState, Sonic.Handling, base.Physics);
		}
		MotionSpringGesturesState.TrackGenState currentTrackState = ((m_trackState != TrackState.Requested) ? MotionSpringGesturesState.TrackGenState.Generated : MotionSpringGesturesState.TrackGenState.Requested);
		return new MotionSpringGesturesState(currentTrackState, Sonic.Handling, base.Physics);
	}

	private void GoToState(MotionStateMachine machine)
	{
		if (m_springType == SpringTV.Type.Bank)
		{
			EventDispatch.GenerateEvent("OnRingBankRequest");
		}
		MotionState nextState = GetNextState();
		machine.RequestState(nextState);
	}

	private void CreateTrackForLanding()
	{
		if (m_doRegeneration)
		{
			Sonic.Handling.StartCoroutine(GenerateSpringTransitionAtEndOfFrame());
		}
	}

	private IEnumerator GenerateSpringTransitionAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		float timeRemaining = base.Physics.JumpTimeRemaining;
		float landingSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
		float landingStripLength = landingSpeed * timeRemaining;
		TrackGenerator.MidGameNewTrackRequest requestData = new TrackGenerator.MidGameNewTrackRequest
		{
			SpringType = m_springType,
			Destination = m_destination,
			CreateFlags = m_createFlags,
			EmptyAirPrefix = landingStripLength
		};
		EventDispatch.GenerateEvent("RequestNewTrackMidGame", requestData);
	}

	private bool IsSuitableTransitionTime()
	{
		Vector3 forward = Camera.main.transform.forward;
		float num = Vector3.Dot(Vector3.up, forward);
		return num > 0.7f;
	}

	private void Event_OnTrackGenerationComplete()
	{
		m_trackState = TrackState.Completed;
	}
}
