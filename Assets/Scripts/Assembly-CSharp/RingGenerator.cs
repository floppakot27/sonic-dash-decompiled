using System;
using UnityEngine;

public class RingGenerator : MonoBehaviour
{
	[Flags]
	private enum State
	{
		None = 0,
		Idle = 1,
		InitialRun = 2
	}

	private const int ReservedSequences = 3;

	[SerializeField]
	private int m_maximumSequenceList = 5;

	[SerializeField]
	private int m_maximumSequenceLength = 20;

	[SerializeField]
	private float m_spaceBetweenRings = 2f;

	[SerializeField]
	private float m_ringHeightOffset = 0.8f;

	private State m_state;

	private RingSequence[] m_ringSequences;

	private int m_reservedSequences;

	public int MaximumSequenceLength => m_maximumSequenceLength;

	public float SpaceBetweenRings => m_spaceBetweenRings;

	public float RingHeightOffset => m_ringHeightOffset;

	public RingSequence[] GetSequences()
	{
		return m_ringSequences;
	}

	public RingSequence GetReservedSequence()
	{
		if (m_reservedSequences == 3)
		{
			return null;
		}
		int num = m_maximumSequenceList + m_reservedSequences;
		m_reservedSequences++;
		return m_ringSequences[num];
	}

	public RingSequence GetFreeSequence()
	{
		RingSequence[] ringSequences = m_ringSequences;
		foreach (RingSequence ringSequence in ringSequences)
		{
			if (ringSequence.IsIdleGameplaySequence)
			{
				return ringSequence;
			}
		}
		return null;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("DisableGameState", this);
		EventDispatch.RegisterInterest("ResetGameState", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this);
		int num = m_maximumSequenceList + 3;
		m_ringSequences = new RingSequence[num];
		for (int i = 0; i < m_ringSequences.Length; i++)
		{
			bool reserved = ((i >= m_maximumSequenceList) ? true : false);
			m_ringSequences[i] = new RingSequence(m_maximumSequenceLength, reserved);
			m_ringSequences[i].Reset();
		}
		m_state |= State.Idle;
	}

	private void Update()
	{
		if ((m_state & State.Idle) != State.Idle)
		{
			UpdateSequences();
		}
	}

	private void UpdateSequences()
	{
		RingSequence[] ringSequences = m_ringSequences;
		foreach (RingSequence ringSequence in ringSequences)
		{
			ringSequence.Update();
		}
	}

	private void ClearAllRings()
	{
		RingSequence[] ringSequences = m_ringSequences;
		foreach (RingSequence ringSequence in ringSequences)
		{
			ringSequence.Reset();
		}
	}

	private void Event_DisableGameState(GameState.Mode nextState)
	{
		m_state &= State.Idle;
		StopAllCoroutines();
	}

	private void Event_ResetGameState(GameState.Mode nextState)
	{
		Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest.Invalid);
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		ClearAllRings();
		m_state &= ~State.Idle;
		m_state |= State.InitialRun;
	}
}
