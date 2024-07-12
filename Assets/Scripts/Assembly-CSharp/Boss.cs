using UnityEngine;

public class Boss : MonoBehaviour
{
	private static Boss s_instance;

	private BossMissileChase m_missileChase;

	private BossMineDeployment m_mineDeployment;

	private BossFlyBy m_flyby;

	private BossFlyOff m_flyOff;

	private BossAttack m_attack;

	[SerializeField]
	private SpawnPool m_pool;

	[SerializeField]
	private Transform m_lookAtPoint;

	public BossVisualController VisualController { get; private set; }

	public BossAnimationController AnimationController { get; private set; }

	public BossAudioController AudioController { get; private set; }

	public Transform LookAtPoint => m_lookAtPoint;

	public static Boss GetInstance()
	{
		return s_instance;
	}

	public BossAttack AttackPhase()
	{
		return m_attack;
	}

	private void Awake()
	{
		s_instance = this;
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
	}

	private void OnDisable()
	{
		s_instance = null;
	}

	private void Start()
	{
		VisualController = GetComponent<BossVisualController>();
		AnimationController = GetComponent<BossAnimationController>();
		AudioController = GetComponent<BossAudioController>();
		m_missileChase = GetComponent<BossMissileChase>();
		m_mineDeployment = GetComponent<BossMineDeployment>();
		m_flyby = GetComponent<BossFlyBy>();
		m_flyOff = GetComponent<BossFlyOff>();
		m_attack = GetComponent<BossAttack>();
		m_missileChase.enabled = false;
		m_flyby.enabled = false;
		m_mineDeployment.enabled = false;
		m_attack.enabled = false;
	}

	private void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		bool visible = true;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		switch (phase.Type)
		{
		case BossBattleSystem.Phase.Types.Arrive:
			visible = false;
			break;
		case BossBattleSystem.Phase.Types.Intro:
			flag2 = true;
			break;
		case BossBattleSystem.Phase.Types.TransitionToAttack1:
			visible = false;
			break;
		case BossBattleSystem.Phase.Types.Attack1Intro:
			flag = true;
			break;
		case BossBattleSystem.Phase.Types.Attack1:
			flag = true;
			break;
		case BossBattleSystem.Phase.Types.TransitionToAttack2:
			flag4 = true;
			m_mineDeployment.StartBehaviour(phase.Duration);
			FrontCamera.Instance.TransitionToBackCamera(phase.Duration, m_lookAtPoint);
			break;
		case BossBattleSystem.Phase.Types.Attack2:
			flag4 = true;
			m_mineDeployment.SetGameplay(phase.m_attackSettings, m_pool);
			break;
		case BossBattleSystem.Phase.Types.TransitionToVulnerable:
			flag3 = true;
			break;
		case BossBattleSystem.Phase.Types.Vulnerable:
			flag5 = true;
			flag3 = true;
			break;
		case BossBattleSystem.Phase.Types.Leave:
			visible = false;
			break;
		case BossBattleSystem.Phase.Types.Finish:
			Sonic.ScoreAnchor.SetActive(value: false);
			visible = false;
			break;
		}
		if ((bool)VisualController)
		{
			VisualController.Visible = visible;
		}
		if ((bool)m_missileChase)
		{
			m_missileChase.enabled = flag;
			if (flag)
			{
				m_missileChase.SetGameplay(phase.m_attackSettings, m_pool);
			}
		}
		if ((bool)m_mineDeployment)
		{
			m_mineDeployment.enabled = flag4;
		}
		if ((bool)m_flyby)
		{
			m_flyby.enabled = flag2;
		}
		if ((bool)m_attack)
		{
			m_attack.enabled = flag5;
		}
		if ((bool)m_flyOff)
		{
			m_flyOff.enabled = flag3;
		}
	}
}
