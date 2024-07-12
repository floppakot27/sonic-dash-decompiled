using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(TrackDatabase))]
[AddComponentMenu("Dash/Track/Track Generator")]
[RequireComponent(typeof(TrackGenerationParameters))]
public class TrackGenerator : Track
{
	public struct MidGameNewTrackRequest
	{
		public static MidGameNewTrackRequest Invalid = default(MidGameNewTrackRequest);

		public float EmptyAirPrefix;

		public SpringTV.Type SpringType;

		public SpringTV.Destination Destination;

		public SpringTV.CreateFlags CreateFlags;
	}

	[Flags]
	private enum TrackCreationFlags
	{
		None = 0,
		IsSpringLanding = 1,
		IsBossBattle = 2
	}

	[Flags]
	private enum CreateFlags
	{
		None = 0,
		ForSpringLanding = 1,
		SetPieces = 2,
		BossBattle = 4
	}

	private enum SegmentCollision
	{
		Yes,
		No
	}

	private bool m_trackIsGenerated;

	private int m_seed;

	private System.Random m_rng;

	private int m_pieceSuffix;

	private TrackDatabase m_database;

	private TrackGenerationParameters m_generationParams;

	private Transform m_piecesParent;

	private TrackSegment m_lastCreatedPiece;

	private bool m_previousRampUp;

	private Queue<TrackSegment> m_pieces;

	private Queue<TrackSegment> m_straightStartPieces;

	private bool m_isStraightOpen;

	private float m_generatedLength;

	private float m_totalTrackLength;

	private float m_trackStartDifficultyRelevantLength;

	private float m_difficultyRelevantTrackLength;

	private int m_trackCount;

	private int m_gameplayStraightTemplatesGenerated;

	private TrackPieceSequence m_trackSequence;

	private SpawnPoolDynamic[] m_scenicSheetSpawnPools;

	private SpawnPoolDynamic m_currentScenicSheetSpawnPool;

	private GameObject m_currentTrackSegmentObject;

	private GameplayTemplateGenerator m_gameplayTemplateGenerator;

	private Queue<TrackSegment> m_decorationQueue;

	private uint m_excludedEntities;

	private uint m_excludedTemplates;

	private bool m_useTrackCollision = true;

	private int m_numBossSpawns = -1;

	private int m_numTracksSinceBossBattle = -1;

	private int m_numBossNotEncounteredRuns = -1;

	private bool m_isTrackStraightLoddingEnabled;

	private SegmentEnabler m_segmentEnabler;

	public int Seed
	{
		get
		{
			return m_seed;
		}
		set
		{
			m_seed = value;
		}
	}

	public bool CanHaveBossBattle { get; set; }

	private bool IsSeedFixed => false;

	private int FixedSeed => -1;

	public override bool IsAvailable => m_pieces != null;

	public override Spline StartSpline => m_pieces.Peek().MiddleSpline;

	public TrackDatabase Database => m_database;

	public bool GameplayMorePiecesRequired => m_generationParams == null || m_generatedLength < m_generationParams.GameplayMinTrackDistanceToGenerate;

	public bool IsTrackGenerated => m_trackIsGenerated;

	public float DifficultyRelevantLengthAtTrackStart => m_trackStartDifficultyRelevantLength;

	public TrackGenerationParameters GenerationParams => m_generationParams;

	public float BossBattleRand { get; private set; }

	public float BossBattleChance { get; private set; }

	private void ResetBossBattle()
	{
		if (m_numBossSpawns > 0)
		{
			m_numBossNotEncounteredRuns = 0;
		}
		else
		{
			m_numBossNotEncounteredRuns++;
		}
		m_numBossSpawns = 0;
		m_numTracksSinceBossBattle = 0;
	}

	private void NotifyForNextBossBattle()
	{
		CanHaveBossBattle = GameState.GetLostWorldEventActive() && !TutorialSystem.instance().isTrackTutorialEnabled();
		if (!CanHaveBossBattle)
		{
			return;
		}
		if (m_numBossSpawns == 0)
		{
			BossBattleChance = m_generationParams.GetBossBattleSpringFirstChance(m_numTracksSinceBossBattle);
			if (m_numBossNotEncounteredRuns > 0)
			{
				BossBattleChance += m_generationParams.GetBossBattleNotEncounteredRunsModifier(m_numBossNotEncounteredRuns - 1);
			}
			if (PlayerStats.GetCurrentStats().m_trackedStats[91] == 0)
			{
				BossBattleChance += m_generationParams.BossBattleNeverSeenChance;
			}
			BossBattleRand = (float)m_rng.NextDouble();
			CanHaveBossBattle = BossBattleRand < BossBattleChance;
		}
		else if (m_numBossSpawns == 1)
		{
			BossBattleChance = m_generationParams.GetBossBattleSpringSecondChance(m_numTracksSinceBossBattle);
			BossBattleRand = (float)m_rng.NextDouble();
			CanHaveBossBattle = BossBattleRand < BossBattleChance;
		}
		else
		{
			CanHaveBossBattle = false;
			BossBattleRand = 0f;
			BossBattleChance = 0f;
		}
	}

	public void ConsumeBossBattle(bool isSpawn)
	{
		if (!TutorialSystem.instance().isTrackTutorialEnabled())
		{
			if (isSpawn)
			{
				m_numTracksSinceBossBattle = 0;
				m_numBossSpawns++;
			}
			else
			{
				m_numTracksSinceBossBattle++;
			}
		}
	}

	private void ResetFixedSeed()
	{
	}

	public new void Awake()
	{
		base.Awake();
		ResetFixedSeed();
		m_straightStartPieces = new Queue<TrackSegment>();
		m_segmentEnabler = new SegmentEnabler();
		m_database = GetComponent<TrackDatabase>();
		m_currentScenicSheetSpawnPool = null;
		m_scenicSheetSpawnPools = GetComponents<SpawnPoolDynamic>();
	}

	private void OnDestroy()
	{
		if (m_segmentEnabler != null)
		{
			m_segmentEnabler.Shutdown();
		}
	}

	private void Event_RequestNewTrackMidGame(MidGameNewTrackRequest request)
	{
		TutorialSystem.instance().notifySectionFinish();
		TutorialSystem.instance().Reset(fullReset: false);
		int num = ConvertDestinationToSubzoneIndex(request.Destination);
		if (TutorialSystem.instance().isTrackTutorialEnabled())
		{
			num = ConvertDestinationToSubzoneIndex(SpringTV.Destination.Grass);
		}
		StatsTrackingChangeZone(num, request.Destination);
		TrackCreationFlags trackCreationFlags = TrackCreationFlags.IsSpringLanding;
		bool flag = (request.CreateFlags & SpringTV.CreateFlags.BossBattle) > SpringTV.CreateFlags.None;
		if (flag)
		{
			trackCreationFlags |= TrackCreationFlags.IsBossBattle;
		}
		ConsumeBossBattle(flag);
		NotifyForNextBossBattle();
		LaunchTrackCreationKickoff(trackCreationFlags, request.SpringType, num, request.EmptyAirPrefix);
	}

	private void Event_ResetTrackState()
	{
		m_useTrackCollision = FeatureSupport.IsSupported("Physics Based Collision");
		m_isTrackStraightLoddingEnabled = FeatureSupport.IsSupported("TrackStraightLodding");
		InitialiseGeneration();
		NotifyForNextBossBattle();
		LaunchTrackCreationKickoff(TrackCreationFlags.None, SpringTV.Type.ChangeZone, -1, 0f);
		SonicSplineTracker.AllowRunning = false;
	}

	public int ConvertDestinationToSubzoneIndex(SpringTV.Destination destination)
	{
		string text = destination.ToString().ToLower();
		List<TrackDatabase.SubzoneInfo> subzones = m_database.Zones.First().Subzones;
		for (int i = 0; i < subzones.Count(); i++)
		{
			if (subzones[i].Name == text)
			{
				return i;
			}
		}
		return -1;
	}

	public int CalculateCurrentSubzoneIndex()
	{
		int num = -1;
		foreach (TrackSegment piece in m_pieces)
		{
			int subzoneIndex = piece.Template.SubzoneIndex;
			if (subzoneIndex >= 0)
			{
				num = subzoneIndex;
				break;
			}
		}
		return (num >= 0) ? num : m_generationParams.FirstSubzoneIndex;
	}

	private void StatsTrackingChangeZone(int subzone, SpringTV.Destination newTrackType)
	{
		if (subzone != CalculateCurrentSubzoneIndex())
		{
			switch (newTrackType)
			{
			case SpringTV.Destination.Grass:
				PlayerStats.IncreaseStat(PlayerStats.StatNames.GrassVisits_Total, 1);
				break;
			case SpringTV.Destination.Temple:
				PlayerStats.IncreaseStat(PlayerStats.StatNames.TempleVisits_Total, 1);
				break;
			case SpringTV.Destination.Beach:
				PlayerStats.IncreaseStat(PlayerStats.StatNames.BeachVisits_Total, 1);
				break;
			}
		}
	}

	private void InitialiseGeneration()
	{
		m_trackIsGenerated = false;
		CreateRandomNumberGenerator();
		m_pieceSuffix = 0;
		m_totalTrackLength = 0f;
		m_difficultyRelevantTrackLength = 0f;
		m_trackCount = 0;
		m_gameplayTemplateGenerator = UnityEngine.Object.FindObjectOfType(typeof(GameplayTemplateGenerator)) as GameplayTemplateGenerator;
		m_generationParams = GetComponent<TrackGenerationParameters>();
		if (FeatureSupport.IsSupported("FavourMemoryOverTrackGenSpeed"))
		{
			SaveMemoryOnSubzoneTransition();
		}
		m_gameplayStraightTemplatesGenerated = 0;
		ResetBossBattle();
		TutorialSystem.instance().Reset(fullReset: true);
	}

	private void LaunchTrackCreationKickoff(TrackCreationFlags flags, SpringTV.Type springType, int forcedSubzone, float emptyAirPrefix)
	{
		StopAllCoroutines();
		EnsureTrackInfoAvailable();
		m_currentTrackSegmentObject = null;
		RSRGenerator.RSRSpawned = false;
		m_lastCreatedPiece = null;
		StartCoroutine(TrackCreationKickoff(flags, springType, forcedSubzone, emptyAirPrefix));
	}

	private IEnumerator TrackCreationKickoff(TrackCreationFlags flags, SpringTV.Type springType, int forcedSubzone, float emptyAirPrefix)
	{
		bool isBossBattle = (flags & TrackCreationFlags.IsBossBattle) > TrackCreationFlags.None;
		bool trackStraightLodding = m_isTrackStraightLoddingEnabled;
		if (isBossBattle)
		{
			m_isTrackStraightLoddingEnabled = false;
		}
		while (null == Sonic.Tracker)
		{
			yield return null;
		}
		m_segmentEnabler.RestartEnablingWith(this);
		if (m_pieces != null)
		{
			foreach (TrackSegment piece in m_pieces)
			{
				piece.SetGameplayEnabled(isEnabled: true);
			}
		}
		if (m_trackSequence != null)
		{
			m_trackSequence = null;
		}
		while (m_segmentEnabler.IsProcessingPending)
		{
			yield return null;
		}
		m_database.KillAllWorldPieces();
		bool isSpringLandingRequested = (flags & TrackCreationFlags.IsSpringLanding) > TrackCreationFlags.None;
		if (isSpringLandingRequested && forcedSubzone != CalculateCurrentSubzoneIndex() && FeatureSupport.IsSupported("FavourMemoryOverTrackGenSpeed"))
		{
			SaveMemoryOnSubzoneTransition();
		}
		bool isChangingZone = m_currentScenicSheetSpawnPool == null || forcedSubzone != CalculateCurrentSubzoneIndex();
		if (isChangingZone)
		{
			if (m_currentScenicSheetSpawnPool != null)
			{
				yield return StartCoroutine(m_currentScenicSheetSpawnPool.Unload());
			}
			m_currentScenicSheetSpawnPool = m_scenicSheetSpawnPools[(forcedSubzone >= 0) ? forcedSubzone : 0];
			yield return StartCoroutine(m_currentScenicSheetSpawnPool.Load());
		}
		if (isBossBattle)
		{
			while (BossLoader.Instance() == null)
			{
				yield return null;
			}
			yield return StartCoroutine(BossLoader.Instance().LoadBoss());
		}
		m_decorationQueue = new Queue<TrackSegment>();
		m_pieces = new Queue<TrackSegment>();
		m_lastCreatedPiece = null;
		m_generatedLength = 0f;
		m_piecesParent = CreatePiecesParent();
		m_straightStartPieces.Clear();
		int subzoneCount = m_database.Zones.First().Subzones.Count();
		int newSubzoneIndex = ((!isSpringLandingRequested) ? m_generationParams.FirstSubzoneIndex : ((forcedSubzone < 0) ? m_rng.Next(subzoneCount) : forcedSubzone));
		yield return StartCoroutine(m_database.LoadPrefabs(newSubzoneIndex));
		if (isChangingZone)
		{
			yield return Resources.UnloadUnusedAssets();
			EventDispatch.GenerateEvent("OnSubzoneAssetsLoaded");
		}
		if ((flags & TrackCreationFlags.IsBossBattle) > TrackCreationFlags.None)
		{
			m_excludedTemplates = 2u;
		}
		m_trackSequence = new TrackPieceSequence(m_rng, m_database, m_generationParams, m_excludedTemplates);
		yield return StartCoroutine(m_gameplayTemplateGenerator.OnStartGeneration(m_rng, !isSpringLandingRequested, Database.SubzoneIndex));
		m_gameplayTemplateGenerator.Track = this;
		CreateFlags creationCoroutineFlags = (((flags & TrackCreationFlags.IsSpringLanding) > TrackCreationFlags.None) ? CreateFlags.ForSpringLanding : CreateFlags.None);
		if ((flags & TrackCreationFlags.IsBossBattle) > TrackCreationFlags.None)
		{
			creationCoroutineFlags |= CreateFlags.BossBattle;
		}
		creationCoroutineFlags |= ((springType == SpringTV.Type.SetPiece) ? CreateFlags.SetPieces : CreateFlags.None);
		StartCoroutine(ProcessDecorationQueue());
		yield return StartCoroutine(TrackCreator(creationCoroutineFlags, emptyAirPrefix));
		if (isChangingZone)
		{
			yield return StartCoroutine(HDTextureLoader.ReplaceHDVariants("HD Textures - Track", loadAsFastAsPossible: false, unregisterBundle: false));
		}
		m_isTrackStraightLoddingEnabled = trackStraightLodding;
	}

	private void SaveMemoryOnSubzoneTransition()
	{
		if (m_database != null)
		{
			m_database.ClearPooledTrackPieces();
		}
	}

	public override SplineUtils.SplineParameters GetSplineToSideOf(Spline fromSpline, LightweightTransform fromTransform, SideDirection toDirection)
	{
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(fromSpline);
		Lane laneOfSpline = segmentOfSpline.GetLaneOfSpline(fromSpline);
		if ((laneOfSpline == Lane.Left && toDirection == SideDirection.Left) || (laneOfSpline == Lane.Right && toDirection == SideDirection.Right))
		{
			return default(SplineUtils.SplineParameters);
		}
		Lane lane = ((toDirection == SideDirection.Left) ? ((laneOfSpline != Lane.Middle) ? Lane.Middle : Lane.Left) : ((laneOfSpline != Lane.Middle) ? Lane.Middle : Lane.Right));
		Spline spline = segmentOfSpline.GetSpline((int)lane);
		Utils.ClosestPoint closestPoint = spline.EstimateDistanceAlongSpline(fromTransform.Location);
		SplineUtils.SplineParameters result = default(SplineUtils.SplineParameters);
		result.Target = spline;
		result.StartPosition = closestPoint.LineDistance;
		result.TravelDirection = Direction_1D.Forwards;
		return result;
	}

	private IEnumerator TrackDestroyer(bool destroyTrack)
	{
		int segmentDeletionBuffer = 2;
		while (true)
		{
			if (IsSonicOnNewSegment())
			{
				TrackSegment currentSegment = m_currentTrackSegmentObject.GetComponent<TrackSegment>();
				if (m_straightStartPieces.Count > 0 && currentSegment.NextSegment == m_straightStartPieces.Peek())
				{
					EnableNextGameplayStraight();
				}
				if (destroyTrack)
				{
					if (segmentDeletionBuffer == 0)
					{
						DestroyOldestTrackSegment();
					}
					else
					{
						segmentDeletionBuffer--;
					}
				}
				EnableCollisionOnSegmentSequence(currentSegment);
			}
			yield return null;
		}
	}

	private IEnumerator TrackCreator(CreateFlags flags, float emptyAirPrefix)
	{
		m_isStraightOpen = false;
		m_straightStartPieces.Clear();
		m_trackStartDifficultyRelevantLength = m_difficultyRelevantTrackLength;
		if (emptyAirPrefix > 0f)
		{
			yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.EmptyAir, emptyAirPrefix, endSection: false));
		}
		TrackDatabase.TrackPiecePrefab firstSegmentPrefab = null;
		firstSegmentPrefab = (((flags & CreateFlags.ForSpringLanding) <= CreateFlags.None) ? m_trackSequence.GetNext(TrackPieceSequence.GenericPieceType.GameStart, TrackPieceSequence.Flags.None) : m_trackSequence.GetNext(TrackPieceSequence.GenericPieceType.TrackCap, TrackPieceSequence.Flags.None));
		TrackSegment firstSegment = CreateNextSegment(firstSegmentPrefab, 1f, endSection: false);
		yield return StartCoroutine(WaitForCleanSegment(firstSegment));
		PostProcessNewTrackPiece(firstSegment);
		m_generatedLength = firstSegment.MiddleSpline.Length;
		float templateFreeDistance = (((flags & CreateFlags.ForSpringLanding) <= CreateFlags.None) ? m_generationParams.InitialTemplateFreeDistance : m_generationParams.SpringLandingTemplateFreeDistance);
		while (m_generatedLength < templateFreeDistance)
		{
			yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.Straight, 1f, endSection: false));
		}
		bool isTutorialEnabled = TutorialSystem.instance().isTrackTutorialEnabled();
		bool isBossBattle = (flags & CreateFlags.BossBattle) > CreateFlags.None;
		m_excludedEntities = 0u;
		m_excludedTemplates = 0u;
		if (isTutorialEnabled)
		{
			int section = TutorialSystem.instance().getCurrentTutorialSection();
			int startIndex = TutorialSystem.instance().getStartTemplateIndexForSection(section);
			int endIndex = startIndex + TutorialSystem.instance().getTemplateCountForSection(section) - 1;
			for (int templateIndex = startIndex; templateIndex <= endIndex; templateIndex++)
			{
				GameplayTemplateGenerator.TemplateInstantiator gameplayTemplate = m_gameplayTemplateGenerator.CreateTutorialTemplate(templateIndex);
				yield return StartCoroutine(AddTemplateToTrack(gameplayTemplate, forceFlat: true));
			}
		}
		else if (isBossBattle)
		{
			m_trackCount++;
			int phaseIndex = 0;
			BossBattleSystem boss = BossBattleSystem.Instance();
			BossBattleSystem.Phase phase = boss.GetPhase(phaseIndex);
			float phaseTrackDistance = 0f;
			float phaseMinTrackDistance = phase.Duration * Sonic.Handling.StartSpeed;
			float straightStartDistance = 0f;
			boss.SetDifficulty();
			while (true)
			{
				phaseTrackDistance += m_generatedLength - straightStartDistance;
				straightStartDistance = m_generatedLength;
				while (phaseTrackDistance >= phaseMinTrackDistance)
				{
					phase.TrackDistance = phaseTrackDistance;
					phaseIndex = boss.GetNextPhaseIndex(phaseIndex);
					phase = boss.GetPhase(phaseIndex);
					if (phase != null && phase.HasTrack())
					{
						if (phase.Enabled)
						{
							phaseTrackDistance = 0f;
							phaseMinTrackDistance = phase.Duration * Sonic.Handling.StartSpeed;
						}
						continue;
					}
					GameplayTemplateGenerator.TemplateInstantiator gameplayTemplate = m_gameplayTemplateGenerator.CreateTemplate(GameplayTemplate.Group.FixedStart, m_gameplayStraightTemplatesGenerated, m_difficultyRelevantTrackLength, m_excludedTemplates);
					yield return StartCoroutine(AddTemplateToTrack(gameplayTemplate, forceFlat: false));
					yield return StartCoroutine(FillRemainingTrackWithTemplate(gameplayTemplate.EndPosition));
					phase = null;
					break;
				}
				if (phase == null)
				{
					break;
				}
				m_excludedEntities = phase.ExcludedEntities;
				m_excludedTemplates = phase.ExcludedTemplates;
				if ((m_excludedTemplates & 2) == 2 && (m_excludedTemplates & 8) == 8 && (m_excludedTemplates & 4) == 4)
				{
					int startIndex = BossBattleSystem.Instance().GetStartTemplateIndexForPhase(phaseIndex);
					int templateCount = BossBattleSystem.Instance().GetTemplateCountForPhase(phaseIndex);
					int templateIndex = startIndex;
					while (phaseTrackDistance < phaseMinTrackDistance)
					{
						GameplayTemplateGenerator.TemplateInstantiator gameplayTemplate = m_gameplayTemplateGenerator.CreateBossBattleTemplate(templateIndex);
						yield return StartCoroutine(AddTemplateToTrack(gameplayTemplate, forceFlat: true));
						templateIndex = startIndex + (templateIndex - startIndex + 1) % templateCount;
						phaseTrackDistance += m_generatedLength - straightStartDistance;
						straightStartDistance = m_generatedLength;
					}
				}
				else
				{
					while (m_generatedLength - straightStartDistance < m_generationParams.MinStraightLength)
					{
						GameplayTemplateGenerator.TemplateInstantiator gameplayTemplate = m_gameplayTemplateGenerator.CreateTemplate(GameplayTemplate.Group.FixedStart, m_gameplayStraightTemplatesGenerated, m_difficultyRelevantTrackLength, m_excludedTemplates);
						m_gameplayStraightTemplatesGenerated++;
						yield return StartCoroutine(AddTemplateToTrack(gameplayTemplate, forceFlat: false));
					}
				}
				if (phaseTrackDistance + m_generatedLength - straightStartDistance < phaseMinTrackDistance && (m_excludedTemplates & 8) == 0)
				{
					foreach (TrackDatabase.TrackPiecePrefab cornerPiecePrefab in m_trackSequence.NextCorner())
					{
						yield return StartCoroutine(AddCornerPiece(cornerPiecePrefab));
					}
				}
				m_isStraightOpen = false;
			}
		}
		else
		{
			m_trackCount++;
			int remainingSetPieces = (((flags & CreateFlags.SetPieces) > CreateFlags.None) ? m_generationParams.SetPiecePerSetPieceTrack : 0);
			int straightsToNextSetPiece = 1;
			GameplayTemplateGenerator.TemplateInstantiator gameplayTemplate;
			while (true)
			{
				straightsToNextSetPiece--;
				float straightStartDistance = m_generatedLength;
				while (m_generatedLength - straightStartDistance < m_generationParams.MinStraightLength)
				{
					if (straightsToNextSetPiece < 0 && remainingSetPieces > 0)
					{
						yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.Straight, 1f, TrackPieceSequence.Flags.ForceToLowElevation, endSection: false));
						GameplayTemplateGenerator.TemplateInstantiator setPieceTemplate = m_gameplayTemplateGenerator.CreateRandomTemplateFromGroup(GameplayTemplate.Group.SetPiece, m_difficultyRelevantTrackLength, m_excludedTemplates);
						yield return StartCoroutine(AddTemplateToSetPiece(setPieceTemplate));
						float afterSetPieceTemplateFreeDistance = m_generationParams.AfterSetPieceTemplateFreeDistance + m_generatedLength;
						while (m_generatedLength < afterSetPieceTemplateFreeDistance)
						{
							yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.Straight, 1f, endSection: false));
						}
						remainingSetPieces--;
						straightsToNextSetPiece = m_rng.Next(2) + 1;
					}
					else
					{
						gameplayTemplate = m_gameplayTemplateGenerator.CreateTemplate(GameplayTemplate.Group.FixedStart, m_gameplayStraightTemplatesGenerated, m_difficultyRelevantTrackLength, m_excludedTemplates);
						m_gameplayStraightTemplatesGenerated++;
						yield return StartCoroutine(AddTemplateToTrack(gameplayTemplate, forceFlat: false));
					}
				}
				if (!GameplayMorePiecesRequired)
				{
					break;
				}
				foreach (TrackDatabase.TrackPiecePrefab cornerPiecePrefab in m_trackSequence.NextCorner())
				{
					yield return StartCoroutine(AddCornerPiece(cornerPiecePrefab));
				}
				m_isStraightOpen = false;
			}
			gameplayTemplate = m_gameplayTemplateGenerator.CreateTemplate(GameplayTemplate.Group.FixedStart, m_gameplayStraightTemplatesGenerated, m_difficultyRelevantTrackLength, m_excludedTemplates);
			yield return StartCoroutine(AddTemplateToTrack(gameplayTemplate, forceFlat: false));
			yield return StartCoroutine(FillRemainingTrackWithTemplate(gameplayTemplate.EndPosition));
		}
		m_gameplayTemplateGenerator.BankSpringsNextTemplate = ((!isTutorialEnabled) ? m_generationParams.GetBankSpringCount(m_trackCount) : 3);
		if (m_generationParams.BankSpringsOmitFrequency > 0 && m_trackCount > 0 && m_trackCount % m_generationParams.BankSpringsOmitFrequency == 0)
		{
			m_gameplayTemplateGenerator.BankSpringsNextTemplate = 0;
		}
		GameplayTemplateGenerator.TemplateInstantiator springTemplate = m_gameplayTemplateGenerator.CreateRandomTemplateFromGroup(GameplayTemplate.Group.EndTrackSpring, m_difficultyRelevantTrackLength, m_excludedTemplates);
		yield return StartCoroutine(AddTemplateToTrack(springTemplate, forceFlat: false));
		yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.TrackCap, 1f, endSection: false));
		yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.EmptyAir, 1f, endSection: true));
		EnableNextGameplayStraight();
		while (m_segmentEnabler.IsProcessingPending)
		{
			yield return null;
		}
		if (TrackMerger.s_instance != null)
		{
			yield return StartCoroutine(TrackMerger.s_instance.performStaticBatching());
		}
		TargetManager.instance().PrepareForGameplay();
		EventDispatch.GenerateEvent("OnTrackGenerationComplete");
		m_trackIsGenerated = true;
		StartCoroutine(TrackDestroyer(!isTutorialEnabled && !isBossBattle));
		if (isBossBattle)
		{
			BossBattleSystem.Instance().Enable();
		}
	}

	private IEnumerator AddCornerPiece(TrackDatabase.TrackPiecePrefab cornerPiecePrefab)
	{
		TrackSegment nextCornerPiece = CreateNextSegment(cornerPiecePrefab, 1f, endSection: false);
		yield return StartCoroutine(WaitForCleanSegment(nextCornerPiece));
		PostProcessNewTrackPiece(nextCornerPiece);
		GameplayTemplateGenerator.TemplateInstantiator cornerTemplate = ((!m_previousRampUp && nextCornerPiece.Template.PieceType.Elevation != TrackDatabase.ElevationType.RampDown) ? m_gameplayTemplateGenerator.CreateRandomTemplateFromGroup(GameplayTemplate.Group.Corner, m_difficultyRelevantTrackLength, m_excludedTemplates, GameplayTemplateGenerator.Failure.CanFail) : m_gameplayTemplateGenerator.CreateRandomTemplateFromGroup(GameplayTemplate.Group.HillyCorner, m_difficultyRelevantTrackLength, m_excludedTemplates, GameplayTemplateGenerator.Failure.CanFail));
		m_previousRampUp = false;
		if (cornerTemplate != null)
		{
			yield return StartCoroutine(cornerTemplate.AddGameplayTo(nextCornerPiece, m_excludedEntities));
		}
		nextCornerPiece.SetGameplayEnabled(!m_isTrackStraightLoddingEnabled);
		if (cornerTemplate != null)
		{
		}
		if (nextCornerPiece.Template.PieceType.Elevation == TrackDatabase.ElevationType.RampUp)
		{
			m_previousRampUp = true;
		}
	}

	private IEnumerator WaitForCleanSegment(TrackSegment segment)
	{
		Spline[] splines = segment.GetComponentsInChildren<Spline>();
		while (splines.Any((Spline s) => s.calculateIsDirty()))
		{
			yield return null;
		}
	}

	private void EnableNextGameplayStraight()
	{
		if (m_isTrackStraightLoddingEnabled)
		{
			TrackSegment trackSegment = m_straightStartPieces.Dequeue();
			TrackSegment trackSegment2 = ((m_straightStartPieces.Count <= 0) ? null : m_straightStartPieces.Peek());
			TrackSegment trackSegment3 = trackSegment;
			while (trackSegment3 != trackSegment2)
			{
				trackSegment3.SetGameplayEnabled(isEnabled: true);
				trackSegment3 = trackSegment3.NextSegment;
			}
		}
	}

	private IEnumerator AddTemplateToSetPiece(GameplayTemplateGenerator.TemplateInstantiator templateMaker)
	{
		TutorialSystem.instance().NotifyTemplate(templateMaker.GameplayTemplate.Name, m_totalTrackLength);
		yield return StartCoroutine(AddPieceToTrack(TrackPieceSequence.GenericPieceType.SetPiece, 1f, endSection: false));
		TrackSegment setPiece = m_lastCreatedPiece;
		yield return StartCoroutine(templateMaker.AddGameplayTo(setPiece, m_excludedEntities));
	}

	private IEnumerator AddTemplateToTrack(GameplayTemplateGenerator.TemplateInstantiator templateMaker, bool forceFlat)
	{
		TutorialSystem.instance().NotifyTemplate(templateMaker.GameplayTemplate.Name, m_totalTrackLength);
		while (templateMaker.InProgress)
		{
			TrackPieceSequence.GenericPieceType nextPieceType = templateMaker.NextTrackType();
			float nextPieceStretch = templateMaker.GetStretchMultiplier(m_difficultyRelevantTrackLength);
			TrackPieceSequence.Flags sequenceFlags = ((templateMaker.IsFlatTrackRequired() || forceFlat) ? TrackPieceSequence.Flags.NoElevationChanges : TrackPieceSequence.Flags.None);
			yield return StartCoroutine(AddPieceToTrack(nextPieceType, nextPieceStretch, sequenceFlags, endSection: false));
			TrackSegment newPiece = m_lastCreatedPiece;
			if (newPiece.Template.PieceType.Elevation == TrackDatabase.ElevationType.RampDown)
			{
				GameplayTemplateGenerator.TemplateInstantiator hillyTemplate = m_gameplayTemplateGenerator.CreateRandomTemplateFromGroup(GameplayTemplate.Group.Hilly, m_difficultyRelevantTrackLength, m_excludedTemplates, GameplayTemplateGenerator.Failure.CanFail);
				if (hillyTemplate != null)
				{
					yield return StartCoroutine(hillyTemplate.AddGameplayTo(newPiece, m_excludedEntities));
				}
			}
			else
			{
				yield return StartCoroutine(templateMaker.AddGameplayTo(newPiece, m_excludedEntities));
			}
			m_previousRampUp = false;
			newPiece.SetGameplayEnabled(!m_isTrackStraightLoddingEnabled);
			if (newPiece.Template.PieceType.Elevation == TrackDatabase.ElevationType.RampUp)
			{
				m_previousRampUp = true;
			}
		}
	}

	private IEnumerator FillRemainingTrackWithTemplate(SplineTracker startMarker)
	{
		if (GameplayTemplateGenerator.IsOrderedGameplayEnabled)
		{
			yield break;
		}
		float remainingSpace = startMarker.Target.Length - startMarker.CurrentDistance;
		if (!(remainingSpace <= 0f))
		{
			GameplayTemplateGenerator.TemplateInstantiator templateMaker = m_gameplayTemplateGenerator.GetTemplateOfMaxLength(GameplayTemplate.Group.Standard, m_difficultyRelevantTrackLength, remainingSpace);
			if (templateMaker != null)
			{
				yield return StartCoroutine(templateMaker.AddGameplayTo(startMarker, m_excludedEntities));
			}
		}
	}

	private IEnumerator AddPieceToTrack(TrackPieceSequence.GenericPieceType type, float lengthMultiplier, bool endSection)
	{
		return AddPieceToTrack(type, lengthMultiplier, TrackPieceSequence.Flags.None, endSection);
	}

	private IEnumerator AddPieceToTrack(TrackPieceSequence.GenericPieceType type, float lengthMultiplier, TrackPieceSequence.Flags sequenceFlags, bool endSection)
	{
		TrackPieceSequence.Flags pieceFlags = sequenceFlags;
		if ((m_excludedTemplates & 4u) != 0)
		{
			pieceFlags |= TrackPieceSequence.Flags.ForceToLowElevation;
		}
		TrackDatabase.TrackPiecePrefab piecePrefab = m_trackSequence.GetNext(type, pieceFlags);
		TrackSegment piece = CreateNextSegment(piecePrefab, lengthMultiplier, endSection);
		yield return StartCoroutine(WaitForCleanSegment(piece));
		PostProcessNewTrackPiece(piece);
	}

	public override Lane GetLaneOfSpline(Spline s)
	{
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(s);
		return segmentOfSpline.GetLaneOfSpline(s);
	}

	protected override Spline GetNextSpline(SplineTracker stoppedTracker)
	{
		if (stoppedTracker.IsReversed)
		{
			return base.GetNextSpline(stoppedTracker);
		}
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(stoppedTracker.Target);
		TrackSegment nextSegment = segmentOfSpline.NextSegment;
		if (nextSegment == null)
		{
			return null;
		}
		Lane laneOfSpline = segmentOfSpline.GetLaneOfSpline(stoppedTracker.Target);
		Spline spline = nextSegment.GetSpline((int)laneOfSpline);
		return (!(spline == null) && !spline.calculateIsDirty()) ? spline : null;
	}

	private void PostProcessNewTrackPiece(TrackSegment newPiece)
	{
		float length = newPiece.MiddleSpline.Length;
		m_generatedLength += length;
		m_totalTrackLength += length;
		if (newPiece.Template.IsDifficultyRelevantPiece() && !TutorialSystem.instance().isTrackTutorialEnabled())
		{
			m_difficultyRelevantTrackLength += length;
		}
		m_decorationQueue.Enqueue(newPiece);
		newPiece.SetGameplayEnabled(!m_isTrackStraightLoddingEnabled);
		if (!m_isStraightOpen)
		{
			m_straightStartPieces.Enqueue(newPiece);
			m_isStraightOpen = true;
		}
	}

	protected override IEnumerable<Spline> GetParallelSplineCandidates(Spline s)
	{
		return s.transform.parent.GetComponentsInChildren<Spline>();
	}

	protected override void RegisterEvents()
	{
		base.RegisterEvents();
		EventDispatch.RegisterInterest("ResetTrackState", this, EventDispatch.Priority.High);
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this, EventDispatch.Priority.High);
	}

	private IEnumerator ProcessDecorationQueue()
	{
		float distanceDecorated = 0f;
		Lane? previousGapLane = null;
		while (true)
		{
			if (!m_decorationQueue.Any())
			{
				yield return null;
				continue;
			}
			TrackSegment segmentToDecorate = m_decorationQueue.Dequeue();
			TrackDatabase.PieceType templateType = segmentToDecorate.Template.PieceType.Type;
			bool isGapPiece = TrackDatabase.IsGapType(templateType);
			Lane? currentGapLane = ((!isGapPiece) ? null : new Lane?(TrackDatabase.GetGapLane(templateType)));
			bool isPreviousGapClosed = previousGapLane.HasValue && (!isGapPiece || currentGapLane.GetValueOrDefault() != previousGapLane.GetValueOrDefault() || (currentGapLane.HasValue ^ previousGapLane.HasValue));
			if (isGapPiece && (!previousGapLane.HasValue || isPreviousGapClosed))
			{
				base.Info.RegisterEntity(new TrackEntity(TrackEntity.Kind.GapStart, currentGapLane.Value), segmentToDecorate.TrackPosition);
			}
			CreateSegmentScenicLayer(segmentToDecorate);
			distanceDecorated += segmentToDecorate.MiddleSpline.Length;
			previousGapLane = currentGapLane;
			if (FrameTimeSentinal.IsFramerateImportant)
			{
				yield return null;
			}
		}
	}

	private bool IsSonicOnNewSegment()
	{
		if (null == Sonic.Tracker || null == Sonic.Tracker.CurrentSpline || !Sonic.Tracker.enabled)
		{
			return false;
		}
		Spline currentSpline = Sonic.Tracker.CurrentSpline;
		GameObject gameObject = ((!(currentSpline != null)) ? null : TrackSegment.GetSegmentOfSpline(currentSpline).gameObject);
		bool result = m_currentTrackSegmentObject != null && gameObject != m_currentTrackSegmentObject;
		m_currentTrackSegmentObject = gameObject;
		return result;
	}

	private TrackSegment CreateNextSegment(TrackDatabase.TrackPiecePrefab pieceType, float stretchMultiplier, bool endSection)
	{
		GameObject gameObject = pieceType.Object as GameObject;
		GameObject gameObject2 = m_database.MakeTrackPiece(gameObject);
		StringBuilder stringBuilder = new StringBuilder(40);
		stringBuilder.Append(m_pieceSuffix);
		stringBuilder.Append(": ");
		stringBuilder.Append(gameObject.name);
		gameObject2.name = stringBuilder.ToString();
		m_pieceSuffix++;
		gameObject2.transform.parent = m_piecesParent;
		TrackSegment component = gameObject2.GetComponent<TrackSegment>();
		component.Template = pieceType;
		component.EndSegment = endSection;
		if (stretchMultiplier != 1f)
		{
			gameObject2.transform.localScale = new Vector3(1f, stretchMultiplier, 1f);
		}
		if (m_lastCreatedPiece != null)
		{
			component.EntrancePoints.First().StitchTo(m_lastCreatedPiece.ExitPoints.First().Transform);
		}
		else
		{
			gameObject2.transform.position = m_piecesParent.position;
			gameObject2.transform.rotation = m_piecesParent.rotation;
		}
		SegmentCollision collisionEnabled = ((m_pieces.Count >= 2) ? SegmentCollision.No : SegmentCollision.Yes);
		SetTrackSegmentCollision(component, collisionEnabled);
		component.TrackPosition = m_totalTrackLength;
		component.DifficultyRelevantTrackPosition = m_difficultyRelevantTrackLength;
		component.GetSpline(0).Finalise();
		component.GetSpline(1).Finalise();
		component.GetSpline(2).Finalise();
		if (m_lastCreatedPiece != null)
		{
			m_lastCreatedPiece.NextSegment = component;
			component.PreviousSegment = m_lastCreatedPiece;
		}
		m_pieces.Enqueue(component);
		m_lastCreatedPiece = component;
		return component;
	}

	private void DestroyOldestTrackSegment()
	{
		TrackSegment trackSegment = m_pieces.Dequeue();
		float num = trackSegment.TrackPosition - trackSegment.MiddleSpline.Length;
		if (trackSegment.PreviousSegment != null)
		{
			num -= trackSegment.PreviousSegment.MiddleSpline.Length;
		}
		base.Info.RemoveAllEntitiesBeforeTrackDistance(num);
		m_generatedLength -= trackSegment.MiddleSpline.Length;
		m_database.KillTrackPiece(trackSegment);
	}

	private Transform CreatePiecesParent()
	{
		return CreateContainerObject("Pieces");
	}

	private void CreateSegmentScenicLayer(TrackSegment newSegment)
	{
		TrackSegmentScenicLayer component = newSegment.GetComponent<TrackSegmentScenicLayer>();
		if (component != null)
		{
			component.InstanceScenicLayer(m_currentScenicSheetSpawnPool, m_rng);
		}
	}

	private Transform CreateContainerObject(string containerName)
	{
		Transform transform = base.transform.Find(containerName);
		if (transform != null)
		{
			transform.parent = null;
			UnityEngine.Object.Destroy(transform.gameObject);
			transform = null;
		}
		GameObject gameObject = new GameObject(containerName);
		transform = gameObject.transform;
		transform.parent = base.transform;
		transform.position = base.transform.position;
		transform.rotation = base.transform.rotation;
		return transform;
	}

	private void SetTrackSegmentCollision(TrackSegment thisSegment, SegmentCollision collisionEnabled)
	{
		if (thisSegment == null || thisSegment.Template.PieceType.Type == TrackDatabase.PieceType.EmptyAir)
		{
			return;
		}
		MeshCollider component = thisSegment.GetComponent<MeshCollider>();
		if (!(component == null))
		{
			if (!m_useTrackCollision)
			{
				collisionEnabled = SegmentCollision.No;
			}
			component.enabled = collisionEnabled == SegmentCollision.Yes;
		}
	}

	private void EnableCollisionOnSegmentSequence(TrackSegment newSegment)
	{
		SetTrackSegmentCollision(newSegment, SegmentCollision.Yes);
		int segmentIndex = GetSegmentIndex(newSegment);
		TrackSegment thisSegment = ((segmentIndex == 0) ? null : m_pieces.ElementAt(segmentIndex - 1));
		TrackSegment thisSegment2 = m_pieces.ElementAt(segmentIndex + 1);
		SetTrackSegmentCollision(thisSegment, SegmentCollision.No);
		SetTrackSegmentCollision(thisSegment2, SegmentCollision.Yes);
	}

	private int GetSegmentIndex(TrackSegment segementToCheck)
	{
		int num = 0;
		foreach (TrackSegment piece in m_pieces)
		{
			if (piece.gameObject == segementToCheck.gameObject)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private void CreateRandomNumberGenerator()
	{
		Seed = ((!IsSeedFixed) ? DateTime.Now.Millisecond : FixedSeed);
		m_rng = ((Seed >= 0) ? new System.Random(Seed) : new System.Random());
	}
}
