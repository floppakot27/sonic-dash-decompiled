using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RingSequence : IEnumerable
{
	public class Ring
	{
		public bool m_occupied;

		public bool m_collected;

		public Vector3 m_position;

		public Vector3 m_localPosition;

		public Quaternion m_localOrientation;

		public GameObject m_owningObject;

		public Vector3 m_floorPosition;

		public Vector3 m_localFloorPosition;

		public Vector3 m_floorUp;

		public uint m_substreaks;

		public bool isMagnetised;

		public Vector3 m_preMagnetisedLocalPosition;

		public float m_magnetismTime;

		public bool m_forceCollecion;

		public void Magnetise()
		{
			isMagnetised = true;
			m_preMagnetisedLocalPosition = m_localPosition;
			m_magnetismTime = 0f;
		}
	}

	[Flags]
	private enum State
	{
		None = 0,
		RingCollected = 1,
		Reserved = 2,
		TransformPosition = 4,
		Collectable = 8,
		StreakCompleted = 0x10,
		StreakMissed = 0x20
	}

	public const int DefaultSequenceLength = 10;

	private static object[] m_ringSequenceParameters = new object[3];

	public static uint NoStreak = 0u;

	private Ring[] m_ringSequence;

	private int m_ringCount;

	private int m_sequenceLength;

	private State m_state;

	private int[] m_substreakCounts = new int[32];

	private Vector3 m_cachedLocalSequencePosition = Vector3.zero;

	private float m_cachedSequenceSqRadius;

	private float m_lastRingTrackPosition;

	private float m_firstRingTrackPosition = 100000000f;

	public bool RingsAvailable => m_ringCount > 0;

	public bool RingsCollected => (m_state & State.RingCollected) == State.RingCollected;

	public bool StreakCompleted => (m_state & State.StreakCompleted) == State.StreakCompleted;

	public bool StreakMissed => (m_state & State.StreakMissed) == State.StreakMissed;

	public int Length => m_sequenceLength;

	public int Capacity => m_ringSequence.Length;

	public bool Reserved => (m_state & State.Reserved) == State.Reserved;

	public bool TransformPosition
	{
		set
		{
			if (value)
			{
				m_state |= State.TransformPosition;
			}
			else
			{
				m_state &= ~State.TransformPosition;
			}
		}
	}

	public bool Collectable
	{
		get
		{
			return (m_state & State.Collectable) == State.Collectable;
		}
		set
		{
			if (value)
			{
				m_state |= State.Collectable;
			}
			else
			{
				m_state &= ~State.Collectable;
			}
		}
	}

	public bool IsIdleGameplaySequence => !Reserved && !RingsAvailable;

	public bool IsSequencePositionSupported => !Reserved;

	public Vector3 WorldLocalSequencePosition => m_cachedLocalSequencePosition;

	public float SqSequenceRadius => m_cachedSequenceSqRadius;

	public RingSequence(bool reserved)
	{
		CreateSequence(10, reserved);
	}

	public RingSequence(int sequenceLength, bool reserved)
	{
		CreateSequence(sequenceLength, reserved);
	}

	public RingID AddRing(Vector3 ringWorldPosition, Quaternion ringOrientation, Vector3 floorWorldPosition, Vector3 floorUp, GameObject owningObject, uint substreaks, float trackPosition)
	{
		Ring ring = m_ringSequence.FirstOrDefault((Ring thisRing) => !thisRing.m_occupied);
		if (ring == null)
		{
			return RingID.Invalid;
		}
		ring.m_occupied = true;
		ring.m_collected = false;
		ring.m_position = ringWorldPosition;
		ring.m_localPosition = owningObject.transform.InverseTransformPoint(ringWorldPosition);
		ring.m_localOrientation = ringOrientation;
		ring.m_owningObject = owningObject;
		ring.m_floorPosition = floorWorldPosition;
		ring.m_localFloorPosition = owningObject.transform.InverseTransformPoint(floorWorldPosition);
		ring.m_floorUp = floorUp;
		ring.m_substreaks = substreaks;
		ring.isMagnetised = false;
		ring.m_forceCollecion = false;
		m_ringCount++;
		m_sequenceLength++;
		foreach (int substreakID in GetSubstreakIDs(substreaks))
		{
			m_substreakCounts[substreakID]++;
		}
		if (m_lastRingTrackPosition < trackPosition)
		{
			m_lastRingTrackPosition = trackPosition;
		}
		if (m_firstRingTrackPosition > trackPosition)
		{
			m_firstRingTrackPosition = trackPosition;
		}
		RingID result = default(RingID);
		result.Sequence = this;
		result.Ring = ring;
		return result;
	}

	public void OnFinishedAddingRings()
	{
		if (!IsSequencePositionSupported)
		{
			return;
		}
		Transform worldRoot = SonicSplineTracker.FindRootTransform();
		IEnumerable<Vector3> enumerable = from r in m_ringSequence
			where r.m_occupied
			select worldRoot.InverseTransformPoint(r.m_position);
		Vector3 vector = enumerable.Aggregate(Vector3.zero, (Vector3 totalPos, Vector3 localPos) => totalPos + localPos);
		m_cachedLocalSequencePosition = vector / m_ringCount;
		float num = 0f;
		foreach (Vector3 item in enumerable)
		{
			float sqrMagnitude = (item - m_cachedLocalSequencePosition).sqrMagnitude;
			if (sqrMagnitude > num)
			{
				num = sqrMagnitude;
			}
		}
		m_cachedSequenceSqRadius = num;
	}

	public void Reset()
	{
		Ring[] ringSequence = m_ringSequence;
		foreach (Ring ring in ringSequence)
		{
			ring.m_occupied = false;
			ring.m_collected = false;
		}
		m_lastRingTrackPosition = 0f;
		m_firstRingTrackPosition = 100000000f;
		m_state &= ~State.StreakMissed;
		m_state &= ~State.StreakCompleted;
		m_ringCount = 0;
		m_sequenceLength = 0;
		ClearCollection();
		for (int j = 0; j < m_substreakCounts.Length; j++)
		{
			m_substreakCounts[j] = 0;
		}
		m_cachedLocalSequencePosition = Vector3.zero;
		m_cachedSequenceSqRadius = 0f;
	}

	public void Update()
	{
		if ((m_state & State.TransformPosition) == State.TransformPosition && RingsAvailable && !(Sonic.Tracker == null))
		{
			m_ringCount = 0;
			for (int i = 0; i < m_ringSequence.Length; i++)
			{
				Ring ring = m_ringSequence[i];
				ring.m_occupied = ring.m_occupied && ring.m_owningObject != null;
				m_ringCount += (ring.m_occupied ? 1 : 0);
			}
			if (Sonic.Tracker.TrackPosition > m_lastRingTrackPosition && !StreakMissed && !StreakCompleted)
			{
				m_state |= State.StreakMissed;
				GenerateStreakEvent("OnRingStreakMissed");
			}
		}
	}

	public void GenerateStreakEvent(string eventName)
	{
		m_ringSequenceParameters[0] = Length;
		m_ringSequenceParameters[1] = m_firstRingTrackPosition;
		m_ringSequenceParameters[2] = m_lastRingTrackPosition;
		EventDispatch.GenerateEvent(eventName, m_ringSequenceParameters);
	}

	public IEnumerator GetEnumerator()
	{
		return m_ringSequence.GetEnumerator();
	}

	public Ring GetRing(int ringIndex)
	{
		if (ringIndex >= m_ringSequence.Length)
		{
			return null;
		}
		return m_ringSequence[ringIndex];
	}

	public void RegisterCollection(Ring collectedRing)
	{
		collectedRing.m_collected = true;
		collectedRing.m_occupied = false;
		m_ringCount--;
		foreach (int substreakID in GetSubstreakIDs(collectedRing.m_substreaks))
		{
			m_substreakCounts[substreakID]--;
			if (m_substreakCounts[substreakID] == 0)
			{
				m_state |= State.StreakCompleted;
			}
		}
		m_state |= State.RingCollected;
		PlayerStats.UpdateDistanceToCurrent(PlayerStats.DistanceNames.DistanceLastPickedRing);
	}

	public void ClearCollection()
	{
		m_state &= ~State.RingCollected;
		m_state &= ~State.StreakCompleted;
	}

	public void ClearCollection(Ring collectedRing)
	{
		collectedRing.m_collected = false;
	}

	public IEnumerable<int> GetSubstreakIDs(uint substreakMask)
	{
		for (int substreakID = 0; substreakID < 32; substreakID++)
		{
			if ((substreakMask & (1 << substreakID)) > 0)
			{
				yield return substreakID;
			}
		}
	}

	private void CreateSequence(int sequenceLength, bool reserved)
	{
		m_ringSequence = new Ring[sequenceLength];
		for (int i = 0; i < sequenceLength; i++)
		{
			m_ringSequence[i] = new Ring();
		}
		m_state &= ~State.RingCollected;
		m_ringCount = 0;
		m_state |= State.TransformPosition;
		m_state |= State.Collectable;
		if (reserved)
		{
			m_state |= State.Reserved;
		}
	}
}
