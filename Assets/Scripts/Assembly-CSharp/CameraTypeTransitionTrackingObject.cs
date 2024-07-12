using UnityEngine;

public class CameraTypeTransitionTrackingObject : CameraType
{
	private Vector3 m_fromCameraStartOffset = Vector3.zero;

	private CameraType m_toCam;

	private Transform m_lookAt;

	private float m_duration;

	private float m_timer;

	[SerializeField]
	private float m_distanceToKeepFromTarget = 2f;

	public override bool EnableSmoothing => false;

	public override void onInactive()
	{
	}

	public void BeginTransition(CameraType fromCam, CameraType toCam, Transform lookAt, float duration)
	{
		m_fromCameraStartOffset = fromCam.transform.position - Sonic.Transform.position;
		m_toCam = toCam;
		m_lookAt = lookAt;
		m_duration = duration;
		m_timer = 0f;
	}

	private void LateUpdate()
	{
		if (!(null == m_toCam) && !(null == m_lookAt))
		{
			float t = Mathf.Clamp01(m_timer / m_duration);
			t = Mathf.SmoothStep(0f, 1f, t);
			Vector3 eulerAngles = Quaternion.LookRotation(m_fromCameraStartOffset, Vector3.up).eulerAngles;
			float magnitude = m_fromCameraStartOffset.magnitude;
			Vector3 forward = m_toCam.transform.position - Sonic.Transform.position;
			Vector3 eulerAngles2 = Quaternion.LookRotation(forward, Vector3.up).eulerAngles;
			float magnitude2 = forward.magnitude;
			Vector3 euler = Vector3.Lerp(eulerAngles, eulerAngles2, t);
			float num = Mathf.Lerp(magnitude, magnitude2, t);
			Vector3 vector = Quaternion.Euler(euler) * Vector3.forward;
			Vector3 position = m_lookAt.position;
			base.transform.position = Sonic.Transform.position + vector * num;
			Vector3 vector2 = base.transform.position - m_lookAt.position;
			vector2.y = 0f;
			float magnitude3 = vector2.magnitude;
			if (magnitude3 < m_distanceToKeepFromTarget)
			{
				vector2.Normalize();
				base.transform.position += vector2 * (m_distanceToKeepFromTarget - magnitude3);
			}
			base.transform.LookAt(position);
			base.CachedLookAt = position;
			m_timer += Time.deltaTime;
		}
	}
}
