using UnityEngine;

[AddComponentMenu("Dash/Enemies/Generic SFX Controller")]
internal class EnemySFXController : MonoBehaviour
{
	[SerializeField]
	private AudioClip m_deathAudioClip;

	[SerializeField]
	private AudioClip m_goldenDeathAudioClip;

	[SerializeField]
	private AudioClip m_sonicDeathAudioClip;

	private Enemy m_enemy;

	public void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
		if (m_sonicDeathAudioClip != null)
		{
			Audio.PlayClip(m_sonicDeathAudioClip, loop: false);
		}
	}

	public void OnDeath(object[] onDeathParams)
	{
		if (m_enemy != null && m_enemy.Golden && m_goldenDeathAudioClip != null)
		{
			Audio.PlayClip(m_goldenDeathAudioClip, loop: false);
		}
		else if (m_deathAudioClip != null)
		{
			Audio.PlayClip(m_deathAudioClip, loop: false);
		}
	}

	private void Start()
	{
		m_enemy = base.gameObject.GetComponent<Enemy>();
	}
}
