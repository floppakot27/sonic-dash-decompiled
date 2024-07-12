using UnityEngine;

public class MenuMeshRotate : MonoBehaviour
{
	private Quaternion m_baseRotation;

	private float m_currentRotation = 0.5f;

	private float m_rotationSpeed = 1.5f;

	[SerializeField]
	private float m_rotationAmount = 40f;

	[SerializeField]
	private bool m_ignoreTimeScale = true;

	public void Start()
	{
		m_baseRotation = base.transform.localRotation;
	}

	private void Update()
	{
		Spin();
	}

	public void Spin()
	{
		float num = ((!m_ignoreTimeScale) ? Time.deltaTime : IndependantTimeDelta.Delta);
		m_currentRotation += num * m_rotationSpeed;
		float num2 = Mathf.Sin(m_currentRotation);
		base.transform.localRotation = m_baseRotation;
		base.transform.Rotate(0f, 0f, m_rotationAmount * num2);
	}
}
