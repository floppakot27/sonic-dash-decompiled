using System;
using System.Collections;
using UnityEngine;

public class MotionSpringBossFinishState : MotionSpringState
{
	private enum TrackState
	{
		NotYetRequested,
		Requested,
		Completed
	}

	private TrackState m_trackState;

	private System.Random m_rng = new System.Random();

	public MotionSpringBossFinishState(SonicHandling handling, SonicPhysics physics)
		: base(physics, handling)
	{
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Execute()
	{
		if (m_trackState == TrackState.NotYetRequested)
		{
			BossBattleSystem bossBattleSystem = BossBattleSystem.Instance();
			if (bossBattleSystem == null || !bossBattleSystem.IsEnabled())
			{
				m_trackState = TrackState.Requested;
				CreateTrackForLanding();
			}
		}
		else if (m_trackState == TrackState.Completed)
		{
			MoveToNextState();
		}
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		return tParams.CurrentTransform;
	}

	private MotionState GetNextState()
	{
		return new MotionSpringDescentState(MotionSpringDescentState.TrackState.Ready, Sonic.Handling, base.Physics);
	}

	private void MoveToNextState()
	{
		MotionState nextState = GetNextState();
		MotionStateMachine internalMotionState = Sonic.Tracker.InternalMotionState;
		internalMotionState.RequestState(nextState);
	}

	private void CreateTrackForLanding()
	{
		Sonic.Handling.StartCoroutine(GenerateSpringTransitionAtEndOfFrame());
	}

	private IEnumerator GenerateSpringTransitionAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		float timeRemaining = base.Physics.JumpTimeRemaining;
		float landingSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
		float landingStripLength = landingSpeed * timeRemaining;
		TrackGenerator track = Sonic.Tracker.Track as TrackGenerator;
		SpringTV.Type springType = SpringTV.RandomType(m_rng);
		SpringTV.Destination currentSubzone = (SpringTV.Destination)track.CalculateCurrentSubzoneIndex();
		SpringTV.Destination destination = currentSubzone;
		if (springType == SpringTV.Type.ChangeZone)
		{
			do
			{
				destination = SpringTV.AnyDestination(m_rng);
			}
			while (destination == currentSubzone);
		}
		SpringTV.CreateFlags createFlags = SpringTV.CreateFlags.None;
		TrackGenerator.MidGameNewTrackRequest requestData = new TrackGenerator.MidGameNewTrackRequest
		{
			SpringType = springType,
			Destination = destination,
			CreateFlags = createFlags,
			EmptyAirPrefix = landingStripLength
		};
		EventDispatch.GenerateEvent("RequestNewTrackMidGame", requestData);
	}

	private void Event_OnTrackGenerationComplete()
	{
		m_trackState = TrackState.Completed;
	}
}
