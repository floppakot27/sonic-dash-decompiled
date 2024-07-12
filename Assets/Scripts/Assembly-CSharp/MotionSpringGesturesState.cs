using System;
using System.Collections.Generic;
using UnityEngine;

public class MotionSpringGesturesState : MotionSpringState
{
	public enum TrackGenState
	{
		Requested,
		Generated
	}

	private bool m_isTrackGenerated;

	private int m_completedGestureWindows;

	private float m_timeToEndOfGestureWindow;

	private float m_timeToNextGestureWindow;

	private float m_gestureWindowDuration;

	private IList<CartesianDir> m_windowInputsRequired;

	private bool m_isSucessful = true;

	private System.Random m_rng = new System.Random();

	private CartesianDir[] m_gesturesOut = new CartesianDir[Sonic.Handling.GesturesPerSpring];

	private CartesianDir CurrentWindowGesture => m_windowInputsRequired[m_completedGestureWindows];

	public MotionSpringGesturesState(TrackGenState currentTrackState, SonicHandling handling, SonicPhysics physics)
		: base(physics, handling)
	{
		m_isTrackGenerated = currentTrackState == TrackGenState.Generated;
		int enumCount = Utils.GetEnumCount<CartesianDir>();
		for (int i = 0; i < m_gesturesOut.Length; i++)
		{
			m_gesturesOut[i] = (CartesianDir)i;
		}
	}

	public override void Enter()
	{
		base.Enter();
		float difficultyAtSonicDistance = Difficulty.GetDifficultyAtSonicDistance();
		m_gestureWindowDuration = Sonic.Handling.SpringGestureWindow.Evaluate(difficultyAtSonicDistance);
		m_windowInputsRequired = CreateGesturesRequired();
		StartGestureWindow();
	}

	public override void Execute()
	{
		m_timeToEndOfGestureWindow -= Time.deltaTime;
		m_timeToNextGestureWindow -= Time.deltaTime;
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		if (m_timeToNextGestureWindow > 0f || m_timeToEndOfGestureWindow > 0f)
		{
			return tParams.CurrentTransform;
		}
		if (!m_isSucessful || m_completedGestureWindows >= Sonic.Handling.GesturesPerSpring)
		{
			if (m_isTrackGenerated)
			{
				MoveToNextState(tParams.StateMachine);
			}
		}
		else
		{
			m_completedGestureWindows++;
			if (m_completedGestureWindows == Sonic.Handling.GesturesPerSpring)
			{
				EventDispatch.GenerateEvent("SpringGestureSuccess");
			}
			else
			{
				StartGestureWindow();
			}
		}
		return tParams.CurrentTransform;
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		CartesianDir dir = ((direction != 0) ? CartesianDir.Right : CartesianDir.Left);
		OnGestureInput(dir);
		return null;
	}

	public override MotionState OnJump(SonicPhysics physics, float initialGroundHeight, GameObject animatingObject, SonicHandling handling)
	{
		OnGestureInput(CartesianDir.Up);
		return null;
	}

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		OnGestureInput(CartesianDir.Down);
		return null;
	}

	private IList<CartesianDir> CreateGesturesRequired()
	{
		for (int num = m_gesturesOut.Length - 1; num > 0; num--)
		{
			int num2 = m_rng.Next(num);
			CartesianDir cartesianDir = m_gesturesOut[num];
			m_gesturesOut[num] = m_gesturesOut[num2];
			m_gesturesOut[num2] = cartesianDir;
		}
		return m_gesturesOut;
	}

	private void StartGestureWindow()
	{
		GesturesHUD.ShowGesture(CurrentWindowGesture);
		m_timeToEndOfGestureWindow = m_gestureWindowDuration;
		m_timeToNextGestureWindow = 0f;
		m_isSucessful = false;
	}

	private void OnGestureInput(CartesianDir dir)
	{
		if (IsInGestureWindow())
		{
			if (CurrentWindowGesture == dir)
			{
				m_isSucessful = true;
				EventDispatch.GenerateEvent("OnSingleSpringGestureSuccess", dir);
			}
			else
			{
				EventDispatch.GenerateEvent("SpringGestureFailure");
			}
			ConsumeGestureWindow();
		}
	}

	private bool IsInGestureWindow()
	{
		return m_timeToEndOfGestureWindow > 0f && m_completedGestureWindows < Sonic.Handling.GesturesPerSpring;
	}

	private void ConsumeGestureWindow()
	{
		m_timeToEndOfGestureWindow = 0f;
		m_timeToNextGestureWindow = Sonic.Handling.PostGestureDelay;
	}

	private void MoveToNextState(MotionStateMachine stateMachine)
	{
		stateMachine.RequestState(new MotionSpringDescentState(MotionSpringDescentState.TrackState.Ready, Sonic.Handling, base.Physics));
	}

	private void Event_OnTrackGenerationComplete()
	{
		m_isTrackGenerated = true;
	}
}
