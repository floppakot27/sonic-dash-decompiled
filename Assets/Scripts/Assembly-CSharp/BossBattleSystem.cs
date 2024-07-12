using System;
using System.Collections.Generic;
using UnityEngine;

public class BossBattleSystem : MonoBehaviour
{
	public enum DifficultySetting
	{
		Easy,
		Hard
	}

	[Serializable]
	public class AttackSettings
	{
		[NonSerialized]
		private string name = string.Empty;

		public float Delay = 1f;

		public float Distance = -10f;

		public bool LeftLane;

		public bool MiddleLane;

		public bool RightLane;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public int getProjectileCount()
		{
			int num = 0;
			if (LeftLane)
			{
				num++;
			}
			if (MiddleLane)
			{
				num++;
			}
			if (RightLane)
			{
				num++;
			}
			return num;
		}
	}

	[Serializable]
	public class GestureSettings
	{
		public enum Types
		{
			Up,
			Right,
			Down,
			Left,
			TapInZone,
			TapOnScreen
		}

		[NonSerialized]
		private string name = string.Empty;

		public float Delay = 1f;

		public float Duration = 6f;

		public float Wait = 1f;

		public Types Type;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
	}

	[Serializable]
	public class Phase
	{
		public enum Types
		{
			Arrive,
			Intro,
			TransitionToAttack1,
			Attack1Intro,
			Attack1,
			TransitionToAttack2,
			Attack2,
			TransitionToVulnerable,
			Vulnerable,
			Leave,
			Finish
		}

		[NonSerialized]
		private bool m_expandPhases;

		[NonSerialized]
		private int m_startIndex = -1;

		[NonSerialized]
		private int m_templateCount = -1;

		public bool Enabled = true;

		public string Name;

		public float Duration = 10f;

		public Types Type;

		public CameraScheduler.GameCameraType CameraType;

		public int m_excludedEntities = 2067455;

		public int m_excludedTemplates = 14;

		public int m_excludedGestures;

		public DifficultySetting Difficulty;

		public bool StrafeFlipped;

		public List<AttackSettings> m_attackSettings = new List<AttackSettings>();

		public List<GestureSettings> m_gestureSettings = new List<GestureSettings>();

		public Dictionary<object, bool> _editorListItemStates = new Dictionary<object, bool>();

		public bool ShowExcludedEntities { get; set; }

		public bool ShowExcludedTemplates { get; set; }

		public bool ShowExcludedGestures { get; set; }

		public uint ExcludedEntities
		{
			get
			{
				return (uint)m_excludedEntities;
			}
			set
			{
				m_excludedEntities = (int)value;
			}
		}

		public uint ExcludedTemplates
		{
			get
			{
				return (uint)m_excludedTemplates;
			}
			set
			{
				m_excludedTemplates = (int)value;
			}
		}

		public uint ExcludedGestures
		{
			get
			{
				return (uint)m_excludedGestures;
			}
			set
			{
				m_excludedGestures = (int)value;
			}
		}

		public bool ExpandPhases
		{
			get
			{
				return m_expandPhases;
			}
			set
			{
				m_expandPhases = value;
			}
		}

		public int StartIndex
		{
			get
			{
				return m_startIndex;
			}
			set
			{
				m_startIndex = value;
			}
		}

		public int TemplateCount
		{
			get
			{
				return m_templateCount;
			}
			set
			{
				m_templateCount = value;
			}
		}

		public float TrackDistance { get; set; }

		public int GestureIndex { get; private set; }

		public GestureSettings CurrentGesture => Gesture(GestureIndex);

		public bool GesturesCompleted { get; private set; }

		public Phase()
		{
			m_startIndex = -1;
			m_templateCount = -1;
		}

		public void Start()
		{
			GestureIndex = -1;
		}

		public void StartGestures()
		{
			GestureIndex = ((m_gestureSettings.Count <= 0) ? (-1) : 0);
			GesturesCompleted = false;
		}

		public bool IsGestureValid(MotionState.GestureType gestureType)
		{
			return (m_excludedGestures & (1 << (int)gestureType)) == 0;
		}

		public string DebugDesc()
		{
			return "Phase = " + Name + ", Distance = " + TrackDistance + ", Start = " + m_startIndex + ", Count = " + m_templateCount;
		}

		public bool HasTrack()
		{
			return Type < Types.Vulnerable;
		}

		public int NextGesture()
		{
			GestureIndex++;
			if (GestureIndex == m_gestureSettings.Count)
			{
				GestureIndex = -1;
				GesturesCompleted = true;
			}
			return GestureIndex;
		}

		public GestureSettings Gesture(int i)
		{
			return (i < 0 || i >= m_gestureSettings.Count) ? null : m_gestureSettings[i];
		}
	}

	private int m_defaultBossScore = 1000;

	public Dictionary<object, bool> _editorListItemStates = new Dictionary<object, bool>();

	private static BossBattleSystem m_instance;

	[SerializeField]
	private List<Phase> m_phases = new List<Phase>();

	private int m_currentPhase = -1;

	private bool m_canUpdate;

	private DifficultySetting m_difficulty;

	[SerializeField]
	private bool m_showDebug;

	public int DefaultBossScore => m_defaultBossScore;

	public float StartTrackDistance { get; private set; }

	public float PhaseTrackDistance { get; private set; }

	public float TotalTrackDistance { get; private set; }

	public float PhaseTime { get; private set; }

	public float TotalTime { get; private set; }

	public bool ShowDebug
	{
		get
		{
			return m_showDebug;
		}
		set
		{
			m_showDebug = value;
		}
	}

	public Phase CurrentPhase
	{
		get
		{
			if (m_currentPhase >= 0 && m_currentPhase < m_phases.Count)
			{
				return m_phases[m_currentPhase];
			}
			return null;
		}
	}

	public static BossBattleSystem Instance()
	{
		return m_instance;
	}

	public bool IsEnabled()
	{
		return m_currentPhase >= 0;
	}

	public int GetCurrentPhase()
	{
		return m_currentPhase;
	}

	public void SetDifficulty()
	{
		int num = PlayerStats.GetCurrentStats().m_trackedStats[90];
		TrackGenerator trackGenerator = Sonic.Tracker.Track as TrackGenerator;
		if (num < trackGenerator.GenerationParams.BossBattleIncreaseDifficulty)
		{
			m_difficulty = DifficultySetting.Easy;
		}
		else
		{
			m_difficulty = DifficultySetting.Hard;
		}
	}

	public int GetNextPhaseIndex(int phaseIndex)
	{
		if (phaseIndex < m_phases.Count)
		{
			Phase.Types type = m_phases[phaseIndex].Type;
			if (type == Phase.Types.Attack1 || type == Phase.Types.Attack2)
			{
				while (type == GetPhase(phaseIndex + 1).Type)
				{
					phaseIndex++;
				}
			}
			phaseIndex++;
			while (phaseIndex < m_phases.Count && !m_phases[phaseIndex].Enabled)
			{
				phaseIndex++;
			}
			if (phaseIndex < m_phases.Count && (m_phases[phaseIndex].Type == Phase.Types.Attack1 || m_phases[phaseIndex].Type == Phase.Types.Attack2))
			{
				Phase.Types type2 = m_phases[phaseIndex].Type;
				int numPhasesOfType = GetNumPhasesOfType(type2, m_difficulty);
				PlayerStats.StatNames statNames = ((m_difficulty != 0) ? PlayerStats.StatNames.BossBattlesHard_Total : PlayerStats.StatNames.BossBattlesEasy_Total);
				int num = PlayerStats.GetCurrentStats().m_trackedStats[(int)statNames];
				int num2 = num % numPhasesOfType;
				while (num2 > 0)
				{
					phaseIndex++;
					if (IsPhaseValidForType(phaseIndex, type2, m_difficulty))
					{
						num2--;
					}
				}
			}
			return phaseIndex;
		}
		return m_phases.Count;
	}

	public Phase GetPhase(int phaseIndex)
	{
		if (phaseIndex < m_phases.Count)
		{
			return m_phases[phaseIndex];
		}
		return null;
	}

	public int GetStartTemplateIndexForPhase(int phase)
	{
		return m_phases[phase].StartIndex;
	}

	public int GetTemplateCountForPhase(int phase)
	{
		return m_phases[phase].TemplateCount;
	}

	public bool IsGestureValid(MotionState.GestureType gestureType)
	{
		return CurrentPhase.IsGestureValid(gestureType);
	}

	public bool IsGestureStrafeFlipped()
	{
		return CurrentPhase.StrafeFlipped;
	}

	private void Awake()
	{
		m_instance = this;
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
	}

	private void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
		m_instance = null;
		if (PlayerStats.instance() != null)
		{
			PlayerStats.IncreaseStat(PlayerStats.StatNames.BossBattles_Total, 1);
			if (m_difficulty == DifficultySetting.Easy)
			{
				PlayerStats.IncreaseStat(PlayerStats.StatNames.BossBattlesEasy_Total, 1);
			}
			else
			{
				PlayerStats.IncreaseStat(PlayerStats.StatNames.BossBattlesHard_Total, 1);
			}
		}
		EventDispatch.GenerateEvent("BossMusicEnd", 0.5f);
		EventDispatch.GenerateEvent("GameMusicStart", 0.5f);
	}

	private void Start()
	{
		m_currentPhase = 0;
		ParseTrackDatabase();
	}

	public void Enable()
	{
		StartTrackDistance = 0f;
		TotalTrackDistance = 0f;
		PhaseTrackDistance = 0f;
		PhaseTime = 0f;
		TotalTime = 0f;
		m_currentPhase = 0;
		m_canUpdate = false;
		EventDispatch.GenerateEvent("OnBossBattleStart");
		EventDispatch.GenerateEvent("OnBossBattlePhaseStart", m_currentPhase);
	}

	public void Disable()
	{
		m_currentPhase = -1;
	}

	private void Update()
	{
	}

	public void UpdateSystem(float distance)
	{
		if (!m_canUpdate || !IsEnabled())
		{
			return;
		}
		if (StartTrackDistance == 0f)
		{
			StartTrackDistance = distance;
		}
		else if (CurrentPhase.HasTrack())
		{
			PhaseTrackDistance += distance - TotalTrackDistance - StartTrackDistance;
			TotalTrackDistance = distance - StartTrackDistance;
			while (CurrentPhase.HasTrack() && IsEnabled() && PhaseTrackDistance >= CurrentPhase.TrackDistance)
			{
				PhaseTrackDistance -= CurrentPhase.TrackDistance;
				NextPhase();
			}
		}
		else if (CurrentPhase.Type != Phase.Types.Vulnerable)
		{
			PhaseTime += Time.deltaTime;
			TotalTime += Time.deltaTime;
			while (IsEnabled() && PhaseTime >= CurrentPhase.Duration)
			{
				PhaseTime -= CurrentPhase.Duration;
				NextPhase();
			}
		}
	}

	public bool NextPhase()
	{
		EventDispatch.GenerateEvent("OnBossBattlePhaseEnd", m_currentPhase);
		m_currentPhase = GetNextPhaseIndex(m_currentPhase);
		if (m_currentPhase == m_phases.Count)
		{
			EventDispatch.GenerateEvent("OnBossBattleEnd");
			Disable();
			return false;
		}
		CurrentPhase.Start();
		EventDispatch.GenerateEvent("OnBossBattlePhaseStart", m_currentPhase);
		return true;
	}

	private int GetNumPhasesOfType(Phase.Types phaseType, DifficultySetting difficulty)
	{
		int num = 0;
		int count = m_phases.Count;
		for (int i = 0; i < count; i++)
		{
			if (IsPhaseValidForType(i, phaseType, difficulty))
			{
				num++;
			}
		}
		return num;
	}

	private bool IsPhaseValidForType(int phaseIndex, Phase.Types phaseType, DifficultySetting difficulty)
	{
		Phase phase = m_phases[phaseIndex];
		return phase.Type == phaseType && phase.Difficulty == difficulty && phase.Enabled;
	}

	private void Event_OnSpringEnd()
	{
		m_canUpdate = true;
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
	}

	private void Event_OnGameDataSaveRequest()
	{
	}

	private void ParseTrackDatabase()
	{
		GameplayTemplate.Group group = GameplayTemplate.Group.BossBattle;
		GameObject gameObject = GameObject.Find("GameplayTemplates");
		GameplayTemplateDatabase component = gameObject.GetComponent<GameplayTemplateDatabase>();
		int countOfTemplatesForGroupID = component.GetCountOfTemplatesForGroupID(group);
		IEnumerator<GameplayTemplate> enumerator = component.GetEnumerator();
		int num = 0;
		bool flag = true;
		while (flag)
		{
			flag = enumerator.MoveNext();
			GameplayTemplate current = enumerator.Current;
			if (!current.ContainsGroup(group))
			{
				continue;
			}
			if (current.Name.Contains("bossstraight1"))
			{
				for (int i = 0; i < m_phases.Count; i++)
				{
					m_phases[i].StartIndex = num;
					m_phases[i].TemplateCount = 1;
				}
				break;
			}
			num++;
		}
	}
}
