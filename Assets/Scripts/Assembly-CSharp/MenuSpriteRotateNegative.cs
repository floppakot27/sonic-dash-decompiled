using UnityEngine;

public class MenuSpriteRotateNegative : MonoBehaviour
{
	public bool m_ignoreTimeScale = true;

	public void Update()
	{
		Spin();
	}

	private void Spin()
	{
		float num = ((!m_ignoreTimeScale) ? Time.deltaTime : IndependantTimeDelta.Delta);
		base.transform.Rotate(0f, 0f, -60f * num);
	}
}
