using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Sonic/Animation Control")]
public class SonicAnimationControl : MonoBehaviour
{
	public enum SplatType
	{
		Stationary,
		Backwards,
		Forwards
	}

	[Serializable]
	private class SonicParticleSystem
	{
		public enum PlayRequest
		{
			Play,
			DoNotPlay,
			FinishCurrentPlay
		}

		[SerializeField]
		private ParticleSystem m_particles;

		[SerializeField]
		private bool m_stickToGround;

		private bool m_isPlaying;

		public void Awake()
		{
			m_isPlaying = false;
			if (m_particles != null)
			{
				m_particles.Stop();
			}
		}

		public void Play()
		{
			m_isPlaying = true;
			ParticlePlayer.Play(m_particles);
		}

		public void Update(PlayRequest shouldPlay, Transform sonic, float jumpHeight)
		{
			if (m_particles == null)
			{
				return;
			}
			if (m_isPlaying)
			{
				switch (shouldPlay)
				{
				case PlayRequest.DoNotPlay:
					m_particles.Stop();
					m_isPlaying = false;
					break;
				case PlayRequest.FinishCurrentPlay:
					if (!m_particles.IsAlive())
					{
						m_isPlaying = false;
					}
					break;
				}
			}
			else if (shouldPlay == PlayRequest.Play)
			{
				ParticlePlayer.Play(m_particles);
				m_isPlaying = true;
			}
			if (m_isPlaying)
			{
				Vector3 vector = ((!m_stickToGround) ? Vector3.zero : (sonic.up * (0f - jumpHeight)));
				m_particles.transform.position = sonic.position + vector;
			}
			m_particles.transform.rotation = Quaternion.LookRotation(-sonic.forward, sonic.up);
		}
	}

	[Serializable]
	private class BankVariableAnim
	{
		public string Regular = string.Empty;

		public string BankVariant = string.Empty;

		public string BossSuccessVariant = string.Empty;

		public string BossFailureVariant = string.Empty;
	}

	[Serializable]
	private class BossAttackAnim
	{
		public string Ready = string.Empty;

		public string Idle = string.Empty;

		public string Recoil = string.Empty;

		public string RecoilToSuccess = string.Empty;
	}

	private struct SpringDanceAnim
	{
		public CartesianDir Direction;

		public string Name;

		public SpringDanceAnim(CartesianDir dir, string name)
		{
			Direction = dir;
			Name = name;
		}
	}

	private const float m_ghostFrequency = 0.25f;

	[SerializeField]
	private bool m_logAnimations;

	[SerializeField]
	private string m_jogAnimName = "run";

	[SerializeField]
	private float m_jogAnimSpeed = 10f;

	[SerializeField]
	private string m_runAnimName = "fastrun";

	[SerializeField]
	private float m_runAnimSpeed = 15f;

	[SerializeField]
	private float m_runCrossoverMinSpeed = 12f;

	[SerializeField]
	private float m_runCrossoverMaxSpeed = 14f;

	[SerializeField]
	private string m_hopLeftName = "hopleft";

	[SerializeField]
	private string m_hopRightName = "hopright";

	[SerializeField]
	private float m_hopCrossfadeDuration = 0.25f;

	[SerializeField]
	private float m_hopWalkNormalizedPoint = 0.675f;

	[SerializeField]
	private string m_backwardsSplatAnim = string.Empty;

	[SerializeField]
	private string m_stationarySplatAnim = string.Empty;

	[SerializeField]
	private string m_forwardsSplatAnim = string.Empty;

	[SerializeField]
	private string m_fallAnimName = string.Empty;

	[SerializeField]
	private string m_recoilName = string.Empty;

	[SerializeField]
	private string m_stumbleAnimName = string.Empty;

	[SerializeField]
	private string m_dashAnimName = string.Empty;

	[SerializeField]
	private string m_glideAnimName = string.Empty;

	[SerializeField]
	private string m_inRollName = string.Empty;

	[SerializeField]
	private string m_outRollName = string.Empty;

	[SerializeField]
	private MeshRenderer m_ball;

	private SkinnedMeshRenderer m_tailRenderer;

	[SerializeField]
	private GameObject m_character;

	[SerializeField]
	private float m_ballSpinMultiplier = 1.2f;

	[SerializeField]
	private SonicParticleSystem m_rollKickupParticles = new SonicParticleSystem();

	[SerializeField]
	private SonicParticleSystem m_spinParticles = new SonicParticleSystem();

	[SerializeField]
	private SonicParticleSystem m_groundSlamParticles = new SonicParticleSystem();

	[SerializeField]
	private ParticleSystem m_respawnParticles;

	[SerializeField]
	private ParticleSystem m_freeRespawnParticles;

	[SerializeField]
	private ParticleSystem m_splatParticles;

	[SerializeField]
	private ParticleSystem m_smashThroughParticles;

	[SerializeField]
	private ParticleSystem m_sploshParticles;

	[SerializeField]
	private ParticleSystem m_runParticlesLeft;

	[SerializeField]
	private ParticleSystem m_runParticlesRight;

	[SerializeField]
	private BankVariableAnim m_springLaunch = new BankVariableAnim();

	[SerializeField]
	private BossAttackAnim m_springBossAttack = new BossAttackAnim();

	[SerializeField]
	private BankVariableAnim m_springLaunchToGlide = new BankVariableAnim();

	[SerializeField]
	private BankVariableAnim m_springGlide = new BankVariableAnim();

	[SerializeField]
	private BankVariableAnim m_springPrepareToLand = new BankVariableAnim();

	[SerializeField]
	private string m_springLandIntoRun = "Snc_SpringLand";

	[SerializeField]
	private string m_springDanceKeyword = "Dance";

	private bool m_isBanking;

	private bool m_isBossSuccess;

	private bool m_isBossFailure;

	private IList<string>[] m_danceAnims;

	private System.Random m_rng = new System.Random();

	private Animation m_animComponent;

	private Collider m_ColliderSphere;

	private Collider m_ColliderCapsule;

	private float m_ghostTimer;

	private bool m_ghosted;

	private bool m_isGhostedBall;

	public float JogAnimSpeed => m_jogAnimSpeed;

	public float RunAnimSpeed => m_runAnimSpeed;

	private bool IsBallColliderEnabled
	{
		get
		{
			return m_ColliderSphere.enabled;
		}
		set
		{
			m_ColliderSphere.enabled = value;
			m_ColliderCapsule.enabled = !value;
		}
	}

	public bool IsBallShown
	{
		get
		{
			return m_ball.enabled;
		}
		set
		{
			if (IsBallShown != value)
			{
				Renderer[] componentsInChildren = m_character.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = !value;
				}
				EnableBall(value);
				if (!value)
				{
					IsBallColliderEnabled = false;
				}
				else
				{
					DisableFeetParticles();
				}
			}
		}
	}

	public bool IsBossAttackShown { get; set; }

	private string SpringLaunch => GetAnimName(m_springLaunch);

	private string SpringLaunchToGlide => GetAnimName(m_springLaunchToGlide);

	private string SpringGlide => GetAnimName(m_springGlide);

	private string SpringPrepareToLand => GetAnimName(m_springPrepareToLand);

	private void Awake()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSmashThrough", this);
		EventDispatch.RegisterInterest("OnSplosh", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnBossAttackGestureStart", this);
		EventDispatch.RegisterInterest("OnBossAttackGestureEndSuccess", this);
		EventDispatch.RegisterInterest("OnBossAttackGestureEndFailure", this);
		EventDispatch.RegisterInterest("OnSpringDescent", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("CharacterLoaded", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnSingleSpringGestureSuccess", this);
		m_animComponent = m_character.GetComponentInChildren<Animation>();
		m_tailRenderer = m_ball.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		m_ColliderSphere = GetComponent<SphereCollider>();
		m_ColliderCapsule = GetComponent<CapsuleCollider>();
		m_ColliderSphere.enabled = false;
		m_ColliderCapsule.enabled = true;
		m_rollKickupParticles.Awake();
		m_spinParticles.Awake();
		m_groundSlamParticles.Awake();
		m_ghostTimer = 0f;
		m_ghosted = false;
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_CharacterLoaded()
	{
		m_danceAnims = FindSpringDanceAnims();
		IEnumerable<string> enumerable = new string[11]
		{
			m_hopLeftName, m_hopRightName, m_stationarySplatAnim, m_backwardsSplatAnim, m_forwardsSplatAnim, m_recoilName, m_inRollName, m_outRollName, m_fallAnimName, m_dashAnimName,
			m_springLandIntoRun
		}.Concat(new BankVariableAnim[4] { m_springGlide, m_springLaunchToGlide, m_springLaunch, m_springPrepareToLand }.SelectMany((BankVariableAnim bankAnims) => new string[2] { bankAnims.Regular, bankAnims.BankVariant }));
		foreach (string item in enumerable)
		{
			m_animComponent[item].layer = 1;
		}
	}

	private void EnableBall(bool enable)
	{
		m_ball.enabled = enable;
		if ((bool)m_tailRenderer)
		{
			m_tailRenderer.enabled = enable;
			if (enable)
			{
				Animation component = m_tailRenderer.transform.parent.GetComponent<Animation>();
				component.Play();
			}
		}
	}

	public void OnGameReset(GameState.Mode mode)
	{
		Event_ResetGameState(mode);
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		if (mode != GameState.Mode.PauseMenu)
		{
			StopAllCoroutines();
			m_animComponent.Stop();
			if (mode == GameState.Mode.Game)
			{
				RestartRun();
				m_animComponent.Sample();
			}
			EnableBall(enable: true);
			IsBallShown = false;
			m_ghostTimer = 0f;
			m_ghosted = false;
			m_isBanking = false;
			m_isBossSuccess = false;
			m_isBossFailure = false;
			DisableFeetParticles();
		}
	}

	private void Event_OnSonicResurrection()
	{
		StopAllCoroutines();
		m_animComponent.Stop();
		RestartRun();
		m_animComponent.Sample();
		EnableBall(enable: true);
		IsBallShown = false;
	}

	private void Update()
	{
		if (!(null == Sonic.Tracker))
		{
			float num = Sonic.Tracker.Speed;
			if (Sonic.Tracker.InternalTracker != null && Sonic.Tracker.InternalTracker.IsReversed && Sonic.Tracker.InternalTracker.RunBackwards)
			{
				num = 0f - num;
			}
			UpdateRun(num);
			m_groundSlamParticles.Update(SonicParticleSystem.PlayRequest.FinishCurrentPlay, base.transform, Sonic.Tracker.JumpHeight);
			bool flag = IsBallShown && Sonic.Tracker.JumpHeight == 0f;
			m_rollKickupParticles.Update((!flag) ? SonicParticleSystem.PlayRequest.DoNotPlay : SonicParticleSystem.PlayRequest.Play, base.transform, Sonic.Tracker.JumpHeight);
			bool isBallShown = IsBallShown;
			m_spinParticles.Update((!isBallShown) ? SonicParticleSystem.PlayRequest.DoNotPlay : SonicParticleSystem.PlayRequest.Play, m_ball.transform, Sonic.Tracker.JumpHeight);
			UpdateGhost();
		}
	}

	private void UpdateGhost()
	{
		bool flag = Sonic.Tracker.GetIsGhosted();
		if (flag)
		{
			m_ghostTimer += Time.deltaTime;
			if (m_ghostTimer > 0.25f)
			{
				m_ghostTimer = 0f;
			}
			if (m_ghostTimer > 0.125f)
			{
				flag = false;
			}
		}
		if (m_ghosted == flag)
		{
			return;
		}
		if (m_isGhostedBall)
		{
			EnableBall(!flag);
		}
		else
		{
			Renderer[] componentsInChildren = m_character.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = !flag;
			}
			if (m_ball.enabled)
			{
				m_isGhostedBall = false;
				EnableBall(enable: false);
			}
		}
		m_ghosted = flag;
	}

	public void StartRollAnim(Pair<float, float> SpinUpTimeTargetVelocity)
	{
		ShowBall();
	}

	public void StopRollAnim()
	{
		m_animComponent.Stop();
		IsBallShown = false;
		m_isGhostedBall = false;
	}

	public void StartStumbleAnim()
	{
		IsBallShown = false;
		StopAllCoroutines();
		m_animComponent.CrossFade(m_stumbleAnimName, 0.2f);
	}

	public void StopStumbleAnim()
	{
		m_animComponent.Stop();
		RestartRun();
	}

	public void StartFallAnim()
	{
		IsBallShown = false;
		StopAllCoroutines();
		m_animComponent[m_fallAnimName].wrapMode = WrapMode.Loop;
		PlayAnimation(m_fallAnimName);
	}

	public void OnDashBegin()
	{
		IsBallShown = false;
		StopAllCoroutines();
		m_animComponent.CrossFade(m_dashAnimName, 0.2f);
	}

	public void OnDashEnd()
	{
		m_animComponent[m_dashAnimName].enabled = false;
		if (!Sonic.Tracker.GetIsOnSetPiece())
		{
			RestartRun();
		}
	}

	public void StartGlideAnim()
	{
		IsBallShown = false;
		StopAllCoroutines();
		m_animComponent.CrossFade(m_glideAnimName, 0.2f);
	}

	public void StopGlideAnim()
	{
		m_animComponent.Stop();
		RestartRun();
	}

	public void OnRewind()
	{
		m_animComponent.Stop();
		RestartRun();
	}

	public void StartJumpAnim(Pair<float, float> SpinUpTimeTargetVelocity)
	{
		ShowBall();
	}

	public void StopJumpAnim()
	{
		IsBallShown = false;
		m_isGhostedBall = false;
	}

	public void StartAttackAnim(Pair<float, float> SpinUpTimeTargetVelocity)
	{
		ShowBall();
	}

	public void StopAttackAnim()
	{
		IsBallShown = false;
	}

	public void StartBossAttackAnim()
	{
		ShowBossAttack();
	}

	public void StopBossAttackAnim()
	{
		IsBossAttackShown = false;
	}

	public void StartRecoilAnim(Pair<float, float> SpinUpTimeTargetVelocity)
	{
		StopAllCoroutines();
		StartRecoil();
	}

	public void StopRecoilAnim()
	{
		StopAllCoroutines();
	}

	public void OnSlam()
	{
		m_groundSlamParticles.Play();
	}

	public void Event_OnSmashThrough()
	{
		OneShotParticleFX(m_smashThroughParticles, attachToWorld: false, new Vector3(0f, 0.5f, 0f));
	}

	public void Event_OnSplosh()
	{
		OneShotParticleFX(m_sploshParticles, attachToWorld: false, new Vector3(0f, 0.1f, 0f));
		m_sploshParticles.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
		Renderer[] componentsInChildren = m_character.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = false;
		}
	}

	public void OnRespawn()
	{
		if (!Sonic.Tracker.GetIsOnSetPiece())
		{
			Renderer[] componentsInChildren = m_character.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = true;
			}
			IsBallShown = false;
		}
	}

	public void TriggerRespawnPowerupEffect(bool freeRevive)
	{
		if (freeRevive)
		{
			ClearAndPlayParticleFX(m_freeRespawnParticles);
		}
		else
		{
			ClearAndPlayParticleFX(m_respawnParticles);
		}
	}

	public void StartHopAnim(SideDirection direction)
	{
		IsBallShown = false;
		string text = ((direction != 0) ? m_hopRightName : m_hopLeftName);
		string text2 = ((direction != 0) ? m_hopLeftName : m_hopRightName);
		if (!m_animComponent.IsPlaying(text))
		{
			StopAllCoroutines();
			m_animComponent[text].speed = 1f;
			float length = m_animComponent[text].length;
			float num = Sonic.Handling.StrafeDuration * 0.9f;
			float num2 = num / length;
			m_animComponent[text].speed = 1f / num2;
			float normalizedTime = ((!m_animComponent[text2].enabled) ? 0f : Mathf.Max(0f, m_hopWalkNormalizedPoint - m_animComponent[text2].normalizedTime));
			m_animComponent[text].normalizedTime = normalizedTime;
			m_animComponent.CrossFade(text, m_hopCrossfadeDuration);
		}
	}

	public float StartSplatAnim(SplatType splatType)
	{
		IsBallShown = false;
		StopAllCoroutines();
		string text = string.Empty;
		switch (splatType)
		{
		case SplatType.Stationary:
			text = m_stationarySplatAnim;
			break;
		case SplatType.Backwards:
			text = m_backwardsSplatAnim;
			break;
		case SplatType.Forwards:
			text = m_forwardsSplatAnim;
			break;
		}
		m_animComponent[text].wrapMode = WrapMode.ClampForever;
		m_animComponent.CrossFade(text, 0.2f);
		OneShotParticleFX(m_splatParticles, attachToWorld: true, new Vector3(0f, 0.5f, 0f));
		return m_animComponent[text].length;
	}

	private void ShowBall()
	{
		if (!IsBallShown)
		{
			StopAllCoroutines();
			if (Sonic.Tracker.GetIsGhosted())
			{
				m_isGhostedBall = true;
			}
			StartCoroutine(PlaySpinAnim());
		}
	}

	private IEnumerator PlaySpinAnim()
	{
		IsBallColliderEnabled = true;
		AnimationState rollInAnim = m_animComponent[m_inRollName];
		rollInAnim.wrapMode = WrapMode.ClampForever;
		PlayAnimation(m_inRollName);
		while (rollInAnim.normalizedTime < 0.99f)
		{
			yield return null;
		}
		rollInAnim.enabled = false;
		yield return StartCoroutine(SpinAndOutAnim());
	}

	private IEnumerator SpinAndOutAnim()
	{
		IsBallColliderEnabled = true;
		float ballRadius = m_ball.transform.localPosition.y;
		float ballCircum = (float)Math.PI * 2f * ballRadius;
		IsBallShown = true;
		Action<float> spinBallForSpeed = delegate(float linearSpeed)
		{
			float num = 360f * linearSpeed / ballCircum;
			float angle = num * Time.deltaTime;
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.right);
			m_ball.transform.rotation *= quaternion;
			for (int i = 0; i < m_ball.transform.childCount; i++)
			{
				Transform child = m_ball.transform.GetChild(i);
				child.rotation = base.transform.rotation;
			}
		};
		while (IsBallShown)
		{
			spinBallForSpeed(Sonic.Tracker.Speed * m_ballSpinMultiplier);
			yield return null;
		}
		PlayAnimation(m_outRollName);
	}

	private void StartRecoil()
	{
		IsBallShown = false;
		if (!m_animComponent.IsPlaying(m_recoilName))
		{
			StopAllCoroutines();
			m_animComponent[m_recoilName].speed = 1f;
			m_animComponent[m_recoilName].normalizedTime = 0f;
			m_animComponent.CrossFade(m_recoilName, 0f);
		}
	}

	private void ShowBossAttack()
	{
		if (!IsBossAttackShown)
		{
			StopAllCoroutines();
			StartCoroutine(PlayAttackSpinAnim());
		}
	}

	private IEnumerator PlayAttackSpinAnim()
	{
		IsBallColliderEnabled = true;
		BossAttack phase = Boss.GetInstance().AttackPhase();
		float timeToHit = phase.CurrentAttackTime();
		AnimationState rollInAnim = m_animComponent[m_inRollName];
		rollInAnim.wrapMode = WrapMode.ClampForever;
		PlayAnimation(m_inRollName);
		while (rollInAnim.normalizedTime < 0.99f)
		{
			timeToHit -= Time.deltaTime;
			yield return null;
		}
		rollInAnim.enabled = false;
		yield return StartCoroutine(PlayAttackSpinAndOutAnim(timeToHit, phase.CurrentRecoilTime(), phase.CurrentTransform()));
	}

	private IEnumerator PlayAttackSpinAndOutAnim(float timeToHit, float timeToRecoil, Transform boneTransform)
	{
		IsBallColliderEnabled = true;
		float ballRadius = m_ball.transform.localPosition.y;
		float ballCircum = (float)Math.PI * 2f * ballRadius;
		IsBallShown = true;
		Action<float> spinBallForSpeed = delegate(float linearSpeed)
		{
			float num = 360f * linearSpeed / ballCircum;
			float angle = num * Time.deltaTime;
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.right);
			m_ball.transform.rotation *= quaternion;
			for (int i = 0; i < m_ball.transform.childCount; i++)
			{
				Transform child = m_ball.transform.GetChild(i);
				child.rotation = base.transform.rotation;
			}
		};
		Vector3 startPos = base.gameObject.transform.position;
		Vector3 endPos = boneTransform.position;
		float endTimeInv = 1f / timeToHit;
		while (timeToHit > 0f)
		{
			timeToHit -= Time.deltaTime;
			base.gameObject.transform.position = Vector3.Lerp(startPos, endPos, 1f - timeToHit * endTimeInv);
			spinBallForSpeed(Sonic.Tracker.Speed * m_ballSpinMultiplier);
			yield return null;
		}
		base.gameObject.transform.position = endPos;
		IsBallShown = false;
		m_animComponent.Stop();
		bool recoilPlaying = true;
		AnimationState recoilAnim = m_animComponent[m_springBossAttack.Recoil];
		recoilAnim.wrapMode = WrapMode.ClampForever;
		PlayAnimation(m_springBossAttack.Recoil);
		endTimeInv = 1f / timeToRecoil;
		while (timeToRecoil > 0f)
		{
			timeToRecoil -= Time.deltaTime;
			base.gameObject.transform.position = Vector3.Lerp(endPos, startPos, 1f - timeToRecoil * endTimeInv);
			if (recoilPlaying && recoilAnim.normalizedTime > 0.99f)
			{
				recoilPlaying = false;
				m_animComponent.Stop();
				PlayAnimation(m_springBossAttack.Idle);
			}
			yield return null;
		}
		base.gameObject.transform.position = startPos;
		while (recoilPlaying && recoilAnim.normalizedTime < 0.99f)
		{
			yield return null;
		}
		recoilPlaying = false;
		m_animComponent.Stop();
		PlayAnimation(m_springBossAttack.Idle);
	}

	private IEnumerator BlendTo(string animationName, float blendDuration, float blendTarget)
	{
		float startWeight = m_animComponent[animationName].weight;
		float blendTimer = blendDuration;
		while (blendTimer > 0f)
		{
			m_animComponent[animationName].weight = Utils.MapValue(blendTimer, blendDuration, 0f, startWeight, blendTarget);
			blendTimer -= Time.deltaTime;
			yield return null;
		}
		m_animComponent[animationName].weight = blendTarget;
	}

	private void DebugLog()
	{
		foreach (AnimationState item in m_animComponent)
		{
			if (item.enabled)
			{
			}
		}
	}

	public void RestartRun()
	{
		string[] array = new string[2] { m_jogAnimName, m_runAnimName };
		foreach (string text in array)
		{
			m_animComponent.Rewind(text);
			m_animComponent[text].enabled = true;
			m_animComponent[text].weight = 1f;
		}
		m_animComponent.Sample();
	}

	public void UpdateRun(float speed)
	{
		if (!m_animComponent.isPlaying)
		{
			RestartRun();
		}
		SetRunSpeed(m_jogAnimName, m_jogAnimSpeed, speed);
		SetRunSpeed(m_runAnimName, m_runAnimSpeed, speed);
		AnimationState animationState = m_animComponent[m_inRollName];
		float speed2 = m_animComponent[m_jogAnimName].speed;
		m_animComponent[m_outRollName].speed = speed2;
		animationState.speed = speed2;
		float num = Utils.MapValue(speed, m_runCrossoverMinSpeed, m_runCrossoverMaxSpeed, 1f, 0f);
		m_animComponent[m_jogAnimName].weight = num;
		float num2 = 1f - num;
		m_animComponent[m_runAnimName].weight = num2;
		if (CharacterManager.Singleton.GetCurrentCharacter() == Characters.Type.Shadow && !IsBallShown)
		{
			if (m_animComponent[m_runAnimName].enabled && !m_animComponent[SpringPrepareToLand].enabled && !m_animComponent[SpringGlide].enabled && num2 > 0.5f)
			{
				EnableFeetParticles();
			}
			else
			{
				DisableFeetParticles();
			}
		}
	}

	private void EnableFeetParticles()
	{
		if ((bool)m_runParticlesLeft && (bool)m_runParticlesRight && m_runParticlesLeft.gameObject.activeInHierarchy && !m_runParticlesLeft.isPlaying)
		{
			m_runParticlesLeft.Play();
			m_runParticlesRight.Play();
		}
	}

	private void DisableFeetParticles()
	{
		if ((bool)m_runParticlesLeft && (bool)m_runParticlesRight && m_runParticlesLeft.gameObject.activeInHierarchy && m_runParticlesLeft.isPlaying)
		{
			m_runParticlesLeft.Stop();
			m_runParticlesRight.Stop();
		}
	}

	private void SetRunSpeed(string runAnimName, float animSpeed, float currentSpeed)
	{
		float speed = Utils.MapValue_NoClamp(currentSpeed, 0f, animSpeed, 0f, 1f);
		m_animComponent[runAnimName].speed = speed;
	}

	private void OneShotParticleFX(ParticleSystem pfx, bool attachToWorld, Vector3 localOffset)
	{
		if (!(pfx == null))
		{
			if (attachToWorld)
			{
				WorldCollector.MarkAsMovable(pfx.gameObject);
			}
			ParticlePlayer.Play(pfx);
			pfx.transform.position = base.transform.position + localOffset;
			pfx.transform.rotation = base.transform.rotation;
		}
	}

	private void ClearAndPlayParticleFX(ParticleSystem pfx)
	{
		if (!(pfx == null))
		{
			ParticlePlayer.Play(pfx);
		}
	}

	private void Event_OnSpringEnd()
	{
		m_isBanking = false;
		m_isBossSuccess = false;
		m_isBossFailure = false;
	}

	private void Event_OnSpringDescent(float springJumpTimeRemaining)
	{
		OnBossSpringDescent();
		if (m_isBossFailure)
		{
			float endTime = GameState.TimeInGame + springJumpTimeRemaining;
			StartCoroutine(SpringLand(endTime));
			m_animComponent.Stop();
		}
		else
		{
			StartCoroutine(SpringGlideToLand(springJumpTimeRemaining));
		}
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		StopAllCoroutines();
		IsBallShown = false;
		m_isBanking = springType == SpringTV.Type.Bank;
		m_isBossSuccess = springType == SpringTV.Type.Boss;
		m_isBossFailure = false;
		m_animComponent.Stop();
		m_animComponent[SpringGlide].wrapMode = WrapMode.Loop;
		PlayAnimation(SpringLaunch);
	}

	private void Event_OnBossAttackGestureStart()
	{
		StopAllCoroutines();
		IsBallShown = false;
		StartCoroutine(BossAttackGestureStart());
	}

	private void Event_OnBossAttackGestureEndSuccess()
	{
		StopAllCoroutines();
		IsBallShown = false;
		StartCoroutine(BossAttackGestureEndSuccess());
	}

	private IEnumerator BossAttackGestureEndSuccess()
	{
		m_animComponent.Stop();
		yield return StartCoroutine(PlayAnimClipOnce(m_springBossAttack.RecoilToSuccess, clampForever: true));
	}

	private void Event_OnBossAttackGestureEndFailure(float springJumpTimeRemaining)
	{
		StopAllCoroutines();
		IsBallShown = false;
		m_isBossSuccess = false;
		m_isBossFailure = true;
		StartCoroutine(BossAttackGestureEndFailure());
	}

	private IEnumerator BossAttackGestureEndFailure()
	{
		yield return StartCoroutine(PlayAnimClipOnce(SpringLaunchToGlide, clampForever: false));
		string clipName = SpringGlide;
		m_animComponent[clipName].wrapMode = WrapMode.Loop;
		PlayAnimation(clipName);
		while (m_animComponent[clipName].enabled)
		{
			yield return null;
		}
		PlayAnimation(clipName);
	}

	private IEnumerator BossAttackGestureStart()
	{
		yield return StartCoroutine(PlayAnimClipOnce(m_springBossAttack.Ready, clampForever: false));
		m_animComponent[m_springBossAttack.Idle].wrapMode = WrapMode.Loop;
		PlayAnimation(m_springBossAttack.Idle);
	}

	private void OnBossSpringDescent()
	{
		if (m_animComponent[m_springBossAttack.RecoilToSuccess].enabled)
		{
			m_animComponent.Stop();
		}
	}

	private IEnumerator SpringGlideToLand(float springJumpTimeRemaining)
	{
		float endTime = GameState.TimeInGame + springJumpTimeRemaining;
		if (m_animComponent[SpringLaunch].wrapMode != WrapMode.Loop)
		{
			while (m_animComponent[SpringLaunch].normalizedTime < 1f)
			{
				yield return null;
			}
			m_animComponent[SpringLaunch].enabled = false;
		}
		m_animComponent[SpringLaunchToGlide].wrapMode = WrapMode.Once;
		PlayAnimation(SpringLaunchToGlide);
		while (m_animComponent[SpringLaunchToGlide].enabled)
		{
			yield return null;
		}
		PlayAnimation(SpringGlide);
		yield return StartCoroutine(SpringLand(endTime));
	}

	private IEnumerator SpringLand(float endTime)
	{
		float prepareToLandLength = m_animComponent[SpringPrepareToLand].length;
		float prepareToLandTime = endTime - prepareToLandLength;
		while (GameState.TimeInGame < prepareToLandTime)
		{
			yield return null;
		}
		PlayAnimation(SpringPrepareToLand);
		while (m_animComponent.IsPlaying(SpringPrepareToLand))
		{
			yield return null;
		}
		PlayAnimation(m_springLandIntoRun);
		m_groundSlamParticles.Play();
		while (m_animComponent.IsPlaying(m_springLandIntoRun))
		{
			yield return null;
		}
	}

	private IList<string>[] FindSpringDanceAnims()
	{
		IList<string>[] array = new IList<string>[Utils.GetEnumCount<CartesianDir>()];
		foreach (AnimationState item in m_animComponent)
		{
			if (item.name.Contains(m_springDanceKeyword))
			{
				CartesianDir cartesianDir = (item.name.EndsWith("Left") ? CartesianDir.Left : (item.name.EndsWith("Right") ? CartesianDir.Right : ((!item.name.EndsWith("Up")) ? CartesianDir.Down : CartesianDir.Up)));
				int num = (int)cartesianDir;
				if (array[num] == null)
				{
					array[num] = new List<string>();
				}
				array[num].Add(item.name);
				item.layer = 2;
			}
		}
		return array;
	}

	private void Event_OnSingleSpringGestureSuccess(CartesianDir gestureDir)
	{
		string text = FindSuitableSpringGesture(gestureDir);
		m_animComponent.CrossFade(text, 0.2f);
	}

	private string FindSuitableSpringGesture(CartesianDir gestureDir)
	{
		int count = m_danceAnims[(int)gestureDir].Count;
		return m_danceAnims[(int)gestureDir][m_rng.Next(count)];
	}

	private void PlayAnimation(string clipName)
	{
		m_animComponent.Play(clipName);
	}

	private IEnumerator PlayAnimClipOnce(string clipName, bool clampForever)
	{
		m_animComponent.Stop();
		m_animComponent[clipName].wrapMode = ((!clampForever) ? WrapMode.Once : WrapMode.ClampForever);
		PlayAnimation(clipName);
		while (m_animComponent[clipName].enabled)
		{
			yield return null;
		}
	}

	private string GetAnimName(BankVariableAnim animVariants)
	{
		if (m_isBanking)
		{
			return animVariants.BankVariant;
		}
		if (m_isBossSuccess)
		{
			return animVariants.BossSuccessVariant;
		}
		if (m_isBossFailure)
		{
			return animVariants.BossFailureVariant;
		}
		return animVariants.Regular;
	}
}
