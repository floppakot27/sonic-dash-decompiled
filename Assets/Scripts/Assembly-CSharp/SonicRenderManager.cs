using UnityEngine;

public class SonicRenderManager : MonoBehaviour
{
	private const int InvalidQueueOrder = -1;

	private const int ForcedQueueOrder = 3500;

	public ParticleSystem m_dashParticles;

	public ParticleSystem m_magnetParticles;

	public ParticleSystem m_shieldParticles;

	public ParticleSystem m_shieldEndParticles;

	public ParticleSystem m_headstartParticles;

	public ParticleSystem m_ringPickup10Particles;

	public ParticleSystem m_ringPickup20Particles;

	public ParticleSystem m_ringPickup50Particles;

	public ParticleSystem m_ringPickup100Particles;

	public ParticleSystem m_MegaHeadStartParticles;

	public ParticleSystem m_RingStreakParticles;

	public ParticleSystem m_RSRGetParticles;

	public ParticleSystem m_GCCGetParticles;

	public ParticleSystem m_DCPieceCollectedParticles;

	public ParticleSystem m_SpinBallParticlesLevel1;

	public ParticleSystem m_SpinBallParticlesLevel2;

	public ParticleSystem m_SpinBallParticlesLevel3;

	public ParticleSystem m_SpinBallDashEnd;

	public Color m_trailColor = Color.white;

	private int[] m_originalRenderQueueOrder;

	private Renderer[] m_storedRenders;

	private bool m_spinning;

	private int m_spinChargeLevel = 1;

	private void Start()
	{
		m_storedRenders = GetComponentsInChildren<Renderer>(includeInactive: true);
		int materialCount = GetMaterialCount(m_storedRenders);
		m_originalRenderQueueOrder = new int[materialCount];
		StoreCurrentQueueOrder(ref m_originalRenderQueueOrder, m_storedRenders);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("EnterMotionRollState", this);
		EventDispatch.RegisterInterest("SpinballCharge", this);
		EventDispatch.RegisterInterest("ExitMotionRollState", this);
		EventDispatch.RegisterInterest("LeavingRollState", this);
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_EnterMotionRollState()
	{
		m_spinning = true;
		m_spinChargeLevel = 1;
	}

	private void Event_SpinballCharge(int level)
	{
		m_spinChargeLevel = level;
	}

	private void Event_ExitMotionRollState()
	{
		m_spinning = false;
	}

	private void Event_LeavingRollState()
	{
		if (m_SpinBallDashEnd != null)
		{
			m_SpinBallDashEnd.gameObject.SetActive(value: true);
			m_SpinBallDashEnd.Play();
		}
	}

	private void Update()
	{
		updateDashParticles();
		updateHeadstartParticles();
		updateMagnetParticles();
		updateShieldParticles();
		updateSpinballChargeParticles();
	}

	private void updateDashParticles()
	{
		if (isDashing())
		{
			if (!m_dashParticles.isPlaying)
			{
				ParticlePlayer.Play(m_dashParticles);
			}
		}
		else if (m_dashParticles != null && m_dashParticles.isPlaying)
		{
			m_dashParticles.Stop();
			m_dashParticles.gameObject.SetActive(value: false);
		}
	}

	private bool isDashing()
	{
		if (DashMonitor.instance().isDashing())
		{
			return true;
		}
		Boss instance = Boss.GetInstance();
		if (instance != null)
		{
			BossAttack bossAttack = instance.AttackPhase();
			if (bossAttack != null)
			{
				return bossAttack.AttackTimerActive();
			}
		}
		return false;
	}

	private void updateHeadstartParticles()
	{
		if (HeadstartMonitor.instance().isHeadstarting())
		{
			ParticleSystem particleSystem = null;
			particleSystem = ((!HeadstartMonitor.instance().isSuperHeadstart()) ? m_headstartParticles : m_MegaHeadStartParticles);
			if (HeadstartMonitor.instance().isHeadstartNearlyFinished())
			{
				if (particleSystem.isPlaying)
				{
					particleSystem.Stop();
				}
			}
			else if (!particleSystem.isPlaying)
			{
				ParticlePlayer.Play(particleSystem, ParticlePlayer.Important.Yes);
			}
		}
		else
		{
			if (m_headstartParticles != null && m_headstartParticles.isPlaying)
			{
				m_headstartParticles.Stop();
			}
			if (m_MegaHeadStartParticles != null && m_MegaHeadStartParticles.isPlaying)
			{
				m_MegaHeadStartParticles.Stop();
			}
		}
	}

	private void updateMagnetParticles()
	{
		if (m_magnetParticles == null)
		{
			return;
		}
		if (MagnetMonitor.instance().isMagnetised())
		{
			if (MagnetMonitor.instance().isMagnetNearlyFinished())
			{
				if (m_magnetParticles.isPlaying)
				{
					m_magnetParticles.Stop();
					m_magnetParticles.gameObject.SetActive(value: false);
				}
			}
			else if (!m_magnetParticles.gameObject.activeInHierarchy || !m_magnetParticles.isPlaying)
			{
				m_magnetParticles.gameObject.SetActive(value: true);
				ParticlePlayer.Play(m_magnetParticles);
			}
		}
		else if (m_magnetParticles.isPlaying)
		{
			m_magnetParticles.Stop();
			m_magnetParticles.gameObject.SetActive(value: false);
		}
	}

	public void updateShieldParticles()
	{
		if (m_shieldParticles == null)
		{
			return;
		}
		if (ShieldMonitor.instance().isShielded())
		{
			if (ShieldMonitor.instance().isShieldNearlyFinished())
			{
				if (m_shieldParticles.isPlaying)
				{
					m_shieldParticles.Stop();
					m_shieldParticles.gameObject.SetActive(value: false);
				}
				if (!m_shieldEndParticles.isPlaying)
				{
					m_shieldEndParticles.Play();
				}
			}
			else if (!m_shieldParticles.gameObject.activeInHierarchy || !m_shieldParticles.isPlaying)
			{
				m_shieldParticles.gameObject.SetActive(value: true);
				ParticlePlayer.Play(m_shieldParticles);
			}
		}
		else if (m_shieldParticles.isPlaying)
		{
			m_shieldParticles.Stop();
			m_shieldParticles.gameObject.SetActive(value: false);
		}
	}

	public void playRingPickupParticles(int ringCount)
	{
		ParticleSystem particleSystem = null;
		switch (ringCount)
		{
		case 10:
			particleSystem = m_ringPickup10Particles;
			break;
		case 20:
			particleSystem = m_ringPickup20Particles;
			break;
		case 50:
			particleSystem = m_ringPickup50Particles;
			break;
		case 100:
			particleSystem = m_ringPickup100Particles;
			break;
		}
		if ((bool)particleSystem)
		{
			ParticlePlayer.Play(particleSystem);
		}
	}

	public void playRingStreakParticles()
	{
		if ((bool)m_RingStreakParticles)
		{
			if (m_RingStreakParticles.isPlaying)
			{
				m_RingStreakParticles.Stop();
			}
			ParticlePlayer.Play(m_RingStreakParticles);
		}
	}

	public void playRSRCollectedParticles()
	{
		if ((bool)m_RSRGetParticles)
		{
			if (m_RSRGetParticles.isPlaying)
			{
				m_RSRGetParticles.Stop();
			}
			ParticlePlayer.Play(m_RSRGetParticles);
		}
	}

	public void playGCCCollectedParticles()
	{
		if ((bool)m_GCCGetParticles)
		{
			if (m_GCCGetParticles.isPlaying)
			{
				m_GCCGetParticles.Stop();
			}
			ParticlePlayer.Play(m_GCCGetParticles);
		}
	}

	public void playDCPieceCollectedParticles()
	{
		if ((bool)m_DCPieceCollectedParticles)
		{
			if (m_DCPieceCollectedParticles.isPlaying)
			{
				m_DCPieceCollectedParticles.Stop();
			}
			ParticlePlayer.Play(m_DCPieceCollectedParticles);
		}
	}

	private void updateSpinballChargeParticles()
	{
		if (m_SpinBallParticlesLevel1 == null || m_SpinBallParticlesLevel2 == null || m_SpinBallParticlesLevel3 == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (m_spinning)
		{
			flag = m_spinChargeLevel == 1;
			flag2 = m_spinChargeLevel == 2;
			flag3 = m_spinChargeLevel == 3;
		}
		if (flag)
		{
			if (!m_SpinBallParticlesLevel1.gameObject.activeInHierarchy || !m_SpinBallParticlesLevel1.isPlaying)
			{
				ParticlePlayer.Play(m_SpinBallParticlesLevel1);
			}
		}
		else if (m_SpinBallParticlesLevel1.isPlaying)
		{
			m_SpinBallParticlesLevel1.Stop();
		}
		if (flag2)
		{
			if (!m_SpinBallParticlesLevel2.gameObject.activeInHierarchy || !m_SpinBallParticlesLevel2.isPlaying)
			{
				ParticlePlayer.Play(m_SpinBallParticlesLevel2);
			}
		}
		else if (m_SpinBallParticlesLevel2.isPlaying)
		{
			m_SpinBallParticlesLevel2.Stop();
		}
		if (flag3)
		{
			if (!m_SpinBallParticlesLevel3.gameObject.activeInHierarchy || !m_SpinBallParticlesLevel3.isPlaying)
			{
				ParticlePlayer.Play(m_SpinBallParticlesLevel3);
			}
		}
		else if (m_SpinBallParticlesLevel3.isPlaying)
		{
			m_SpinBallParticlesLevel3.Stop();
		}
		if (m_SpinBallDashEnd != null && m_SpinBallDashEnd.isPlaying && (Sonic.Tracker.isJumping() || Sonic.Tracker.IsFalling()))
		{
			m_SpinBallDashEnd.Stop();
			m_SpinBallDashEnd.gameObject.SetActive(value: false);
		}
	}

	private int GetMaterialCount(Renderer[] renderers)
	{
		int num = 0;
		foreach (Renderer renderer in renderers)
		{
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				num++;
			}
		}
		return num;
	}

	private void StoreCurrentQueueOrder(ref int[] values, Renderer[] renderers)
	{
		int num = 0;
		foreach (Renderer renderer in renderers)
		{
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				values[num] = renderer.materials[j].renderQueue;
				num++;
			}
		}
	}

	private void SetMaterialQueueOrder(Renderer[] renderers, int order)
	{
		int num = 0;
		foreach (Renderer renderer in renderers)
		{
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				int renderQueue = ((order != -1) ? order : m_originalRenderQueueOrder[num]);
				renderer.materials[j].renderQueue = renderQueue;
				num++;
			}
		}
	}

	private void Event_ResetGameState(GameState.Mode nextMode)
	{
		if (nextMode == GameState.Mode.Menu)
		{
			SetMaterialQueueOrder(m_storedRenders, 3500);
		}
	}

	private void Event_OnNewGameStarted()
	{
		SetMaterialQueueOrder(m_storedRenders, -1);
		m_spinning = false;
		m_spinChargeLevel = 1;
	}
}
