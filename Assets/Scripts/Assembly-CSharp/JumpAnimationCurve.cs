using UnityEngine;

public class JumpAnimationCurve : IJumpCurve
{
	private float m_groundHeight;

	private AnimationCurve m_jumpAnimCurve;

	public float JumpDuration => m_jumpAnimCurve.keys[m_jumpAnimCurve.length - 1].time;

	public JumpAnimationCurve(float groundHeight, AnimationCurve jumpAnimCurve)
	{
		m_groundHeight = groundHeight;
		m_jumpAnimCurve = jumpAnimCurve;
	}

	public float CalculateHeight(float atTime)
	{
		return m_groundHeight + m_jumpAnimCurve.Evaluate(atTime);
	}
}
