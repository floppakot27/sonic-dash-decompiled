using UnityEngine;

public class RingStreak : MonoBehaviour
{
	public float m_dashMeterPerStreak = 0.1f;

	private RingGenerator m_ringGenerator;

	private void Start()
	{
		m_ringGenerator = GetComponent<RingGenerator>();
	}

	private void Update()
	{
		RingSequence[] sequences = m_ringGenerator.GetSequences();
		RingSequence[] array = sequences;
		foreach (RingSequence ringSequence in array)
		{
			if (ringSequence.StreakCompleted)
			{
				ringSequence.GenerateStreakEvent("OnRingStreakCompleted");
				Sonic.RenderManager.playRingStreakParticles();
			}
		}
	}
}
