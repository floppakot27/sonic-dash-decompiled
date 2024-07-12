using UnityEngine;

public class CameraTypeAnimation : CameraType
{
	public float m_weight;

	private float m_previousWeight;

	public float Weight => m_weight;

	private void Update()
	{
		if (m_previousWeight != m_weight)
		{
			if (Mathf.Approximately(m_weight, 0f))
			{
				CameraScheduler.ClearAnimationCamera();
			}
			else
			{
				CameraScheduler.UseAnimationCamera(this);
			}
			m_previousWeight = m_weight;
		}
	}

	private void LateUpdate()
	{
		base.CachedLookAt = base.transform.position + base.transform.forward;
	}
}
