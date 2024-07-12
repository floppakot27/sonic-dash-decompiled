using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
	private class TargetDescription
	{
		public Enemy m_target;

		public bool m_active;

		public bool m_removed;

		public float m_timer;

		public Vector2 m_screenPosition;

		public bool m_new;

		public float m_innerScale;

		public float m_outerScale;

		public float m_alpha;
	}

	public bool m_sameLaneMode = true;

	public bool m_greenTargetsOnlyMode = true;

	public bool m_singleTargetMode = true;

	public bool m_ignoreOccluded = true;

	public bool m_floorTargetingEnabled = true;

	public float m_autoAttackHeight = 2f;

	public float m_groundTargetWindowPushbackMultiplier = 1f;

	public bool m_sameLaneThenMultiAttackMode = true;

	public bool m_queueAttacks = true;

	public bool m_timeBasedRange;

	public float m_attackTime = 1.2f;

	private bool m_targetingSlowdownTarget;

	private float m_realtimeDelta;

	private bool m_attackQueued;

	private float m_queuedAttackTapX;

	private float m_queuedAttackTapY;

	private bool m_attacking;

	private int m_activeChoppers;

	private static TargetManager m_globalInstance;

	private List<Enemy> m_targets;

	public Material m_innerTargetMaterial;

	public Material m_outerTargetMaterial;

	public Mesh m_planeMesh;

	private Material m_internalInnerTargetMaterial;

	private Material m_internalOuterTargetMaterial;

	private TargetDescription[] m_activeTargets;

	private IList<TrackEntity> m_trackInfoScratchList = new List<TrackEntity>();

	private bool m_active;

	private bool m_initialised;

	public float m_poweredUpFarRangeBoost = 1.25f;

	public float m_initialTargetScale = 30f;

	public float m_finalTargetScale = 8f;

	public bool m_targetsAppearInstantly = true;

	public float m_targetAppearDuration = 0.6f;

	public float m_outTargetInitialScaling = 2f;

	public float m_initialAlpha;

	public float m_finalAlpha = 1f;

	public Color m_goodTargetColour = new Color(2f / 85f, 0.8784314f, 2f / 85f, 1f);

	public Color m_badTargetColour = new Color(0.8784314f, 2f / 85f, 2f / 85f, 1f);

	public float m_occlusionRadius = 0.8f;

	public float m_movementDisallowDistance = 40f;

	public float m_enemyGroupingThreshold = 30f;

	[SerializeField]
	private AudioClip m_targetAcquiredAudioClip;

	private float m_lastTime;

	private Enemy m_autoAttackTarget;

	public bool m_slowdownEnabled = true;

	public bool m_slowdownInAirOnly = true;

	public float m_timeToSlowdown = 0.3f;

	public float m_timeToSpeedup = 0.3f;

	public float m_slowdownTargetSlowdownFactor = 0.3f;

	private float m_timeScale;

	public float EnemyGroupingThreshold => m_enemyGroupingThreshold;

	public void activate()
	{
		m_active = true;
		m_lastTime = Time.realtimeSinceStartup;
	}

	public void deactivate()
	{
		m_active = false;
	}

	private void Awake()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this);
		m_globalInstance = this;
		if (m_innerTargetMaterial == null || m_outerTargetMaterial == null || m_planeMesh == null)
		{
		}
		m_internalInnerTargetMaterial = new Material(m_innerTargetMaterial);
		m_internalOuterTargetMaterial = new Material(m_outerTargetMaterial);
		m_active = false;
		m_initialised = false;
		m_lastTime = Time.realtimeSinceStartup;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnSonicRespawn", this);
	}

	public void Event_OnSonicDeath()
	{
		m_active = false;
	}

	public void Event_OnSonicResurrection()
	{
		m_active = true;
	}

	public void Event_OnSonicRespawn()
	{
		m_active = true;
	}

	public static TargetManager instance()
	{
		return m_globalInstance;
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		Reset();
	}

	private void Event_OnNewGameStarted()
	{
		Reset();
		m_activeChoppers = 0;
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		Reset();
	}

	private void Reset()
	{
		m_targets = new List<Enemy>(256);
		m_initialised = true;
		m_active = false;
		m_attacking = false;
		m_timeScale = 1f;
		m_targetingSlowdownTarget = false;
		m_realtimeDelta = 0f;
	}

	public void PrepareForGameplay()
	{
		m_active = false;
		int count = m_targets.Count;
		if (count > 0)
		{
			m_activeTargets = new TargetDescription[count];
			for (int i = 0; i < count; i++)
			{
				m_activeTargets[i] = new TargetDescription();
				m_activeTargets[i].m_target = m_targets[i];
				m_activeTargets[i].m_active = false;
				m_activeTargets[i].m_removed = false;
			}
		}
		m_activeChoppers = 0;
	}

	public void addTarget(Enemy target)
	{
		m_targets.Add(target);
	}

	public void removeTarget(Enemy target)
	{
		TargetDescription[] activeTargets = m_activeTargets;
		foreach (TargetDescription targetDescription in activeTargets)
		{
			if (targetDescription != null && target == targetDescription.m_target)
			{
				targetDescription.m_removed = true;
				targetDescription.m_active = false;
				break;
			}
		}
	}

	public void setAutoAttackTarget(Enemy enemy)
	{
		m_autoAttackTarget = enemy;
	}

	public Enemy getAutoAttackTarget()
	{
		return m_autoAttackTarget;
	}

	public float getAutoAttackHeight()
	{
		return m_autoAttackHeight;
	}

	private void Update()
	{
		if (m_initialised && m_active)
		{
			updateTargets();
			drawTargets();
		}
		else
		{
			m_lastTime = Time.realtimeSinceStartup;
		}
		UpdateTimeScale();
	}

	private void updateTargets()
	{
		if ((Sonic.Tracker != null && !Sonic.Tracker.IsTrackerAvailable) || m_activeTargets == null || m_targets == null)
		{
			return;
		}
		Vector3 position = Sonic.Transform.position;
		float trackPosition = Sonic.Tracker.TrackPosition;
		PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.IncreasedAttackRange);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		m_realtimeDelta = realtimeSinceStartup - m_lastTime;
		m_lastTime = realtimeSinceStartup;
		if (m_activeTargets == null)
		{
			return;
		}
		TargetDescription[] activeTargets = m_activeTargets;
		foreach (TargetDescription targetDescription in activeTargets)
		{
			if (targetDescription == null)
			{
				continue;
			}
			Enemy target = targetDescription.m_target;
			Enemy target2 = targetDescription.m_target;
			Vector3 targetPosition = target2.getTargetPosition();
			if (null == target)
			{
				targetDescription.m_active = false;
			}
			if (DashMonitor.instance().isDashing())
			{
				targetDescription.m_active = false;
			}
			else if (null != target && !targetDescription.m_removed)
			{
				bool flag = target.isAttackable();
				if (!target.isAttackableFromAir())
				{
					flag = false;
				}
				if (!m_attacking && target.getLane() != Sonic.Tracker.getLane())
				{
					flag = false;
				}
				if (flag)
				{
					if (!targetDescription.m_active)
					{
						targetDescription.m_active = true;
						targetDescription.m_new = true;
					}
				}
				else
				{
					targetDescription.m_active = false;
				}
			}
			if (targetDescription.m_active)
			{
				bool flag2 = true;
				if (!m_attacking && target2.getLane() != Sonic.Tracker.getLane())
				{
					flag2 = false;
				}
				if (flag2)
				{
					if (target2.TrackDistance > trackPosition)
					{
						float currentSpeed = Sonic.Tracker.getPhysics().CurrentSpeed;
						float num = target2.TrackDistance - trackPosition;
						float num2 = num / currentSpeed;
						if (num2 < m_attackTime)
						{
							bool flag3 = true;
							if (!m_attacking)
							{
								float nearRange = target2.getNearRange();
								if (num < nearRange)
								{
									flag3 = false;
								}
							}
							if (!flag3)
							{
								flag2 = false;
							}
						}
						else
						{
							flag2 = false;
						}
					}
					else
					{
						flag2 = false;
					}
				}
				if (!flag2)
				{
					targetDescription.m_active = false;
				}
			}
			if (!targetDescription.m_active)
			{
				continue;
			}
			int layerMask = 32768;
			bool flag4 = false;
			Vector3 vector = position;
			Vector3 vector2 = targetPosition;
			flag4 = Physics.Linecast(vector, vector2, layerMask);
			if (!flag4)
			{
				Vector3 lhs = vector2 - vector;
				lhs.Normalize();
				Vector3 vector3 = Vector3.Cross(lhs, Vector3.up);
				vector3.Normalize();
				Vector3 start = position - vector3 * m_occlusionRadius;
				Vector3 end = targetPosition - vector3 * m_occlusionRadius;
				flag4 = Physics.Linecast(start, end, layerMask);
				if (!flag4)
				{
					Vector3 start2 = position + vector3 * m_occlusionRadius;
					Vector3 end2 = targetPosition + vector3 * m_occlusionRadius;
					flag4 = Physics.Linecast(start2, end2, layerMask);
				}
			}
			if (flag4)
			{
				targetDescription.m_active = false;
			}
		}
		if (m_attacking)
		{
			float num3 = -1f;
			TargetDescription[] activeTargets2 = m_activeTargets;
			foreach (TargetDescription targetDescription2 in activeTargets2)
			{
				if (targetDescription2 != null && null != targetDescription2.m_target && targetDescription2.m_active && (num3 < 0f || targetDescription2.m_target.TrackDistance < num3))
				{
					num3 = targetDescription2.m_target.TrackDistance;
				}
			}
			if (!(num3 < 0f))
			{
				TargetDescription[] activeTargets3 = m_activeTargets;
				foreach (TargetDescription targetDescription3 in activeTargets3)
				{
					if (targetDescription3.m_target.TrackDistance - num3 > 1f)
					{
						targetDescription3.m_active = false;
					}
				}
			}
		}
		else
		{
			TargetDescription targetDescription4 = null;
			float num4 = -1f;
			TargetDescription[] activeTargets4 = m_activeTargets;
			foreach (TargetDescription targetDescription5 in activeTargets4)
			{
				if (targetDescription5 != null && null != targetDescription5.m_target && targetDescription5.m_active)
				{
					float num5 = targetDescription5.m_target.TrackDistance - trackPosition;
					if (targetDescription4 == null)
					{
						targetDescription4 = targetDescription5;
						num4 = num5;
					}
					else if (num5 < num4)
					{
						targetDescription4 = targetDescription5;
						num4 = num5;
					}
				}
			}
			if (targetDescription4 != null)
			{
				TargetDescription[] activeTargets5 = m_activeTargets;
				foreach (TargetDescription targetDescription6 in activeTargets5)
				{
					if (targetDescription6 != targetDescription4)
					{
						targetDescription6.m_active = false;
					}
				}
			}
		}
		m_targetingSlowdownTarget = false;
		TargetDescription[] activeTargets6 = m_activeTargets;
		foreach (TargetDescription targetDescription7 in activeTargets6)
		{
			if (targetDescription7 != null && null != targetDescription7.m_target && targetDescription7.m_active)
			{
				if (targetDescription7.m_target.isSlowdownTarget())
				{
					m_targetingSlowdownTarget = true;
				}
				if (targetDescription7.m_new)
				{
					EventDispatch.GenerateEvent("OnTarget");
					targetDescription7.m_timer = 0f;
					targetDescription7.m_new = false;
					Audio.PlayClip(m_targetAcquiredAudioClip, loop: false);
				}
				else
				{
					targetDescription7.m_timer += m_realtimeDelta;
				}
				if (m_targetsAppearInstantly || targetDescription7.m_timer > m_targetAppearDuration)
				{
					targetDescription7.m_innerScale = m_finalTargetScale;
					targetDescription7.m_outerScale = m_finalTargetScale;
					targetDescription7.m_alpha = 1f;
				}
				else
				{
					float t = targetDescription7.m_timer / m_targetAppearDuration;
					targetDescription7.m_innerScale = Mathf.Lerp(m_initialTargetScale, m_finalTargetScale, t);
					targetDescription7.m_outerScale = Mathf.Lerp(m_initialTargetScale * m_outTargetInitialScaling, m_finalTargetScale, t);
					targetDescription7.m_alpha = Mathf.Lerp(m_initialAlpha, m_finalAlpha, t);
				}
				targetDescription7.m_screenPosition = Camera.main.WorldToScreenPoint(targetDescription7.m_target.transform.position);
			}
		}
	}

	private void UpdateTimeScale()
	{
		if (m_slowdownEnabled)
		{
			bool flag = (bool)Sonic.Tracker && Sonic.Tracker.isJumping();
			bool flag2 = true;
			if (m_slowdownInAirOnly)
			{
				flag2 = flag;
			}
			if (m_targetingSlowdownTarget && flag2)
			{
				float num = (1f - m_slowdownTargetSlowdownFactor) / m_timeToSlowdown;
				m_timeScale -= num * m_realtimeDelta;
				m_timeScale = Mathf.Clamp(m_timeScale, m_slowdownTargetSlowdownFactor, 1f);
			}
			else
			{
				float num2 = (1f - m_slowdownTargetSlowdownFactor) / m_timeToSpeedup;
				m_timeScale += num2 * m_realtimeDelta;
				m_timeScale = Mathf.Clamp(m_timeScale, m_slowdownTargetSlowdownFactor, 1f);
			}
		}
		TimeScaler.GameplayScale = m_timeScale;
	}

	private void drawTargets()
	{
		if (m_activeTargets == null || !m_active || m_activeTargets == null || m_targets == null)
		{
			return;
		}
		TargetDescription[] activeTargets = m_activeTargets;
		foreach (TargetDescription targetDescription in activeTargets)
		{
			if (targetDescription != null && null != targetDescription.m_target && targetDescription.m_active)
			{
				Enemy target = targetDescription.m_target;
				Color color = new Color(m_goodTargetColour.r, m_goodTargetColour.g, m_goodTargetColour.b, targetDescription.m_alpha);
				Color color2 = new Color(m_badTargetColour.r, m_badTargetColour.g, m_badTargetColour.b, targetDescription.m_alpha);
				if (target.isAttackableFromAir())
				{
					m_internalInnerTargetMaterial.color = color;
					m_internalOuterTargetMaterial.color = color;
				}
				else
				{
					m_internalInnerTargetMaterial.color = color2;
					m_internalOuterTargetMaterial.color = color2;
				}
				Vector3 targetPosition = target.getTargetPosition();
				Vector3 vector = Camera.main.gameObject.transform.position - targetPosition;
				vector.Normalize();
				targetPosition += vector * 0f;
				Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
				Quaternion quaternion2 = Quaternion.AngleAxis(Time.realtimeSinceStartup * 360f, vector);
				Quaternion quaternion3 = Quaternion.AngleAxis((0f - Time.realtimeSinceStartup) * 360f, vector);
				Quaternion q = quaternion2 * quaternion;
				Vector3 s = new Vector3(targetDescription.m_innerScale, targetDescription.m_innerScale, targetDescription.m_innerScale);
				Vector3 s2 = new Vector3(targetDescription.m_outerScale, targetDescription.m_outerScale, targetDescription.m_outerScale);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(targetPosition, q, s);
				m_internalInnerTargetMaterial.renderQueue = 3250;
				Graphics.DrawMesh(m_planeMesh, matrix, m_internalInnerTargetMaterial, 0);
				q = quaternion3 * quaternion;
				matrix.SetTRS(targetPosition, q, s2);
				m_internalOuterTargetMaterial.renderQueue = 3250;
				Graphics.DrawMesh(m_planeMesh, matrix, m_internalOuterTargetMaterial, 0);
			}
		}
	}

	public Enemy getClosestTarget(float x, float y)
	{
		Enemy result = null;
		if (m_activeTargets != null)
		{
			float num = -1f;
			Vector2 vector = new Vector2(x, y);
			TargetDescription[] activeTargets = m_activeTargets;
			foreach (TargetDescription targetDescription in activeTargets)
			{
				if (targetDescription != null && null != targetDescription.m_target && targetDescription.m_active)
				{
					Vector2 vector2 = targetDescription.m_screenPosition - vector;
					if (vector2.sqrMagnitude < num || num < 0f)
					{
						num = vector2.sqrMagnitude;
						result = targetDescription.m_target;
					}
				}
			}
		}
		return result;
	}

	public bool isFloorTargetingEnabled()
	{
		return m_floorTargetingEnabled;
	}

	public void notifyAttack()
	{
		m_attacking = true;
	}

	public void notifyEndOfAttacking()
	{
		m_attacking = false;
	}

	public void queueAttack(float x, float y)
	{
		m_attackQueued = true;
		m_queuedAttackTapX = x;
		m_queuedAttackTapY = y;
	}

	public bool isAttackQueued()
	{
		return m_attackQueued;
	}

	public void consumeAttack()
	{
		m_attackQueued = false;
	}

	public float getQueuedAttackX()
	{
		return m_queuedAttackTapX;
	}

	public float getQueuedAttackY()
	{
		return m_queuedAttackTapY;
	}

	public List<Enemy> GetEnemiesInRange(float near, float far)
	{
		List<Enemy> list = new List<Enemy>();
		Sonic.Tracker.Track.Info.EntitiesInRange(7u, near, far, ref m_trackInfoScratchList);
		for (int i = 0; i < m_trackInfoScratchList.Count; i++)
		{
			TrackEntity trackEntity = m_trackInfoScratchList[i];
			if (trackEntity != null && trackEntity.IsValid && trackEntity is TrackGameObject)
			{
				Enemy component = (trackEntity as TrackGameObject).Object.GetComponent<Enemy>();
				if (component != null)
				{
					list.Add(component);
				}
			}
		}
		return list;
	}

	public void NotifyChopperActive()
	{
		m_activeChoppers++;
	}

	public void NotifyChopperInactive()
	{
		m_activeChoppers--;
	}

	public int GetNumberOfActiveChoppers()
	{
		return m_activeChoppers;
	}
}
