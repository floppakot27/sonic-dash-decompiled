using UnityEngine;

public class TutorialSystem : MonoBehaviour
{
	public enum DialogType
	{
		Undefined,
		JumpBegin,
		JumpHint,
		JumpEnd,
		RollBegin,
		RollHint,
		RollEnd,
		StrafeBegin,
		StrafeHint,
		StrafeEnd,
		AttackBegin,
		AttackHint,
		AttackEnd,
		Complete,
		DashRings,
		DashButton,
		DashAutoTrigger,
		DashFinish,
		ChopperJump,
		ChopperTap,
		Bank,
		Chopper,
		Retry
	}

	private struct DialogTriggerDescription
	{
		public DialogType m_type;

		public float m_distance;
	}

	private const string StarRingSaveProperty = "Tutorial Star Awarded";

	private const string ShowTutorialProperty = "ShowTutorial";

	private const string TrackTutorialShownProperty = "TrackTutorialShown";

	private const string AttackTutorialShownProperty = "AttackTutorialShown";

	private const string DashTutorialShownProperty = "DashTutorialShown";

	private static TutorialSystem m_instance;

	private int m_numberOfRegisteredTutorialTriggerPoints;

	public int m_maximumNumberOfTrackTutorialDialogs = 128;

	private int m_dialogToCheck;

	private bool m_trackTutorialActive;

	private DialogTriggerDescription[] m_dialogTriggers;

	private bool m_bankedRingsPopupPermitted;

	public bool m_forceBankedRingsPopupPermitted;

	private TrackSegment m_firstTrackSegment;

	public float m_chopperJumpPushForward = 30f;

	public GameObject m_popupJumpBeginTrigger;

	public GameObject m_popupRollBeginTrigger;

	public GameObject m_popupStrafeBeginTrigger;

	public GameObject m_popupAttackBeginTrigger;

	public GameObject m_popupCompleteTrigger;

	public GameObject m_popupBankTrigger;

	public GameObject m_popupDashRingsTrigger;

	public GameObject m_popupDashButtonTrigger;

	public GameObject m_popupChopperJumpTrigger;

	public GameObject m_popupChopperJumpSwipeTrigger;

	public GameObject m_popupChopperTapTrigger;

	public GameObject m_popupBankedRingsTrigger;

	public GameObject m_popupRetryTrigger;

	private GameObject[] m_popupJumpBeginPeers;

	private GameObject[] m_popupRollBeginPeers;

	private GameObject[] m_popupStrafeBeginPeers;

	private GameObject[] m_popupAttackBeginPeers;

	private GameObject[] m_popupCompletePeers;

	private GameObject[] m_popupBankPeers;

	private GameObject[] m_popupDashRingsPeers;

	private GameObject[] m_popupDashButtonPeers;

	private GameObject[] m_popupChopperJumpPeers;

	private GameObject[] m_popupChopperJumpSwipePeers;

	private GameObject[] m_popupChopperTapPeers;

	private GameObject[] m_popupBankedRingsPeers;

	private GameObject[] m_popupRetryPeers;

	public float m_popupChopperJumpSwipeDelay = 1f;

	private GameObject m_delayTriggerGO;

	private GameObject[] m_delayTriggerPeers;

	private float m_delayTriggerTime;

	private float m_bankedRingsTimer = -1f;

	public float m_bankedRingsDelay;

	private DialogType m_requestType;

	private int m_numberOfTutorialSections;

	private TutorialSection[] m_tutorialSections;

	private int m_currentSection;

	public float m_errorPopupActiveDuration = 0.5f;

	private bool m_errorPopupActive;

	private float m_errorTime;

	public bool m_showTutorial;

	public bool m_trackTutorialShown;

	public bool m_attackTutorialShown;

	public bool m_dashTutorialShown;

	public bool m_completePopupShown;

	private int[] m_analyticsCounts;

	private bool m_regenerationAllowed;

	private int m_homingKills;

	private int m_rollingKills;

	private int m_respawns;

	private bool m_respawnNotified;

	public void PlayOnScreenGesture(CartesianDir dir)
	{
	}

	public void Enable()
	{
		m_showTutorial = true;
		m_trackTutorialShown = false;
		m_attackTutorialShown = false;
		m_dashTutorialShown = false;
		m_completePopupShown = false;
	}

	public void Disable()
	{
		m_showTutorial = false;
		m_trackTutorialShown = true;
		m_attackTutorialShown = true;
		m_dashTutorialShown = true;
		m_completePopupShown = true;
	}

	public void onRewindStart()
	{
		DialogRequest(DialogType.Retry);
	}

	public void onRewindFinished()
	{
		GameAnalytics.s_playerDeath = false;
		m_dialogToCheck--;
	}

	public bool isTrackTutorialEnabled()
	{
		return m_showTutorial && !m_trackTutorialShown;
	}

	private void Awake()
	{
		m_instance = this;
		m_dialogTriggers = new DialogTriggerDescription[m_maximumNumberOfTrackTutorialDialogs];
		m_analyticsCounts = new int[m_maximumNumberOfTrackTutorialDialogs];
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("OnTarget", this);
		EventDispatch.RegisterInterest("OnRingBankRequest", this);
		EventDispatch.RegisterInterest("OnEnemyKilled", this);
		Reset(fullReset: true);
	}

	private void SetPeersActive(GameObject[] peers)
	{
		if (peers != null)
		{
			foreach (GameObject gameObject in peers)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void SetPeersInactive(GameObject[] peers)
	{
		if (peers != null)
		{
			foreach (GameObject gameObject in peers)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private void Event_OnSpringEnd()
	{
		if (m_showTutorial && !m_completePopupShown)
		{
			m_completePopupShown = true;
			ActiveProperties activeProperties = PropertyStore.ActiveProperties();
			if (!activeProperties.GetBool("Tutorial Star Awarded"))
			{
				PropertyStore.Store("Tutorial Star Awarded", property: true);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.StarRingsEarned_Total, 1);
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(1, GameAnalytics.RingsRecievedReason.Tutorial));
			}
			disableAllDrawnObjects();
			if ((bool)m_popupCompleteTrigger)
			{
				SetPeersActive(m_popupCompletePeers);
				m_popupCompleteTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			}
			m_trackTutorialShown = true;
			m_trackTutorialActive = false;
			GameAnalytics.TutorialCompleted(m_analyticsCounts);
		}
	}

	private void Event_OnTarget()
	{
		disableNonChopperJumpDrawnObjects();
		if ((bool)m_popupChopperTapTrigger)
		{
			SetPeersActive(m_popupChopperTapPeers);
			m_popupChopperTapTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void Event_OnRingBankRequest()
	{
		if (m_bankedRingsPopupPermitted || m_forceBankedRingsPopupPermitted)
		{
			m_bankedRingsTimer = m_bankedRingsDelay;
			m_bankedRingsPopupPermitted = false;
		}
	}

	private void Event_OnEnemyKilled(Enemy enemy, Enemy.Kill killType)
	{
		if (isTrackTutorialEnabled())
		{
			switch (killType)
			{
			case Enemy.Kill.Homing:
				m_homingKills++;
				break;
			case Enemy.Kill.Rolling:
				m_rollingKills++;
				break;
			}
		}
	}

	public void notifyRespawn()
	{
		m_respawns++;
		m_respawnNotified = true;
	}

	public void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_showTutorial = true;
		m_trackTutorialShown = false;
		m_attackTutorialShown = false;
		m_dashTutorialShown = false;
		m_completePopupShown = false;
		if (activeProperties.DoesPropertyExist("ShowTutorial"))
		{
			m_showTutorial = activeProperties.GetInt("ShowTutorial") > 0;
		}
		if (activeProperties.DoesPropertyExist("TrackTutorialShown"))
		{
			m_trackTutorialShown = activeProperties.GetInt("TrackTutorialShown") > 0;
		}
		if (activeProperties.DoesPropertyExist("AttackTutorialShown"))
		{
			m_attackTutorialShown = activeProperties.GetInt("AttackTutorialShown") > 0;
		}
		if (activeProperties.DoesPropertyExist("DashTutorialShown"))
		{
			m_dashTutorialShown = activeProperties.GetInt("DashTutorialShown") > 0;
		}
		m_completePopupShown = m_trackTutorialShown;
	}

	public void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("ShowTutorial", m_showTutorial ? 1 : 0);
		PropertyStore.Store("TrackTutorialShown", m_trackTutorialShown ? 1 : 0);
		PropertyStore.Store("AttackTutorialShown", m_attackTutorialShown ? 1 : 0);
		PropertyStore.Store("DashTutorialShown", m_dashTutorialShown ? 1 : 0);
	}

	private void Start()
	{
		ParseTrackDatabase();
		cacheDrawObjects();
		disableAllDrawnObjects();
	}

	public static TutorialSystem instance()
	{
		return m_instance;
	}

	public void attachToSystem()
	{
	}

	public void Reset(bool fullReset)
	{
		m_trackTutorialActive = m_showTutorial && !m_trackTutorialShown;
		m_numberOfRegisteredTutorialTriggerPoints = 0;
		m_dialogToCheck = 0;
		m_errorPopupActive = false;
		m_homingKills = 0;
		m_rollingKills = 0;
		m_respawns = 0;
		m_respawnNotified = false;
		m_firstTrackSegment = null;
		if (fullReset)
		{
			m_currentSection = 0;
			m_bankedRingsPopupPermitted = false;
		}
		if (m_trackTutorialActive)
		{
		}
		m_delayTriggerGO = null;
	}

	public void NotifyTrackSegment(TrackSegment segment)
	{
		if (m_firstTrackSegment == null)
		{
			m_firstTrackSegment = segment;
		}
	}

	public void NotifyTemplate(string name, float distance)
	{
		if (m_numberOfRegisteredTutorialTriggerPoints >= m_maximumNumberOfTrackTutorialDialogs)
		{
			return;
		}
		float num = 0f;
		DialogType dialogType = DialogType.Undefined;
		if (name.ToLower().Contains("tutorialjumpbegin"))
		{
			dialogType = DialogType.JumpBegin;
		}
		else if (name.ToLower().Contains("tutorialjumphint"))
		{
			dialogType = DialogType.JumpHint;
		}
		else if (name.ToLower().Contains("tutorialjumpend"))
		{
			dialogType = DialogType.JumpEnd;
		}
		else if (name.ToLower().Contains("tutorialrollbegin"))
		{
			dialogType = DialogType.RollBegin;
		}
		else if (name.ToLower().Contains("tutorialrollhint"))
		{
			dialogType = DialogType.RollHint;
		}
		else if (name.ToLower().Contains("tutorialrollend"))
		{
			dialogType = DialogType.RollEnd;
		}
		else if (name.ToLower().Contains("tutorialstrafebegin"))
		{
			dialogType = DialogType.StrafeBegin;
		}
		else if (name.ToLower().Contains("tutorialstrafehint"))
		{
			dialogType = DialogType.StrafeHint;
		}
		else if (name.ToLower().Contains("tutorialstrafeend"))
		{
			dialogType = DialogType.StrafeEnd;
		}
		else if (name.ToLower().Contains("tutorialattackbegin"))
		{
			dialogType = DialogType.AttackBegin;
		}
		else if (name.ToLower().Contains("tutorialattackhint"))
		{
			dialogType = DialogType.AttackHint;
		}
		else if (name.ToLower().Contains("tutorialattackend"))
		{
			dialogType = DialogType.AttackEnd;
		}
		else if (name.ToLower().Contains("tutorialbank"))
		{
			dialogType = DialogType.Bank;
		}
		else if (name.ToLower().Contains("tutorialdashgetrings"))
		{
			dialogType = DialogType.DashRings;
		}
		else if (name.ToLower().Contains("tutorialdashpressbutton"))
		{
			dialogType = DialogType.DashButton;
		}
		else if (!name.ToLower().Contains("tutorialdashautotrigger"))
		{
			if (name.ToLower().Contains("tutorialdashfinish"))
			{
				dialogType = DialogType.DashFinish;
			}
			else if (name.ToLower().Contains("chopper"))
			{
				dialogType = DialogType.ChopperJump;
			}
			else if (name.ToLower().Contains("tutorialend"))
			{
				dialogType = DialogType.Complete;
			}
		}
		if (dialogType != 0)
		{
			m_dialogTriggers[m_numberOfRegisteredTutorialTriggerPoints].m_distance = distance - num;
			m_dialogTriggers[m_numberOfRegisteredTutorialTriggerPoints].m_type = dialogType;
			m_numberOfRegisteredTutorialTriggerPoints++;
		}
	}

	public void NotifyChopperGap(float distance)
	{
		if (m_numberOfRegisteredTutorialTriggerPoints > 0)
		{
			int num = m_numberOfRegisteredTutorialTriggerPoints - 1;
			if (m_dialogTriggers[num].m_type == DialogType.ChopperJump)
			{
				m_dialogTriggers[num].m_distance = distance - m_chopperJumpPushForward;
			}
		}
	}

	public float getPreviousDialogStartPosition()
	{
		if (m_dialogToCheck == 0)
		{
			return 0f;
		}
		return m_dialogTriggers[m_dialogToCheck - 1].m_distance - 5f;
	}

	public void UpdateSystem(float distance)
	{
		if (m_dialogToCheck < m_numberOfRegisteredTutorialTriggerPoints && m_dialogTriggers[m_dialogToCheck].m_distance < distance)
		{
			bool flag = true;
			if (m_dialogTriggers[m_dialogToCheck].m_type == DialogType.ChopperJump && (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting()))
			{
				flag = false;
			}
			if (flag)
			{
				DialogRequest(m_dialogTriggers[m_dialogToCheck].m_type);
				m_analyticsCounts[m_dialogToCheck]++;
			}
			m_dialogToCheck++;
			if (m_dialogToCheck < m_numberOfRegisteredTutorialTriggerPoints)
			{
			}
		}
	}

	public void DialogRequest(DialogType type)
	{
		m_requestType = type;
	}

	private void ProcessRequests()
	{
		if (m_respawnNotified)
		{
			m_errorTime = 0f;
			Sonic.Tracker.getPhysics().SlowSonic();
			m_respawnNotified = false;
			m_errorPopupActive = true;
		}
		if (m_requestType == DialogType.Undefined || !m_showTutorial)
		{
			return;
		}
		GameObject gameObject = null;
		switch (m_requestType)
		{
		case DialogType.JumpBegin:
			gameObject = m_popupJumpBeginTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupJumpBeginPeers);
			break;
		case DialogType.RollBegin:
			gameObject = m_popupRollBeginTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupRollBeginPeers);
			break;
		case DialogType.StrafeBegin:
			gameObject = m_popupStrafeBeginTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupStrafeBeginPeers);
			break;
		case DialogType.AttackBegin:
			gameObject = m_popupAttackBeginTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupAttackBeginPeers);
			break;
		case DialogType.Bank:
			gameObject = m_popupBankTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupBankPeers);
			m_bankedRingsPopupPermitted = true;
			break;
		case DialogType.DashRings:
			gameObject = m_popupDashRingsTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupDashRingsPeers);
			EventDispatch.GenerateEvent("OnAutoFillEnabled");
			break;
		case DialogType.DashButton:
			gameObject = m_popupDashButtonTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupDashButtonPeers);
			EventDispatch.GenerateEvent("OnAutoFillEnabled");
			break;
		case DialogType.DashAutoTrigger:
			if (!DashMonitor.instance().isDashing())
			{
				EventDispatch.GenerateEvent("OnDashAutoTrigger");
			}
			break;
		case DialogType.DashFinish:
			EventDispatch.GenerateEvent("OnAutoFillDisabled");
			EventDispatch.GenerateEvent("OnDashStop");
			break;
		case DialogType.Retry:
			gameObject = m_popupRetryTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupRetryPeers);
			break;
		case DialogType.ChopperJump:
			gameObject = m_popupChopperJumpTrigger;
			disableAllDrawnObjects();
			SetPeersActive(m_popupChopperJumpPeers);
			m_delayTriggerGO = m_popupChopperJumpSwipeTrigger;
			m_delayTriggerPeers = m_popupChopperJumpSwipePeers;
			m_delayTriggerTime = m_popupChopperJumpSwipeDelay;
			break;
		}
		if ((bool)gameObject)
		{
			gameObject.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		m_requestType = DialogType.Undefined;
	}

	private void Update()
	{
		if (!m_showTutorial)
		{
			return;
		}
		ProcessRequests();
		if (m_errorPopupActive)
		{
			m_errorTime += Time.deltaTime;
			if (m_errorTime > m_errorPopupActiveDuration)
			{
				m_errorPopupActive = false;
			}
		}
		if ((bool)m_delayTriggerGO)
		{
			m_delayTriggerTime -= Time.deltaTime;
			if (m_delayTriggerTime < 0f)
			{
				if ((bool)m_delayTriggerGO)
				{
					SetPeersActive(m_delayTriggerPeers);
					m_delayTriggerGO.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
				}
				m_delayTriggerGO = null;
			}
		}
		if (m_bankedRingsTimer >= 0f)
		{
			m_bankedRingsTimer -= Time.deltaTime;
			if (m_bankedRingsTimer < 0f && (bool)m_popupBankedRingsTrigger)
			{
				SetPeersActive(m_popupBankedRingsPeers);
				m_popupBankedRingsTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			}
		}
	}

	public void ToggleRegenerationAllowed()
	{
		m_regenerationAllowed = !m_regenerationAllowed;
	}

	public bool IsRegenerationAllowed()
	{
		return m_regenerationAllowed;
	}

	public TrackSegment GetRespawnTrackSegment()
	{
		return m_firstTrackSegment;
	}

	private void ParseTrackDatabase()
	{
		GameObject gameObject = GameObject.Find("GameplayTemplates");
		GameplayTemplateDatabase component = gameObject.GetComponent<GameplayTemplateDatabase>();
		int countOfTemplatesForGroupID = component.GetCountOfTemplatesForGroupID(GameplayTemplate.Group.Tutorial);
		m_numberOfTutorialSections = 1;
		if (m_numberOfTutorialSections > 0)
		{
			m_tutorialSections = new TutorialSection[m_numberOfTutorialSections];
			for (int i = 0; i < m_numberOfTutorialSections; i++)
			{
				m_tutorialSections[i] = new TutorialSection();
			}
		}
		m_tutorialSections[0].m_startIndex = 0;
		m_tutorialSections[0].m_templateCount = countOfTemplatesForGroupID;
		m_tutorialSections[0].m_type = TutorialSection.Type.Survival;
	}

	private int getCurrentSectionSuccessLevel()
	{
		int result = 2;
		switch (m_tutorialSections[m_currentSection].m_type)
		{
		case TutorialSection.Type.Survival:
			if (m_respawns > 1)
			{
				result = 0;
			}
			else if (m_respawns > 0)
			{
				result = 1;
			}
			break;
		case TutorialSection.Type.Kill:
			if (m_rollingKills == 0)
			{
				result = 0;
			}
			else if (m_rollingKills == 1)
			{
				result = 1;
			}
			break;
		}
		return result;
	}

	public void notifySectionFinish()
	{
		if (m_showTutorial && m_trackTutorialActive)
		{
			m_currentSection++;
			if (m_currentSection >= m_numberOfTutorialSections)
			{
				m_trackTutorialShown = true;
				m_trackTutorialActive = false;
				SLAnalytics.LogTrackingEvent("TutorialComplete", "Complete");
			}
		}
	}

	public int getCurrentTutorialSection()
	{
		return m_currentSection;
	}

	public int getStartTemplateIndexForSection(int section)
	{
		if (section < m_numberOfTutorialSections)
		{
			return m_tutorialSections[section].m_startIndex;
		}
		return -1;
	}

	public int getTemplateCountForSection(int section)
	{
		if (section < m_numberOfTutorialSections)
		{
			return m_tutorialSections[section].m_templateCount;
		}
		return 0;
	}

	private GameObject[] findPeers(GameObject gameObject)
	{
		GameObject[] array = null;
		if (null != gameObject)
		{
			Transform transform = gameObject.transform;
			Transform parent = transform.parent;
			if (parent != null)
			{
				int childCount = parent.childCount;
				if (childCount > 1)
				{
					array = new GameObject[childCount - 1];
					int num = 0;
					for (int i = 0; i < childCount; i++)
					{
						Transform child = parent.GetChild(i);
						if (child != transform)
						{
							array[num] = child.gameObject;
							num++;
						}
					}
				}
			}
		}
		return array;
	}

	private void cacheDrawObjects()
	{
		m_popupJumpBeginPeers = findPeers(m_popupJumpBeginTrigger);
		m_popupRollBeginPeers = findPeers(m_popupRollBeginTrigger);
		m_popupStrafeBeginPeers = findPeers(m_popupStrafeBeginTrigger);
		m_popupAttackBeginPeers = findPeers(m_popupAttackBeginTrigger);
		m_popupCompletePeers = findPeers(m_popupCompleteTrigger);
		m_popupBankPeers = findPeers(m_popupBankTrigger);
		m_popupDashRingsPeers = findPeers(m_popupDashRingsTrigger);
		m_popupDashButtonPeers = findPeers(m_popupDashButtonTrigger);
		m_popupChopperJumpPeers = findPeers(m_popupChopperJumpTrigger);
		m_popupChopperJumpSwipePeers = findPeers(m_popupChopperJumpSwipeTrigger);
		m_popupChopperTapPeers = findPeers(m_popupChopperTapTrigger);
		m_popupBankedRingsPeers = findPeers(m_popupBankedRingsTrigger);
		m_popupRetryPeers = findPeers(m_popupRetryTrigger);
	}

	private void disableAllDrawnObjects()
	{
		SetPeersInactive(m_popupJumpBeginPeers);
		SetPeersInactive(m_popupRollBeginPeers);
		SetPeersInactive(m_popupStrafeBeginPeers);
		SetPeersInactive(m_popupAttackBeginPeers);
		SetPeersInactive(m_popupCompletePeers);
		SetPeersInactive(m_popupBankPeers);
		SetPeersInactive(m_popupDashRingsPeers);
		SetPeersInactive(m_popupDashButtonPeers);
		SetPeersInactive(m_popupBankedRingsPeers);
		SetPeersInactive(m_popupChopperJumpPeers);
		SetPeersInactive(m_popupChopperJumpSwipePeers);
		SetPeersInactive(m_popupChopperTapPeers);
		SetPeersInactive(m_popupRetryPeers);
	}

	private void disableNonChopperJumpDrawnObjects()
	{
		SetPeersInactive(m_popupJumpBeginPeers);
		SetPeersInactive(m_popupRollBeginPeers);
		SetPeersInactive(m_popupStrafeBeginPeers);
		SetPeersInactive(m_popupAttackBeginPeers);
		SetPeersInactive(m_popupCompletePeers);
		SetPeersInactive(m_popupBankPeers);
		SetPeersInactive(m_popupDashRingsPeers);
		SetPeersInactive(m_popupDashButtonPeers);
		SetPeersInactive(m_popupBankedRingsPeers);
		SetPeersInactive(m_popupRetryPeers);
	}
}
