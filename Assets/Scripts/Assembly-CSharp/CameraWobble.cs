using System;
using UnityEngine;

public class CameraWobble : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_wobbleFrequency = Vector2.zero;

	[SerializeField]
	private Vector2 m_wobbleAmplitude = Vector2.zero;

	[SerializeField]
	private float m_smoothTime = 1f;

	private float m_timer;

	private float m_intensity;

	private float m_velocity;

	private Vector2 m_frequency;

	private Vector2 m_amplitude;

	public static CameraWobble Instance { get; private set; }

	public static void WobbleCamera(float intensity)
	{
		if ((bool)Instance)
		{
			Instance.Wobble(intensity);
		}
	}

	public static void WobbleCamera(float intensity, Vector2 frequency, Vector2 amplitude)
	{
		if ((bool)Instance)
		{
			Instance.Wobble(intensity, frequency, amplitude);
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		m_timer += Time.deltaTime;
		m_intensity = Mathf.SmoothDamp(m_intensity, 0f, ref m_velocity, m_smoothTime);
		float x = Mathf.Sin(m_timer * m_frequency.x * (float)Math.PI * 2f) * m_amplitude.x * m_intensity;
		float y = Mathf.Sin(m_timer * m_frequency.y * (float)Math.PI * 2f) * m_amplitude.y * m_intensity;
		Vector3 direction = new Vector3(x, y, 0f);
		direction = base.transform.TransformDirection(direction);
		base.transform.localPosition = direction;
	}

	public void Wobble(float intensity)
	{
		m_intensity = intensity;
		m_frequency = m_wobbleFrequency;
		m_amplitude = m_wobbleAmplitude;
		m_timer = 0f;
	}

	public void Wobble(float intensity, Vector2 frequency, Vector2 amplitude)
	{
		m_intensity = intensity;
		m_frequency = frequency;
		m_amplitude = amplitude;
		m_timer = 0f;
	}
}
