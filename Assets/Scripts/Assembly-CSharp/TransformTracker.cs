using System.Collections;
using UnityEngine;

internal class TransformTracker
{
	private Transform m_toTrack;

	private float m_smoothingFactor;

	private Vector3 m_accurateVelocity;

	private Vector3 m_smoothedVelocity;

	public bool IsValid => m_toTrack != null;

	public Vector3 AccurateVelocity => m_accurateVelocity;

	public Vector3 SmoothedVelocity => m_smoothedVelocity;

	public TransformTracker()
	{
		m_toTrack = null;
	}

	public TransformTracker(Transform toTrack, MonoBehaviour owner, float smoothingFactor)
	{
		m_toTrack = toTrack;
		m_smoothingFactor = smoothingFactor;
		owner.StartCoroutine(Track());
	}

	private IEnumerator Track()
	{
		Vector3 lastPos = m_toTrack.position;
		m_smoothedVelocity = Vector3.zero;
		while (true)
		{
			Vector3 newPos = m_toTrack.position;
			m_accurateVelocity = (newPos - lastPos) / Time.fixedDeltaTime;
			m_smoothedVelocity = Utils.Smooth(m_smoothedVelocity, m_accurateVelocity, m_smoothingFactor);
			lastPos = newPos;
			yield return new WaitForFixedUpdate();
		}
	}
}
