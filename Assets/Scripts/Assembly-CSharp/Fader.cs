using UnityEngine;

public class Fader : MonoBehaviour
{
	public Material m_faderMaterial;

	private bool m_flashRequest;

	private float m_requestedFlashDuration;

	private bool m_flashActive;

	private float m_time;

	private float m_currentFlashDuration;

	private float m_blend;

	private void Start()
	{
		m_flashActive = false;
	}

	private void Update()
	{
		if (m_flashRequest)
		{
			m_flashActive = true;
			m_time = 0f;
			m_currentFlashDuration = m_requestedFlashDuration;
			m_flashRequest = false;
		}
		else
		{
			m_time += Time.deltaTime;
		}
		if (m_flashActive)
		{
			if (m_time > m_currentFlashDuration)
			{
				m_flashActive = false;
			}
			m_blend = 1f - m_time / m_currentFlashDuration;
		}
	}

	public void flash(float duration)
	{
		m_flashRequest = true;
		m_requestedFlashDuration = duration;
	}

	private void OnPostRender()
	{
		if (m_flashActive && (bool)m_faderMaterial)
		{
			GL.PushMatrix();
			GL.LoadOrtho();
			m_faderMaterial.SetPass(0);
			GL.Begin(7);
			GL.Color(new Color(m_blend, m_blend, m_blend, m_blend));
			GL.Vertex3(0f, 0f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.Vertex3(1f, 1f, 0f);
			GL.Vertex3(0f, 1f, 0f);
			GL.End();
			GL.PopMatrix();
		}
	}
}
