using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
	[Serializable]
	public class HitpointSetting
	{
		[SerializeField]
		public string m_boneName;

		[SerializeField]
		public float m_attackTime;

		[SerializeField]
		public float m_recoilTime;

		[SerializeField]
		public int m_scoreForHit;

		[SerializeField]
		public AnimationCurve m_bulletTimeCurve;

		[NonSerialized]
		public Transform m_transform;
	}

	private BossMovementController m_movementController;

	private BossAnimationController m_animationController;

	private BossVisualController m_visualController;

	private int m_attackIndex;

	private bool m_attackSuccess = true;

	private float m_timeToEndOfGestureWindow;

	private float m_timeToStartGestureWindow;

	private float m_gestureWindowDuration;

	private float m_timeToNextGestureWindow;

	private BossBattleSystem.GestureSettings.Types m_currentWindowGesture;

	private float m_attackTimer;

	private float m_recoilTimer;

	private float m_totalAttackTime;

	private AnimationCurve m_attackBulletTimeCurve;

	private Transform m_moonCenterTransform;

	[SerializeField]
	private Vector3 m_attackStartInSonicLocalSpace = Vector3.zero;

	[SerializeField]
	private Vector3 m_attackEndInSonicLocalSpace = Vector3.zero;

	[SerializeField]
	private AnimationCurve m_attackProgressCurve;

	[SerializeField]
	private float m_startDelay = 2f;

	[SerializeField]
	private float m_timeMultiplier = 1f;

	[SerializeField]
	private float m_surpriseSFXDelay = 1f;

	[SerializeField]
	private bool m_enableBossDrift = true;

	[SerializeField]
	private List<HitpointSetting> m_hitpoints = new List<HitpointSetting>();

	[SerializeField]
	private Color m_targetColour = new Color(2f / 85f, 0.8784314f, 2f / 85f, 1f);

	[SerializeField]
	private Mesh m_planeMesh;

	[SerializeField]
	private Material m_innerTargetMaterial;

	[SerializeField]
	private Material m_outerTargetMaterial;

	[SerializeField]
	private float m_innerScale;

	[SerializeField]
	private float m_outerScale;

	[SerializeField]
	private float m_targetScale;

	[SerializeField]
	private float m_alpha = 1f;

	[SerializeField]
	private Vector3 m_successEndInSonicLocalSpace;

	[SerializeField]
	private AnimationCurve m_successProgressCurve;

	[SerializeField]
	private float m_failureWaitTime;

	[SerializeField]
	private Vector2 m_failureCameraWobbleFrequency = new Vector2(5f, 1f);

	[SerializeField]
	private Vector2 m_failureCameraWobbleAmplitude = new Vector2(0.01f, 0.01f);

	[SerializeField]
	private Vector3 m_failureEndInSonicLocalSpace;

	[SerializeField]
	private AnimationCurve m_failueProgressCurve;

	[SerializeField]
	private Vector3 m_driftOffsets = Vector3.zero;

	[SerializeField]
	private Vector3 m_driftSpeed = Vector3.zero;

	[SerializeField]
	private float m_driftFadeTime = 1f;

	private bool m_useDrift;

	private Vector3 m_driftValues = Vector3.zero;

	private float m_driftStrength;

	private Vector3 m_sonicPosition;

	public bool AttackSuccess => m_attackSuccess;

	private void Awake()
	{
		m_movementController = GetComponent<BossMovementController>();
		m_animationController = GetComponent<BossAnimationController>();
		m_visualController = GetComponent<BossVisualController>();
		int count = m_hitpoints.Count;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = Utils.FindChildByName(base.gameObject, m_hitpoints[i].m_boneName);
			m_hitpoints[i].m_transform = gameObject.transform;
		}
		m_moonCenterTransform = GameObject.Find("moon_bone_body").transform;
	}

	private void OnEnable()
	{
		EventDispatch.RegisterInterest("OnBossBattleOutroStart", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroEnd", this);
		EventDispatch.RegisterInterest("OnGestureInput", this);
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		TimeScaler.BulletTimeScale = 1f;
		EventDispatch.UnregisterInterest("OnBossBattleOutroStart", this);
		EventDispatch.UnregisterInterest("OnBossBattleOutroEnd", this);
		EventDispatch.UnregisterInterest("OnGestureInput", this);
	}

	public void StartBehaviour()
	{
		m_attackIndex = -1;
		StartCoroutine(PerformBehaviour());
	}

	private void Update()
	{
		BossBattleSystem.Phase currentPhase = BossBattleSystem.Instance().CurrentPhase;
		Sonic.ScoreAnchor.transform.position = m_moonCenterTransform.position;
		if (currentPhase.CurrentGesture != null && m_recoilTimer > 0f)
		{
			float bulletTimeScale = m_attackBulletTimeCurve.Evaluate(m_totalAttackTime - m_attackTimer - m_recoilTimer);
			TimeScaler.BulletTimeScale = bulletTimeScale;
		}
		else
		{
			TimeScaler.BulletTimeScale = 1f;
		}
		if (currentPhase.CurrentGesture != null && m_attackTimer == 0f && m_recoilTimer == 0f)
		{
			UpdateSonicDrift();
			return;
		}
		m_driftStrength = 0f;
		m_sonicPosition = Sonic.Transform.position;
	}

	private IEnumerator PerformBehaviour()
	{
		BossBattleSystem boss = BossBattleSystem.Instance();
		m_visualController.Visible = false;
		m_useDrift = true;
		yield return new WaitForEndOfFrame();
		while (boss == null && Sonic.Transform == null)
		{
			yield return null;
		}
		float timeToStart = m_startDelay;
		while (timeToStart > 0f)
		{
			timeToStart -= Time.deltaTime;
			yield return null;
		}
		m_visualController.Visible = true;
		EventDispatch.GenerateEvent("OnBossAttackGestureStart");
		yield return StartCoroutine(WaitForBossArrival());
		yield return StartCoroutine(PerformBossAttacks());
		m_useDrift = false;
		if (m_attackSuccess)
		{
			yield return StartCoroutine(PlayOutcomeSuccess());
		}
		else
		{
			EventDispatch.GenerateEvent("BossMusicEnd", 2f);
			yield return StartCoroutine(PlayOutcomeFail());
		}
		boss.NextPhase();
	}

	private IEnumerator WaitForBossArrival()
	{
		m_animationController.PlayAnimation(BossAnim.AttackStart);
		m_movementController.SetOrientation(m_attackStartInSonicLocalSpace - m_attackEndInSonicLocalSpace);
		BossMovementController.MovementParameters moveParams = new BossMovementController.MovementParameters
		{
			m_destination = Sonic.Transform.position + Sonic.Transform.rotation * m_attackStartInSonicLocalSpace,
			m_orientation = Quaternion.AngleAxis(180f, new Vector3(0f, 1f, 0f)) * Sonic.Transform.rotation,
			m_duration = 0.1f,
			m_moveWithTracker = false,
			m_faceMovementDirection = false,
			m_splineDistanceFromSonic = 0f
		};
		m_movementController.MoveToDestination(moveParams, m_enableBossDrift, snapToDestination: true);
		yield return new WaitForEndOfFrame();
		m_animationController.PlayAnimation(BossAnim.AttackVulnerable);
		float sfxtimer = 0f;
		bool sfxPlayed = false;
		float timer = 0f;
		while (timer <= m_attackProgressCurve.keys[m_attackProgressCurve.length - 1].time)
		{
			Vector3 position = Vector3.Lerp(t: m_attackProgressCurve.Evaluate(timer), from: m_attackStartInSonicLocalSpace, to: m_attackEndInSonicLocalSpace);
			position = Sonic.Transform.rotation * position;
			position += Sonic.Transform.position;
			m_movementController.MoveToDestination(position, m_enableBossDrift, snapToDestination: true);
			timer += Time.deltaTime;
			sfxtimer += Time.deltaTime;
			if (!sfxPlayed && sfxtimer > m_surpriseSFXDelay)
			{
				sfxPlayed = true;
				Boss.GetInstance().AudioController.PlayQTEStartSFX();
			}
			yield return null;
		}
		EventDispatch.GenerateEvent("OnAttackTheBossHintPrompt");
		while (m_animationController.IsCharacterAnimPlaying(BossAnim.AttackVulnerable))
		{
			sfxtimer += Time.deltaTime;
			if (!sfxPlayed && sfxtimer > m_surpriseSFXDelay)
			{
				sfxPlayed = true;
				Boss.GetInstance().AudioController.PlayQTEStartSFX();
			}
			yield return null;
		}
	}

	private IEnumerator PerformBossAttacks()
	{
		BossBattleSystem boss = BossBattleSystem.Instance();
		BossBattleSystem.Phase phase = boss.CurrentPhase;
		m_sonicPosition = Sonic.Transform.position;
		phase.StartGestures();
		StartGestureWindow();
		m_attackSuccess = true;
		while (m_attackSuccess)
		{
			if (m_timeToStartGestureWindow > 0f)
			{
				m_timeToStartGestureWindow -= Time.deltaTime * m_timeMultiplier;
				if (m_timeToStartGestureWindow < 0f)
				{
					m_timeToStartGestureWindow = 0f;
				}
			}
			else if (m_timeToEndOfGestureWindow > 0f)
			{
				m_timeToEndOfGestureWindow -= Time.deltaTime * m_timeMultiplier;
				if (!(m_timeToEndOfGestureWindow > 0f))
				{
					m_timeToEndOfGestureWindow = 0f;
					m_attackSuccess = false;
					EventDispatch.GenerateEvent("OnGestureWindowTimeOut");
				}
			}
			else if (m_attackTimer > 0f)
			{
				m_attackTimer -= Time.deltaTime * m_timeMultiplier;
				if (m_attackTimer <= 0f)
				{
					m_attackTimer = 0f;
					EndBossAttackGesture();
				}
			}
			else if (m_recoilTimer > 0f)
			{
				m_recoilTimer -= Time.deltaTime * m_timeMultiplier;
				if (m_recoilTimer <= 0f)
				{
					m_recoilTimer = 0f;
				}
			}
			else if (m_timeToNextGestureWindow >= 0f)
			{
				m_timeToNextGestureWindow -= Time.deltaTime * m_timeMultiplier;
				if (m_timeToNextGestureWindow <= 0f)
				{
					m_timeToNextGestureWindow = 0f;
					if (boss.CurrentPhase.NextGesture() <= 0)
					{
						break;
					}
					StartGestureWindow();
				}
			}
			yield return null;
		}
		Sonic.Transform.position = m_sonicPosition;
	}

	private void Event_OnGestureInput(BossBattleSystem.GestureSettings.Types gesture)
	{
		if (IsInGestureWindow())
		{
			m_timeToEndOfGestureWindow = 0f;
			if (m_currentWindowGesture == gesture)
			{
				StartBossAttackGesture();
			}
			else
			{
				m_attackSuccess = false;
			}
		}
	}

	private IEnumerator PlayOutcomeSuccess()
	{
		StartCoroutine(PlayCharacterAnimToEnd(BossAnim.AttackHit3));
		while (m_animationController.IsAttackCameraAnimPlaying(BossAnim.AttackHit3))
		{
			yield return null;
		}
		EventDispatch.GenerateEvent("BossSpringGestureSuccess");
	}

	private IEnumerator PlayOutcomeFail()
	{
		EventDispatch.GenerateEvent("OnAttackTheBossFailPrompt");
		CameraWobble.WobbleCamera(10f, m_failureCameraWobbleFrequency, m_failureCameraWobbleAmplitude);
		Boss.GetInstance().AudioController.PlayQTEGestureFailSFX();
		float failTimer = 0f;
		while (failTimer < m_failureWaitTime)
		{
			failTimer += Time.deltaTime;
			yield return null;
		}
		BossAnim failAnim = GetFailAnim();
		m_animationController.PlayAnimation(failAnim);
		StartCoroutine(PlayCharacterAnimToEnd(failAnim));
		while (m_animationController.IsAttackCameraAnimPlaying(failAnim))
		{
			yield return null;
		}
	}

	private BossAnim GetFailAnim()
	{
		return m_attackIndex switch
		{
			0 => BossAnim.AttackFailure1, 
			1 => BossAnim.AttackFailure2, 
			2 => BossAnim.AttackFailure3, 
			_ => BossAnim.Idle, 
		};
	}

	private void Event_OnBossBattleOutroStart()
	{
		if (m_attackSuccess)
		{
			Boss.GetInstance().AudioController.PlayQTEDefeatedSFX();
		}
	}

	private void Event_OnBossBattleOutroEnd()
	{
		if (!m_attackSuccess)
		{
			Boss.GetInstance().AudioController.PlayQTEEscapeSFX();
			EventDispatch.GenerateEvent("BossSpringGestureFailure");
		}
	}

	private IEnumerator PlayCharacterAnimToEnd(BossAnim anim)
	{
		while (m_animationController.IsCharacterAnimPlaying(anim))
		{
			yield return null;
		}
		m_visualController.Visible = false;
	}

	private void DrawTarget()
	{
		Color color = new Color(m_targetColour.r, m_targetColour.g, m_targetColour.b, m_alpha);
		m_innerTargetMaterial.color = color;
		m_outerTargetMaterial.color = color;
		Vector3 position = m_hitpoints[m_attackIndex].m_transform.position;
		Vector3 vector = Camera.main.gameObject.transform.position - position;
		vector.Normalize();
		Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(Time.realtimeSinceStartup * 360f, vector);
		Quaternion quaternion3 = Quaternion.AngleAxis((0f - Time.realtimeSinceStartup) * 360f, vector);
		Quaternion q = quaternion2 * quaternion;
		float num = m_timeToEndOfGestureWindow / m_gestureWindowDuration;
		float num2 = m_targetScale + (m_innerScale - m_targetScale) * num;
		Vector3 s = new Vector3(num2, num2, num2);
		float num3 = m_targetScale + (m_outerScale - m_targetScale) * num;
		Vector3 s2 = new Vector3(num3, num3, num3);
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(position, q, s);
		m_innerTargetMaterial.renderQueue = 3250;
		Graphics.DrawMesh(m_planeMesh, matrix, m_innerTargetMaterial, 0);
		q = quaternion3 * quaternion;
		matrix.SetTRS(position, q, s2);
		m_outerTargetMaterial.renderQueue = 3250;
		Graphics.DrawMesh(m_planeMesh, matrix, m_outerTargetMaterial, 0);
	}

	private void StartGestureWindow()
	{
		BossBattleSystem.GestureSettings currentGesture = BossBattleSystem.Instance().CurrentPhase.CurrentGesture;
		m_currentWindowGesture = currentGesture.Type;
		m_timeToStartGestureWindow = currentGesture.Delay;
		m_gestureWindowDuration = currentGesture.Duration;
		m_timeToNextGestureWindow = currentGesture.Wait;
		m_attackIndex++;
		m_timeToEndOfGestureWindow = m_gestureWindowDuration;
		ShowGesture();
	}

	private void ShowGesture()
	{
		EventDispatch.GenerateEvent("OnBossReticuleShow", m_hitpoints[m_attackIndex]);
	}

	private bool IsInGestureWindow()
	{
		return m_timeToStartGestureWindow == 0f && m_timeToEndOfGestureWindow > 0f && BossBattleSystem.Instance().CurrentPhase.CurrentGesture != null;
	}

	private void StartBossAttackGesture()
	{
		HitpointSetting hitpointSetting = m_hitpoints[m_attackIndex];
		m_attackTimer = hitpointSetting.m_attackTime;
		m_recoilTimer = hitpointSetting.m_recoilTime;
		m_totalAttackTime = m_attackTimer + m_recoilTimer;
		m_attackBulletTimeCurve = hitpointSetting.m_bulletTimeCurve;
		Sonic.Tracker.gameObject.SendMessage("StartBossAttackAnim", null, SendMessageOptions.DontRequireReceiver);
		Boss.GetInstance().AudioController.PlayQTEAttackSFX();
		switch (m_attackIndex)
		{
		case 0:
			m_animationController.PlayAnimation(BossAnim.AttackHit1, m_timeMultiplier);
			break;
		case 1:
			m_animationController.PlayAnimation(BossAnim.AttackHit2, m_timeMultiplier);
			break;
		case 2:
			EventDispatch.GenerateEvent("BossMusicEnd", 0.5f);
			m_animationController.PlayAnimation(BossAnim.AttackHit3, m_timeMultiplier);
			break;
		}
		Trail.m_instance.activate();
	}

	private void EndBossAttackGesture()
	{
		Trail.m_instance.deactivate();
		switch (m_attackIndex)
		{
		case 0:
			Boss.GetInstance().AudioController.PlayQTEHit1SFX();
			Sonic.ScoreAnchor.SetActive(value: true);
			EventDispatch.GenerateEvent("OnBossHit", m_hitpoints[0].m_scoreForHit);
			m_visualController.Flash(BossVisualController.Part.Vehicle);
			break;
		case 1:
			Boss.GetInstance().AudioController.PlayQTEHit2SFX();
			Sonic.ScoreAnchor.SetActive(value: true);
			EventDispatch.GenerateEvent("OnBossHit", m_hitpoints[1].m_scoreForHit);
			m_visualController.Flash(BossVisualController.Part.Boss);
			break;
		case 2:
			Boss.GetInstance().AudioController.PlayQTEHit3SFX();
			Sonic.ScoreAnchor.SetActive(value: true);
			EventDispatch.GenerateEvent("OnBossHit", m_hitpoints[2].m_scoreForHit);
			m_visualController.Flash(BossVisualController.Part.Boss);
			break;
		}
		m_visualController.PlayAttackImpactEffect(m_attackIndex);
	}

	private void UpdateSonicDrift()
	{
		if (m_useDrift)
		{
			if (m_driftStrength < 1f)
			{
				m_driftStrength += Time.deltaTime * m_driftFadeTime;
			}
		}
		else if (m_driftStrength > 0f)
		{
			m_driftStrength -= Time.deltaTime * m_driftFadeTime;
		}
		m_driftStrength = Mathf.Clamp01(m_driftStrength);
		m_driftValues += m_driftSpeed * Time.deltaTime;
		Vector3 vector = m_driftStrength * m_driftOffsets;
		Vector3 direction = new Vector3(Mathf.Cos(m_driftValues.x) * vector.x, Mathf.Cos(m_driftValues.y) * vector.y, Mathf.Cos(m_driftValues.z) * vector.z);
		direction = Sonic.Transform.TransformDirection(direction);
		Sonic.Transform.position = m_sonicPosition + direction;
	}

	public float CurrentAttackTime()
	{
		return m_hitpoints[m_attackIndex].m_attackTime / m_timeMultiplier;
	}

	public float CurrentRecoilTime()
	{
		return m_hitpoints[m_attackIndex].m_recoilTime / m_timeMultiplier;
	}

	public Transform CurrentTransform()
	{
		return m_hitpoints[m_attackIndex].m_transform;
	}

	public bool IsInAttack()
	{
		if (m_attackSuccess)
		{
			if (m_timeToEndOfGestureWindow > 0f || m_attackTimer > 0f)
			{
				return true;
			}
			if (m_attackIndex < m_hitpoints.Count - 1)
			{
				return m_attackIndex < 0 || IsInPostAttack();
			}
		}
		return false;
	}

	public bool AttackTimerActive()
	{
		return m_attackTimer > 0f;
	}

	public bool IsInPostAttack()
	{
		return m_attackTimer == 0f && m_recoilTimer + m_timeToNextGestureWindow > 0f;
	}

	public float GetGestureProgress()
	{
		if (m_gestureWindowDuration != 0f)
		{
			return m_timeToEndOfGestureWindow / m_gestureWindowDuration;
		}
		return 0f;
	}
}
