using System;
using UnityEngine;

[AddComponentMenu("Dash/Cameras/Spline")]
public class CameraTypeSpline : CameraType
{
	public enum Direction
	{
		Forward,
		Backwards
	}

	[Flags]
	private enum State
	{
		None = 0,
		Moving = 1
	}

	[SerializeField]
	private iTweenPath m_positionPath;

	[SerializeField]
	private iTweenPath m_lookAtPath;

	[SerializeField]
	private float m_transitionTime = 30f;

	private GameObject m_lookAtTracker;

	private State m_state;

	public iTweenPath PositionPath
	{
		get
		{
			return m_positionPath;
		}
		set
		{
			m_positionPath = value;
		}
	}

	public iTweenPath LookAtPath
	{
		get
		{
			return m_lookAtPath;
		}
		set
		{
			m_lookAtPath = value;
		}
	}

	public float TransitionTime
	{
		get
		{
			return m_transitionTime;
		}
		set
		{
			m_transitionTime = value;
		}
	}

	public bool InTransition => (m_state & State.Moving) == State.Moving;

	public override bool EnableSmoothing => true;

	public void PrepareForMovement(Direction direction)
	{
		Vector3[] pathPoints = GetPathPoints(m_positionPath, direction);
		Vector3[] pathPoints2 = GetPathPoints(m_lookAtPath, direction);
		base.transform.position = pathPoints[0];
		m_lookAtTracker.transform.position = pathPoints2[0];
		base.transform.LookAt(m_lookAtTracker.transform.position);
	}

	public void StartMovement(Direction direction, iTween.EaseType camPosEaseType, iTween.EaseType lookAtEasyType)
	{
		Vector3[] pathPoints = GetPathPoints(m_positionPath, direction);
		Vector3[] pathPoints2 = GetPathPoints(m_lookAtPath, direction);
		iTween.MoveTo(base.gameObject, iTween.Hash("path", pathPoints, "time", m_transitionTime, "easetype", camPosEaseType, "oncomplete", "OnPathFinished", "oncompletetarget", base.gameObject, "oncompleteparams", this));
		iTween.MoveTo(m_lookAtTracker, iTween.Hash("path", pathPoints2, "time", m_transitionTime, "easetype", lookAtEasyType));
		m_state |= State.Moving;
	}

	private void Start()
	{
		m_lookAtTracker = new GameObject($"{base.name} Look At Tracker");
	}

	private void Update()
	{
		if ((m_state & State.Moving) == State.Moving && (bool)m_lookAtTracker)
		{
			base.transform.LookAt(m_lookAtTracker.transform.position);
		}
		base.CachedLookAt = m_lookAtTracker.transform.position;
	}

	private Vector3[] GetPathPoints(iTweenPath path, Direction direction)
	{
		Vector3[] array = path.nodes.ToArray();
		if (direction == Direction.Backwards)
		{
			Array.Reverse(array);
		}
		return array;
	}

	private void OnPathFinished()
	{
		m_state &= ~State.Moving;
	}
}
