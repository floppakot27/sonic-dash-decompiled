using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
	private ParticleSystem m_particleSystem;

	[SerializeField]
	private bool m_clearOnPlay;

	[SerializeField]
	private bool m_disableOnStop;

	private void Start()
	{
		m_particleSystem = GetComponent<ParticleSystem>();
	}

	private void OnEnable()
	{
		StartEmit();
	}

	private void OnDisable()
	{
		StopEmit();
	}

	public void StartEmit()
	{
		if ((bool)m_particleSystem)
		{
			base.particleSystem.gameObject.SetActive(value: true);
			if (m_clearOnPlay)
			{
				m_particleSystem.Clear();
			}
			m_particleSystem.Play();
		}
	}

	public void StopEmit()
	{
		if ((bool)m_particleSystem)
		{
			base.particleSystem.Stop();
			if (m_disableOnStop)
			{
				base.particleSystem.gameObject.SetActive(value: false);
			}
		}
	}
}
