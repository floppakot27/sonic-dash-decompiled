using UnityEngine;

public class MenuSpriteRotate : MonoBehaviour
{
	public float m_rotationSpeed = 60f;

	public bool m_reverseRotation;

	public bool m_ignoreTimeScale = true;

	public void Update()
	{
		Spin();
	}

	private void Spin()
	{
		float num = ((!m_reverseRotation) ? m_rotationSpeed : (0f - m_rotationSpeed));
		float num2 = ((!m_ignoreTimeScale) ? Time.deltaTime : IndependantTimeDelta.Delta);
		base.transform.Rotate(0f, 0f, num * num2);
	}
}
