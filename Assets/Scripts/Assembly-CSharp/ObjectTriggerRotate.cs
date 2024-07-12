using UnityEngine;

public class ObjectTriggerRotate : MonoBehaviour
{
	[SerializeField]
	private float m_triggerDistance = 50f;

	[SerializeField]
	private AnimationCurve m_animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 360f, 1f, 0f));

	private Quaternion m_initialRotation;

	private float m_timer;

	private void OnEnable()
	{
		m_initialRotation = base.transform.rotation;
	}

	private void Update()
	{
		float sqrMagnitude = (base.transform.position - Sonic.Tracker.transform.position).sqrMagnitude;
		if (sqrMagnitude < m_triggerDistance * m_triggerDistance)
		{
			m_timer += Time.deltaTime;
			float angle = m_animationCurve.Evaluate(m_timer);
			base.transform.rotation = m_initialRotation * Quaternion.AngleAxis(angle, Vector3.up);
		}
	}
}
