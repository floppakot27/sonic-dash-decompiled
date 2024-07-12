using System;
using UnityEngine;

[Serializable]
public class AudioClipManager
{
	private bool m_bActive;

	private float m_fTimer;

	private int m_nTableIndex;

	private int[] m_random;

	private float m_fFrequencyMultiplier = 1f;

	[SerializeField]
	private AudioClip[] m_clipList;

	[SerializeField]
	private float m_frequency = 1f;

	public AudioClipManager(AudioClip[] clipList)
	{
		m_clipList = clipList;
		m_fTimer = 0f;
		m_bActive = false;
		m_nTableIndex = 0;
		m_fFrequencyMultiplier = 1f;
	}

	public void BeginPlaying()
	{
		if (m_clipList.Length > 0)
		{
			m_bActive = true;
			m_fTimer = 0f;
			int num = m_clipList.Length;
			m_random = new int[num * 5];
			for (int i = 0; i < m_random.Length; i++)
			{
				m_random[i] = UnityEngine.Random.Range(0, num);
			}
		}
	}

	public void EndPlaying()
	{
		m_bActive = false;
	}

	public void Update()
	{
		if (!m_bActive)
		{
			return;
		}
		float num = m_frequency * m_fFrequencyMultiplier;
		if ((double)num > 1E-06 && m_fTimer >= 1f / num)
		{
			m_fTimer = 0f;
			if (m_clipList.Length > 0)
			{
				m_nTableIndex++;
				if (m_nTableIndex >= m_random.Length)
				{
					m_nTableIndex = 0;
				}
				int num2 = m_random[m_nTableIndex];
				Audio.PlayClip(m_clipList[num2], loop: false);
			}
		}
		else
		{
			m_fTimer += Time.deltaTime;
		}
	}

	public AudioClip GetRandomClip()
	{
		if (m_clipList.Length > 0)
		{
			int num = UnityEngine.Random.Range(0, m_clipList.Length);
			return m_clipList[num];
		}
		return null;
	}

	public void SetFrequencyMultiplier(float fMultiplier)
	{
		m_fFrequencyMultiplier = fMultiplier;
	}
}
