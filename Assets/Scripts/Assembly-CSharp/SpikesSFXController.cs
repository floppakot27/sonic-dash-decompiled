using UnityEngine;

[AddComponentMenu("Dash/Enemies/Spikes SFX Controller")]
internal class SpikesSFXController : EnemySFXController
{
	[SerializeField]
	private AudioClip m_chunkBounceSFXClip;

	public void OnChunkBounce(Vector3 bouncePos)
	{
		if (!(m_chunkBounceSFXClip != null))
		{
		}
	}
}
