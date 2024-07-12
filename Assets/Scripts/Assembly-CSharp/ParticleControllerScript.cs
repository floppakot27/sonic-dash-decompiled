using UnityEngine;

public class ParticleControllerScript : MonoBehaviour
{
	public ParticleSystem m_dashParticles;

	public ParticleSystem m_headstartParticles;

	public ParticleSystem m_respawnParticles;

	public ParticleSystem m_freeRespawnParticles;

	public ParticleSystem m_magnetParticles;

	public ParticleSystem m_shieldParticles;

	public ParticleSystem m_shieldEndParticles;

	public ParticleSystem m_AirborneParticles;

	public ParticleSystem m_MegaHeadStartParticles;

	public ParticleSystem m_RingStreakParticles;

	public ParticleSystem m_RSRGetParticles;

	public ParticleSystem m_GCCGetParticles;

	public ParticleSystem m_DCPieceCollectedParticles;

	public ParticleSystem m_SpinBallParticlesLevel1;

	public ParticleSystem m_SpinBallParticlesLevel2;

	public ParticleSystem m_SpinBallParticlesLevel3;

	public ParticleSystem m_SpinBallDashEnd;

	public ParticleSystem m_ringPickup10Particles;

	public ParticleSystem m_ringPickup20Particles;

	public ParticleSystem m_ringPickup50Particles;

	public ParticleSystem m_ringPickup100Particles;

	public ParticleSystem m_goldenEnemiKillParticles;

	public ParticleSystem m_goldnikAura;

	public GameObject m_respawnPosition;

	public GameObject m_floorPosition;

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnGameReset", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		m_respawnParticles.transform.localPosition = Vector3.zero;
		m_freeRespawnParticles.transform.localPosition = Vector3.zero;
		m_SpinBallDashEnd.transform.localPosition = new Vector3(0f, 0.3f, 0f);
		base.transform.localPosition = Vector3.zero;
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_OnNewGameStarted()
	{
		stopAndClearParticles();
	}

	private void Event_OnGameFinished()
	{
		stopAndClearParticles();
	}

	private void Event_OnGameReset()
	{
		stopAndClearParticles();
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		stopAndClearParticles();
	}

	private void stopAndClearParticles()
	{
		m_dashParticles.Stop();
		m_dashParticles.Clear();
		m_headstartParticles.Stop();
		m_headstartParticles.Clear();
		m_respawnParticles.Stop();
		m_respawnParticles.Clear();
		m_freeRespawnParticles.Stop();
		m_freeRespawnParticles.Clear();
		m_magnetParticles.Stop();
		m_magnetParticles.Clear();
		m_shieldParticles.Stop();
		m_shieldParticles.Clear();
		m_shieldEndParticles.Stop();
		m_shieldEndParticles.Clear();
		m_ringPickup10Particles.Stop();
		m_ringPickup10Particles.Clear();
		m_ringPickup20Particles.Stop();
		m_ringPickup20Particles.Clear();
		m_ringPickup50Particles.Stop();
		m_ringPickup50Particles.Clear();
		m_ringPickup100Particles.Stop();
		m_ringPickup100Particles.Clear();
		m_AirborneParticles.Stop();
		m_AirborneParticles.Clear();
		m_MegaHeadStartParticles.Stop();
		m_MegaHeadStartParticles.Clear();
		m_RingStreakParticles.Stop();
		m_RingStreakParticles.Clear();
		m_RSRGetParticles.Stop();
		m_RSRGetParticles.Clear();
		m_DCPieceCollectedParticles.Stop();
		m_DCPieceCollectedParticles.Clear();
		m_SpinBallParticlesLevel1.Stop();
		m_SpinBallParticlesLevel1.Clear();
		m_SpinBallParticlesLevel2.Stop();
		m_SpinBallParticlesLevel2.Clear();
		m_SpinBallParticlesLevel3.Stop();
		m_SpinBallParticlesLevel3.Clear();
		m_SpinBallDashEnd.Stop();
		m_SpinBallDashEnd.Clear();
		m_goldenEnemiKillParticles.Stop();
		m_goldnikAura.Clear();
	}
}
