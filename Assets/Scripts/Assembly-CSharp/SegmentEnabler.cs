using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentEnabler
{
	private static SegmentEnabler s_enabler;

	private Queue<TrackSegment> m_segmentsToUpdate = new Queue<TrackSegment>();

	private TrackSegment m_currentSegment;

	private Queue<Transform> m_segmentTransformsToUpdate = new Queue<Transform>();

	public bool IsProcessingPending => m_currentSegment != null || m_segmentsToUpdate.Count > 0;

	public SegmentEnabler()
	{
		s_enabler = this;
	}

	public static void RequestUpdate(TrackSegment segment)
	{
		s_enabler.m_segmentsToUpdate.Enqueue(segment);
	}

	public void Shutdown()
	{
		s_enabler = null;
	}

	public void Reset()
	{
		m_segmentsToUpdate.Clear();
		m_segmentTransformsToUpdate.Clear();
	}

	public void RestartEnablingWith(MonoBehaviour owner)
	{
		owner.StartCoroutine(ProcessQueue());
	}

	private IEnumerator ProcessQueue()
	{
		while (true)
		{
			yield return null;
			while (!FrameTimeSentinal.IsFramerateImportant && IsProcessingPending)
			{
				ProcessSingleQueueItem();
			}
		}
	}

	private void ProcessSingleQueueItem()
	{
		if (m_currentSegment != null)
		{
			while (m_segmentTransformsToUpdate.Count > 0 && m_segmentTransformsToUpdate.Peek() == null)
			{
				m_segmentTransformsToUpdate.Dequeue();
			}
			if (m_segmentTransformsToUpdate.Count > 0)
			{
				Transform gameplayEnabledOn = m_segmentTransformsToUpdate.Dequeue();
				m_currentSegment.SetGameplayEnabledOn(gameplayEnabledOn);
			}
			if (m_segmentTransformsToUpdate.Count == 0)
			{
				m_currentSegment = null;
			}
		}
		else if (m_segmentsToUpdate.Count > 0)
		{
			m_currentSegment = m_segmentsToUpdate.Dequeue();
			m_currentSegment.PushTransformsToEnable(m_segmentTransformsToUpdate);
		}
	}
}
