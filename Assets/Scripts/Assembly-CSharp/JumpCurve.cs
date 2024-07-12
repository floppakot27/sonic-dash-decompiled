using UnityEngine;

public class JumpCurve : IJumpCurve
{
	private float m_jumpDuration;

	private float m_jumpHeight;

	public float JumpDuration => m_jumpDuration;

	public float JumpHeight => m_jumpHeight;

	public JumpCurve(float initialHeight, float jumpHeight, float jumpDuration)
	{
		m_jumpDuration = jumpDuration;
		m_jumpHeight = jumpHeight;
	}

	public float CalculateHeight(float time)
	{
		if (m_jumpDuration > 0f)
		{
			float value = time / m_jumpDuration;
			value = Mathf.Clamp(value, 0f, 1f);
			return m_jumpHeight * (1f - (value * 2f - 1f) * (value * 2f - 1f));
		}
		return 0f;
	}
}
