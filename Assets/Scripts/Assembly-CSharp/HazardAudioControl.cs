using UnityEngine;

public class HazardAudioControl : MonoBehaviour
{
	private static HazardAudioControl s_singleton;

	[SerializeField]
	private AudioClip m_onDeathAudioClip;

	public static HazardAudioControl Singleton => s_singleton;

	private void Start()
	{
		s_singleton = this;
	}

	public void PlayWallDeathSfx()
	{
		if (m_onDeathAudioClip != null)
		{
			Audio.PlayClip(m_onDeathAudioClip, loop: false);
		}
	}
}
