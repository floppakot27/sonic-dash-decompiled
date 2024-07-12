using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[AddComponentMenu("Dash/Gameplay Templates/Template Generator")]
[RequireComponent(typeof(GameplayTemplateDatabase))]
public class GameplayTemplateGenerator : MonoBehaviour
{
	public enum Failure
	{
		CanFail,
		MustNotFail
	}

	public class TemplateInstantiator
	{
		public enum PolymorphicDecision
		{
			ForceToEnemies,
			ForceToObstacles,
			Random
		}

		private int m_lastChopperGapCount;

		private GameplayTemplate m_templateCreating;

		private int m_currentRow;

		private int m_trackSegmentsConsumed;

		private float m_templateSpacing;

		private float m_jumpHeight;

		private TrackEntity.Kind m_enemyChoice;

		private PolymorphicDecision m_polymorphicDecision;

		private GameObject m_multiLaneObstacle;

		private RingStreakConstructor m_ringStreakConstructor;

		private GameplayTemplateGenerator m_templateGenerator;

		private float m_leftoverDeltaFromLastSegment;

		private TrackPieceSequence.GenericPieceType m_currentTrackType;

		private bool m_isFlatTrackRequired;

		public GameplayTemplate GameplayTemplate => m_templateCreating;

		public bool InProgress => m_currentRow < m_templateCreating.RowCount;

		public SplineTracker EndPosition { get; private set; }

		public TemplateInstantiator(GameplayTemplate templateToCreate, float templateSpacing, float jumpHeight, GameplayTemplateGenerator templateGenerator)
		{
			m_templateCreating = templateToCreate;
			m_jumpHeight = jumpHeight;
			m_templateSpacing = templateSpacing;
			m_enemyChoice = GameplayTemplate.PickRandomGroundEnemy(templateGenerator.RNG);
			m_polymorphicDecision = PolymorphicDecision.Random;
			m_templateGenerator = templateGenerator;
			m_currentTrackType = CalculateCurrentTrackType();
			m_isFlatTrackRequired = m_templateCreating.ContainsGroup(GameplayTemplate.Group.Tutorial) || m_templateCreating.ContainsGroup(GameplayTemplate.Group.EndTrackSpring);
		}

		public TrackPieceSequence.GenericPieceType NextTrackType()
		{
			return m_currentTrackType;
		}

		public float GetStretchMultiplier(float difficultyDistance)
		{
			GameplayTemplate.Row row = m_templateCreating.GetRow(m_currentRow);
			if (row.GapRow == GameplayTemplate.Row.GapRowType.Chopper)
			{
				GameplayTemplateParameters parameters = m_templateGenerator.Parameters;
				int gapChopperCountAtDifficulty = parameters.GetGapChopperCountAtDifficulty(Difficulty.GetDifficultyAtDistance(difficultyDistance));
				float chopperSeperationOnGaps = m_templateGenerator.ChopperSeperationOnGaps;
				float num = 1.5f;
				float num2 = 0.95f;
				int num3 = gapChopperCountAtDifficulty - 1;
				float num4 = chopperSeperationOnGaps * num;
				float num5 = chopperSeperationOnGaps;
				float num6 = chopperSeperationOnGaps * num2;
				float result = num4 + num5 * (float)num3 + num6;
				m_lastChopperGapCount = gapChopperCountAtDifficulty;
				return result;
			}
			if (row.GapRow == GameplayTemplate.Row.GapRowType.Jumpable)
			{
				float sonicSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
				float jumpLength = Sonic.Handling.GetJumpLength(sonicSpeed);
				return m_templateGenerator.Parameters.GetJumpGapLength(jumpLength, Difficulty.GetDifficultyAtDistance(difficultyDistance));
			}
			return 1f;
		}

		public bool IsFlatTrackRequired()
		{
			return m_isFlatTrackRequired;
		}

		public IEnumerator AddGameplayTo(SplineTracker segmentTracker, uint excludedEntities)
		{
			TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(segmentTracker.Target);
			return AddGameplayTo(segmentTracker, segmentOfSpline, excludedEntities);
		}

		public IEnumerator AddGameplayTo(TrackSegment segment, uint excludedEntities)
		{
			SplineTracker segmentTracker = new SplineTracker(segment.MiddleSpline, 0f, 0f);
			return AddGameplayTo(segmentTracker, segment, excludedEntities);
		}

		public IEnumerator AddGameplayTo(SplineTracker segmentTracker, TrackSegment segment, uint excludedEntities)
		{
			m_trackSegmentsConsumed++;
			if (m_ringStreakConstructor == null)
			{
				m_ringStreakConstructor = new RingStreakConstructor(m_templateGenerator.Track);
			}
			if (!m_templateCreating.GetRow(m_currentRow).IsEventRow)
			{
				segmentTracker.UpdatePositionByDelta(m_leftoverDeltaFromLastSegment);
				if (!segmentTracker.Tracking)
				{
					m_leftoverDeltaFromLastSegment -= segmentTracker.PreviousDelta;
					yield break;
				}
			}
			else
			{
				m_leftoverDeltaFromLastSegment = 0f;
			}
			TrackSegment currentSegment = TrackSegment.GetSegmentOfSpline(segmentTracker.Target);
			bool isAnyLaneMissing = currentSegment.IsMissingAnyLanes;
			Track.Lane? gapLane = ((!isAnyLaneMissing) ? null : new Track.Lane?(TrackDatabase.GetGapLane(currentSegment.Template.PieceType.Type)));
			StringBuilder templateContainerName = new StringBuilder(50);
			templateContainerName.Append("Template ");
			templateContainerName.Append(m_templateCreating.Name);
			if (m_trackSegmentsConsumed > 1)
			{
				templateContainerName.Append(" (part ");
				templateContainerName.Append(m_trackSegmentsConsumed);
				templateContainerName.Append(")");
			}
			GameObject containerObject = new GameObject(templateContainerName.ToString());
			Transform templateContainer = containerObject.transform;
			if (m_trackSegmentsConsumed == 1)
			{
				currentSegment.OnStartingTemplate(m_templateCreating);
			}
			currentSegment.AddTemplateContainer(templateContainer);
			bool isSinglePieceTemplate = TrackDatabase.IsSetPiece(segment.Template.PieceType.Type) || TrackDatabase.IsTurn(segment.Template.PieceType.Type);
			if (isSinglePieceTemplate)
			{
				segmentTracker.UpdatePositionByDelta(m_templateSpacing * (float)m_templateGenerator.Parameters.EmptyRowsBeforeSetPieceDashPads);
			}
			if (TrackDatabase.IsSetPiece(segment.Template.PieceType.Type))
			{
				AddRowOfDashPads(segmentTracker, segment.TrackPosition);
			}
			if (isSinglePieceTemplate)
			{
				float remainingSplineLength = segmentTracker.Target.Length - segmentTracker.CurrentDistance;
				float templateLength = m_templateSpacing * (float)m_templateCreating.Length;
				float centeringOffset = m_templateGenerator.Parameters.TemplateBiasTowardsEndOfSetPiece * (remainingSplineLength - templateLength);
				segmentTracker.UpdatePositionByDelta(centeringOffset);
			}
			while (true)
			{
				m_ringStreakConstructor.BeginRow(gapLane, segmentTracker);
				GameplayTemplate.Row row = m_templateCreating.GetRow(m_currentRow);
				TrackPieceSequence.GenericPieceType currentRowTrackType = CalculateCurrentTrackType();
				if (currentRowTrackType != m_currentTrackType)
				{
					m_currentTrackType = currentRowTrackType;
					m_leftoverDeltaFromLastSegment = 0f;
					yield break;
				}
				float trackDistance = currentSegment.TrackPosition + segmentTracker.CurrentDistance;
				if (trackDistance >= (float)m_templateGenerator.m_lastDistanceTV)
				{
					if (ShowDistanceTV(segment.Template.PieceType.Type) && !m_templateCreating.Name.StartsWith("tutorial") && (excludedEntities & 0x40000) == 0)
					{
						AddDistanceTV(segmentTracker, currentSegment.TrackPosition, m_templateGenerator.m_lastDistanceTV);
					}
					m_templateGenerator.m_lastDistanceTV += m_templateGenerator.m_distanceBetweenTVs;
				}
				if (!row.IsEventRow)
				{
					m_multiLaneObstacle = null;
					DCs.ChallengeSpringCreated = false;
					GameplayTemplate.Lane[] laneOrder = new GameplayTemplate.Lane[5]
					{
						GameplayTemplate.Lane.LeftFish,
						GameplayTemplate.Lane.Middle,
						GameplayTemplate.Lane.Left,
						GameplayTemplate.Lane.Right,
						GameplayTemplate.Lane.RightFish
					};
					GameplayTemplate.Lane[] array = laneOrder;
					foreach (GameplayTemplate.Lane lane in array)
					{
						if (lane != 0 && lane != GameplayTemplate.Lane.RightFish)
						{
							Track.Lane trackLane = GameplayTemplate.ToTrackLane(lane);
							if (currentSegment.IsMissingLane(trackLane))
							{
								continue;
							}
						}
						GameplayTemplate.Cell cell = row[lane];
						IEnumerator cellInstancer = InstanceCell(cell, lane, trackDistance, templateContainer, segmentTracker, isAnyLaneMissing, excludedEntities);
						while (cellInstancer.MoveNext())
						{
						}
					}
				}
				else if (row.GapRow == GameplayTemplate.Row.GapRowType.Chopper)
				{
					FillTrackWithChoppers(segmentTracker, trackDistance, templateContainer);
				}
				m_currentRow++;
				if (!InProgress)
				{
					m_templateGenerator.m_previousTracker = segmentTracker;
					m_templateGenerator.m_previousSegment = currentSegment;
					yield return m_templateGenerator.StartCoroutine(m_templateGenerator.InstanceRings(m_ringStreakConstructor, m_templateSpacing, m_jumpHeight));
					break;
				}
				bool consumeWholeTrackSegment = row.IsEventRow;
				if (!consumeWholeTrackSegment)
				{
					segmentTracker.UpdatePositionByDelta(m_templateSpacing);
				}
				if (consumeWholeTrackSegment || !segmentTracker.Tracking)
				{
					m_currentTrackType = CalculateCurrentTrackType();
					m_leftoverDeltaFromLastSegment = ((!consumeWholeTrackSegment) ? (m_templateSpacing - segmentTracker.PreviousDelta) : 0f);
					m_templateGenerator.m_previousTracker = segmentTracker;
					m_templateGenerator.m_previousSegment = currentSegment;
					yield break;
				}
				if (FrameTimeSentinal.IsFramerateImportant)
				{
					yield return null;
				}
			}
			EndPosition = segmentTracker;
		}

		private bool ShowDistanceTV(TrackDatabase.PieceType type)
		{
			return !TrackDatabase.IsSetPiece(type);
		}

		private void AddDistanceTV(SplineTracker segmentTracker, float segmentStartTrackDistance, float trackDistance)
		{
			SplineTracker splineTracker;
			if (trackDistance - segmentStartTrackDistance < 0f)
			{
				if (!(m_templateGenerator.m_previousSegment != null) || !(trackDistance - m_templateGenerator.m_previousSegment.TrackPosition < m_templateGenerator.m_previousTracker.Target.Length) || !(trackDistance - m_templateGenerator.m_previousSegment.TrackPosition >= 0f) || !ShowDistanceTV(m_templateGenerator.m_previousSegment.Template.PieceType.Type))
				{
					return;
				}
				splineTracker = new SplineTracker(m_templateGenerator.m_previousTracker.Target, trackDistance - m_templateGenerator.m_previousSegment.TrackPosition, 0f);
			}
			else
			{
				if (!(trackDistance - segmentStartTrackDistance < segmentTracker.Target.Length))
				{
					return;
				}
				splineTracker = new SplineTracker(segmentTracker.Target, trackDistance - segmentStartTrackDistance, 0f);
			}
			WorldTransformLock laneTransform = GetLaneTransform(splineTracker, GameplayTemplate.Lane.Middle);
			GameObject obj = m_templateGenerator.CreateDistanceTV(splineTracker.Target, laneTransform.CurrentTransform, (int)trackDistance);
			Track.Lane lane = GameplayTemplate.ToTrackLane(GameplayTemplate.Lane.Middle);
			m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(TrackEntity.Kind.DistanceTV, lane, obj), trackDistance);
		}

		private void AddRowOfDashPads(SplineTracker segmentTracker, float segmentStartTrackDistance)
		{
			float distance = segmentTracker.CurrentDistance + segmentStartTrackDistance;
			GameplayTemplate.Lane[] trackLanes = GameplayTemplate.TrackLanes;
			foreach (GameplayTemplate.Lane lane in trackLanes)
			{
				WorldTransformLock laneTransform = GetLaneTransform(segmentTracker, lane);
				GameObject obj = m_templateGenerator.CreateDashPad(segmentTracker.Target, laneTransform.CurrentTransform);
				Track.Lane lane2 = GameplayTemplate.ToTrackLane(lane);
				m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(TrackEntity.Kind.DashPad, lane2, obj), distance);
			}
			segmentTracker.UpdatePositionByDelta(m_templateSpacing);
		}

		private void FillTrackWithChoppers(SplineTracker segmentTracker, float segmentTrackPosition, Transform chopperContainer)
		{
			TutorialSystem.instance().NotifyChopperGap(segmentTrackPosition);
			int lastChopperGapCount = m_lastChopperGapCount;
			float chopperSeperationOnGaps = m_templateGenerator.ChopperSeperationOnGaps;
			float num = 1.5f;
			float num2 = 0.95f;
			float delta = chopperSeperationOnGaps * num;
			float delta2 = chopperSeperationOnGaps;
			float delta3 = chopperSeperationOnGaps * num2;
			segmentTracker.UpdatePositionByDelta(delta);
			SideDirection sideDirection = SideDirection.Left;
			int num3 = lastChopperGapCount;
			while (num3 > 0)
			{
				GameplayTemplate.Lane lane = ((sideDirection != 0) ? GameplayTemplate.Lane.RightFish : GameplayTemplate.Lane.LeftFish);
				LightweightTransform currentTransform = GetLaneTransform(segmentTracker, lane).CurrentTransform;
				float num4 = segmentTrackPosition + segmentTracker.CurrentDistance;
				Enemy.Direction direction = ((sideDirection == SideDirection.Left) ? Enemy.Direction.ToPlayersRight : Enemy.Direction.ToPlayersLeft);
				GameObject gameObject = m_templateGenerator.CreateChopper(segmentTracker.Target, currentTransform, direction, num4);
				gameObject.transform.parent = chopperContainer;
				m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(TrackEntity.Kind.Chopper, Track.Lane.Middle, gameObject), num4);
				sideDirection = ((sideDirection == SideDirection.Left) ? SideDirection.Right : SideDirection.Left);
				num3--;
				if (num3 > 0)
				{
					segmentTracker.UpdatePositionByDelta(delta2);
				}
				else
				{
					segmentTracker.UpdatePositionByDelta(delta3);
				}
			}
		}

		private TrackPieceSequence.GenericPieceType CalculateCurrentTrackType()
		{
			if (!m_templateCreating.HasGaps)
			{
				return TrackPieceSequence.GenericPieceType.Straight;
			}
			GameplayTemplate.Row row = m_templateCreating.GetRow(m_currentRow);
			if (row.IsEmptyAir)
			{
				return TrackPieceSequence.GenericPieceType.EmptyAir;
			}
			if (row.IsFullGap)
			{
				return TrackPieceSequence.GenericPieceType.TrackCap;
			}
			if (row.IsGapInLane(GameplayTemplate.Lane.Left))
			{
				return (!row[GameplayTemplate.Lane.Left].IsGapEnd) ? TrackPieceSequence.GenericPieceType.GapLeft : TrackPieceSequence.GenericPieceType.GapLeftEnd;
			}
			if (row.IsGapInLane(GameplayTemplate.Lane.Right))
			{
				return (!row[GameplayTemplate.Lane.Right].IsGapEnd) ? TrackPieceSequence.GenericPieceType.GapRight : TrackPieceSequence.GenericPieceType.GapRightEnd;
			}
			return TrackPieceSequence.GenericPieceType.Straight;
		}

		private IEnumerator InstanceCell(GameplayTemplate.Cell cell, GameplayTemplate.Lane lane, float trackDistance, Transform templateContainer, SplineTracker currentTrack, bool isAnyLaneMissing, uint excludedEntities)
		{
			if (!cell.HasEntities || GameplayTemplate.IsGapCell(cell))
			{
				yield break;
			}
			WorldTransformLock laneTransformLock = GetLaneTransform(currentTrack, lane);
			Spline laneSpline = laneTransformLock.World.GetComponent<Spline>();
			LightweightTransform laneTransform = laneTransformLock.CurrentTransform;
			if (cell.LowEntity != null && cell.LowEntity.Type == 256)
			{
				if ((0x100 & excludedEntities) == 0)
				{
					m_ringStreakConstructor.RegisterLowRing(GameplayTemplate.ToTrackLane(lane));
				}
			}
			else
			{
				GameObject newLowEntity = CreateSuitableEntity(cell.LowEntity, laneSpline, laneTransform, laneTransform, trackDistance, lane, isAnyLaneMissing, excludedEntities);
				if (newLowEntity != null)
				{
					newLowEntity.transform.parent = templateContainer;
					yield return null;
					laneTransform = laneTransformLock.CurrentTransform;
				}
			}
			LightweightTransform highTransform = new LightweightTransform(laneTransform.Location + laneTransform.Up * (m_jumpHeight - 0.35f), laneTransform.Orientation);
			if (cell.HighEntity != null && cell.HighEntity.Type == 256)
			{
				if ((0x100 & excludedEntities) == 0)
				{
					m_ringStreakConstructor.RegisterHighRing(GameplayTemplate.ToTrackLane(lane));
				}
				yield break;
			}
			GameObject newHighEntity = CreateSuitableEntity(cell.HighEntity, laneSpline, highTransform, laneTransform, trackDistance, lane, isAnyLaneMissing, excludedEntities);
			if (newHighEntity != null)
			{
				newHighEntity.transform.parent = templateContainer;
			}
		}

		private WorldTransformLock GetLaneTransform(SplineTracker tracker, GameplayTemplate.Lane lane)
		{
			tracker.ForceUpdateTransform();
			int num;
			switch (lane)
			{
			case GameplayTemplate.Lane.LeftFish:
			case GameplayTemplate.Lane.Middle:
			case GameplayTemplate.Lane.RightFish:
				return new WorldTransformLock(tracker.Target, tracker.CurrentSplineTransform);
			case GameplayTemplate.Lane.Left:
				num = 0;
				break;
			default:
				num = 1;
				break;
			}
			SideDirection toDirection = (SideDirection)num;
			SplineUtils.SplineParameters splineToSideOf = m_templateGenerator.Track.GetSplineToSideOf(tracker.Target, tracker.CurrentSplineTransform, toDirection);
			return new WorldTransformLock(splineToSideOf.Target, splineToSideOf.Target.GetTransform(splineToSideOf.StartPosition));
		}

		private GameObject CreateSuitableEntity(GameplayTemplate.Entity entityToGenerate, Spline spline, LightweightTransform cellTransform, LightweightTransform laneTransform, float trackDistance, GameplayTemplate.Lane lane, bool isAnyLaneMissing, uint excludedEntities)
		{
			TrackEntity.Kind type = (TrackEntity.Kind)entityToGenerate.Type;
			Track.Lane lane2 = lane switch
			{
				GameplayTemplate.Lane.Left => Track.Lane.Left, 
				GameplayTemplate.Lane.Middle => Track.Lane.Middle, 
				_ => Track.Lane.Right, 
			};
			if (((uint)type & excludedEntities) == 0)
			{
				switch (type)
				{
				case TrackEntity.Kind.Invalid:
				case TrackEntity.Kind.Ring:
					break;
				case TrackEntity.Kind.Chopper:
				{
					GameObject gameObject7 = m_templateGenerator.CreateChopper(spline, laneTransform, entityToGenerate.Direction, trackDistance);
					m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(TrackEntity.Kind.Chopper, Track.Lane.Middle, gameObject7), trackDistance);
					return gameObject7;
				}
				case TrackEntity.Kind.Spring:
				{
					GameObject gameObject3 = m_templateGenerator.CreateSpring(spline, laneTransform, lane2);
					m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(TrackEntity.Kind.Spring, lane2, gameObject3), trackDistance);
					return gameObject3;
				}
				case TrackEntity.Kind.ChallengePiece:
				{
					if (!DCs.GetSpawnChallengePiece())
					{
						return null;
					}
					GameObject gameObject6 = m_templateGenerator.CreateChallengePiece(spline, cellTransform);
					DCs.SetSpawnedPiece(gameObject6);
					m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(type, lane2, gameObject6), trackDistance);
					return gameObject6;
				}
				case TrackEntity.Kind.RedStarRing:
				{
					if (!RSRGenerator.CanSpawnRSR())
					{
						return null;
					}
					GameObject gameObject4 = m_templateGenerator.CreateRedStarRing(spline, cellTransform);
					m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(type, lane2, gameObject4), trackDistance);
					RSRGenerator.RSRSpawned = true;
					return gameObject4;
				}
				case TrackEntity.Kind.GCCollectable:
				{
					if (!GCCollectableGenerator.CanSpawnGCCollectable(trackDistance))
					{
						return null;
					}
					GameObject gameObject5 = m_templateGenerator.CreateGCCollectable(spline, cellTransform);
					m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(type, lane2, gameObject5), trackDistance);
					GCCollectableGenerator.GCCollectableSpawned(trackDistance);
					return gameObject5;
				}
				default:
				{
					if ((type & (TrackEntity.Kind)165888) != 0)
					{
						float distance = PlayerStats.GetCurrentStats().m_trackedDistances[7];
						if (m_templateGenerator.m_segmentsWithoutPowerup < m_templateGenerator.Parameters.GetSegmentsWithoutPowerups(distance))
						{
							m_templateGenerator.m_segmentsWithoutPowerup++;
							return null;
						}
						float spawnChance = m_templateGenerator.Parameters.GetSpawnChance(distance);
						if (UnityEngine.Random.value > spawnChance)
						{
							return null;
						}
						m_templateGenerator.m_segmentsWithoutPowerup = 0;
						GameObject gameObject = m_templateGenerator.CreatePowerup(spline, cellTransform, type);
						m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(type, lane2, gameObject), trackDistance);
						return gameObject;
					}
					bool flag = GameplayTemplate.IsSatisfiedByGroundEnemy((uint)type);
					bool flag2 = entityToGenerate.Direction == Enemy.Direction.Stationary || !isAnyLaneMissing;
					flag = flag && flag2;
					bool flag3 = GameplayTemplate.IsSatisfiedByObstacle((uint)type);
					bool flag4 = flag3 && m_polymorphicDecision == PolymorphicDecision.ForceToObstacles;
					if (flag && !flag4 && (!flag3 || m_polymorphicDecision == PolymorphicDecision.ForceToEnemies || (flag && (!flag3 || m_templateGenerator.RNG.NextDouble() < 0.5))))
					{
						TrackEntity.Kind kind = ((!GameplayTemplate.IsSpecificGroundEnemy((uint)type)) ? m_enemyChoice : type);
						GameObject gameObject2 = m_templateGenerator.CreateGroundEnemy(kind, entityToGenerate.Direction, spline, laneTransform, trackDistance);
						m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(kind, lane2, gameObject2), trackDistance);
						return gameObject2;
					}
					if (flag3)
					{
						return GenerateObstacle(entityToGenerate, lane2, trackDistance, laneTransform, spline, isAnyLaneMissing);
					}
					return null;
				}
				}
			}
			return null;
		}

		private GameObject GenerateObstacle(GameplayTemplate.Entity entityToGenerate, Track.Lane lane, float trackDistance, LightweightTransform laneTransform, Spline spline, bool isAnyLaneMissing)
		{
			uint type = entityToGenerate.Type;
			if (entityToGenerate.IsPossiblyMultiLane && lane != Track.Lane.Middle && m_multiLaneObstacle != null)
			{
				TrackEntity.Kind kind = (TrackEntity.Kind)((int)type & 0xF0);
				m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(kind, lane, m_multiLaneObstacle), trackDistance);
				return null;
			}
			TrackEntity.Kind kind2 = GameplayTemplate.PickSuitableObstacle(type, m_templateGenerator.RNG);
			bool flag = entityToGenerate.IsPossiblyMultiLane && !isAnyLaneMissing && lane == Track.Lane.Middle && (kind2 & (TrackEntity.Kind)240) != 0;
			int desiredLaneCount = ((!flag) ? 1 : 3);
			GameObject gameObject = m_templateGenerator.CreateObstacle(kind2, spline, laneTransform, ref desiredLaneCount);
			m_templateGenerator.Track.Info.RegisterEntity(new TrackGameObject(kind2, lane, gameObject), trackDistance);
			if (flag && desiredLaneCount > 1)
			{
				m_multiLaneObstacle = gameObject;
			}
			return gameObject;
		}
	}

	private List<SpringTV.Type> m_choiceTypeOrder = new List<SpringTV.Type>
	{
		SpringTV.Type.ChangeZone,
		SpringTV.Type.ChangeZone,
		SpringTV.Type.SetPiece,
		SpringTV.Type.Random,
		SpringTV.Type.ChangeZone
	};

	private List<SpringTV.Destination> m_choiceDestinationOrder = new List<SpringTV.Destination>
	{
		SpringTV.Destination.Beach,
		SpringTV.Destination.Temple,
		SpringTV.Destination.Grass,
		SpringTV.Destination.Grass,
		SpringTV.Destination.Grass
	};

	private GameplayTemplateDatabase m_database;

	private EnemyGenerator m_enemyGenerator;

	private PowerupGenerator m_powerupGenerator;

	private ObstacleGenerator m_obstacleGenerator;

	private System.Random m_rng;

	private GameplayTemplateParameters m_parameters;

	private GameplayTemplate m_lastTemplate;

	private float m_splineStartTrackDistance;

	private int m_springLane;

	private bool m_springBankIsSpawned;

	private Queue<bool> m_isNextSpringABankSpring;

	private float m_lastSetPieceSpringDistance;

	private bool m_springChangeZoneIsSpawned;

	private int m_segmentsWithoutPowerup;

	protected int m_lastDistanceTV = 500;

	protected int m_distanceBetweenTVs = 500;

	protected SplineTracker m_previousTracker;

	protected TrackSegment m_previousSegment;

	[SerializeField]
	private float m_difficultyScatter = 2f;

	[SerializeField]
	private RingGenerator m_ringGenerator;

	[SerializeField]
	private MiscGenerator m_miscGenerator;

	public bool m_forceTutorial;

	private int m_nextOrderedSetPieceIndex;

	public Track Track { get; set; }

	public System.Random RNG => m_rng;

	public GameplayTemplateParameters Parameters => m_parameters;

	public float ChopperSeperationOnGaps
	{
		get
		{
			float sonicSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
			float jumpLength = Sonic.Handling.GetJumpLength(sonicSpeed);
			return Parameters.GetChopperSeperation(jumpLength);
		}
	}

	public float MaximumJumpableGap
	{
		get
		{
			float sonicSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
			return Sonic.Handling.GetJumpLength(sonicSpeed);
		}
	}

	public int BankSpringsNextTemplate { get; set; }

	public static bool IsOrderedGameplayEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public void Awake()
	{
		m_database = GetComponent<GameplayTemplateDatabase>();
		m_enemyGenerator = UnityEngine.Object.FindObjectOfType(typeof(EnemyGenerator)) as EnemyGenerator;
		m_powerupGenerator = UnityEngine.Object.FindObjectOfType(typeof(PowerupGenerator)) as PowerupGenerator;
		m_obstacleGenerator = UnityEngine.Object.FindObjectOfType(typeof(ObstacleGenerator)) as ObstacleGenerator;
		m_parameters = GetComponentInChildren<GameplayTemplateParameters>();
	}

	public IEnumerator OnStartGeneration(System.Random rng, bool isFirstTrack, int subzoneIndex)
	{
		if (isFirstTrack)
		{
			m_lastDistanceTV = m_distanceBetweenTVs;
		}
		StopAllCoroutines();
		if ((bool)m_enemyGenerator && m_enemyGenerator.enabled)
		{
			m_enemyGenerator.OnStartGeneration();
		}
		if ((bool)m_powerupGenerator && m_powerupGenerator.enabled)
		{
			m_powerupGenerator.OnStartGeneration();
		}
		if ((bool)m_obstacleGenerator && m_obstacleGenerator.enabled)
		{
			yield return StartCoroutine(m_obstacleGenerator.OnStartGeneration(subzoneIndex));
		}
		if ((bool)m_miscGenerator && m_miscGenerator.enabled)
		{
			m_miscGenerator.OnStartGeneration();
		}
		m_rng = rng;
		m_lastTemplate = null;
		BankSpringsNextTemplate = -1;
		m_lastSetPieceSpringDistance = 0f - m_parameters.MinDistanceBetweenSetPieceSprings;
	}

	public TemplateInstantiator GetTemplateOfMaxLength(GameplayTemplate.Group group, float totalTrackLength, float maxTemplateLength)
	{
		Func<GameplayTemplate, bool> customIsSuitable = (GameplayTemplate template) => !template.HasGaps;
		IList<GameplayTemplate> allTemplatesForDistance = GetAllTemplatesForDistance(group, totalTrackLength, customIsSuitable);
		int minTemplateRowCount = m_parameters.MinTemplateRowCount;
		float num = CalculateTemplateSpacing();
		int maxRowCount = Mathf.FloorToInt(maxTemplateLength / num);
		maxRowCount = ((maxRowCount > minTemplateRowCount) ? (Mathf.FloorToInt((float)maxRowCount / (float)minTemplateRowCount) * minTemplateRowCount) : maxRowCount);
		if (maxRowCount <= minTemplateRowCount)
		{
			return null;
		}
		int minRowCount = Mathf.FloorToInt(maxTemplateLength * 0.75f / num);
		minRowCount = ((minRowCount > minTemplateRowCount) ? (Mathf.FloorToInt((float)minRowCount / (float)minTemplateRowCount) * minTemplateRowCount) : minRowCount);
		IEnumerable<GameplayTemplate> enumerable = allTemplatesForDistance.Where((GameplayTemplate template) => template.RowCount <= maxRowCount && template.RowCount >= minRowCount);
		if (!enumerable.Any())
		{
			Debug.LogWarning(string.Concat("no templates from group ", group, " at track distance ", totalTrackLength, " to fill space of size ", maxTemplateLength, "\nneed templates of column size ", minRowCount, "..", maxRowCount));
			return null;
		}
		GameplayTemplate template2 = m_database.PickRandomTemplate(enumerable, m_rng, null);
		return CreateInstantiator(template2);
	}

	public TemplateInstantiator CreateRandomTemplateFromGroup(GameplayTemplate.Group groupID, float currentTrackLength, uint excludedTemplates)
	{
		return CreateRandomTemplateFromGroup(groupID, currentTrackLength, excludedTemplates, Failure.MustNotFail);
	}

	public TemplateInstantiator CreateRandomTemplateFromGroup(GameplayTemplate.Group groupID, float currentTrackLength, uint excludedTemplates, Failure canFail)
	{
		GameplayTemplate gameplayTemplate = PickTemplateForDistance(groupID, currentTrackLength, excludedTemplates, canFail);
		if (canFail == Failure.CanFail && gameplayTemplate == null)
		{
			return null;
		}
		return CreateInstantiator(gameplayTemplate);
	}

	public TemplateInstantiator CreateTemplate(GameplayTemplate.Group groupID, int templateCount, float currentTrackLength, uint excludedTemplates)
	{
		GameplayTemplate gameplayTemplate = ((groupID == GameplayTemplate.Group.Standard) ? null : GetSequentialGroupTemplate(groupID, templateCount));
		GameplayTemplate template = ((gameplayTemplate != null) ? gameplayTemplate : PickTemplateForDistance(GameplayTemplate.Group.Standard, currentTrackLength, excludedTemplates, Failure.MustNotFail));
		return CreateInstantiator(template);
	}

	public TemplateInstantiator CreateTutorialTemplate(int templateIndex)
	{
		GameplayTemplate template = PickTutorialTemplate(templateIndex);
		return CreateInstantiator(template);
	}

	public TemplateInstantiator CreateBossBattleTemplate(int templateIndex)
	{
		GameplayTemplate template = PickBossBattleTemplate(templateIndex);
		return CreateInstantiator(template);
	}

	private IEnumerator InstanceRings(RingStreakConstructor ringsCtor, float spacing, float jumpHeight)
	{
		if (m_ringGenerator == null)
		{
			yield break;
		}
		RingSequence sequence = m_ringGenerator.GetFreeSequence();
		if (sequence == null)
		{
			yield break;
		}
		JumpCurve jumpCurve = Sonic.Handling.CreateJumpFrom(0f);
		float sonicSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame) * Parameters.RingJumpCurveFactor;
		JumpDistanceCurve jumpDistanceCurve = new JumpDistanceCurve(jumpCurve, sonicSpeed);
		foreach (RingStreakConstructor.RingPlacement ringPlacement in ringsCtor.RingPlacements(spacing, jumpDistanceCurve, m_parameters.RingSkipCellCount))
		{
			LightweightTransform ringWorldGroundTransform = ringPlacement.FloorTransform;
			float ringHeight = m_ringGenerator.RingHeightOffset + ringPlacement.AdditionalHeight;
			Vector3 ringWorldPosition = ringWorldGroundTransform.Location + ringWorldGroundTransform.Up * ringHeight;
			RingID ringID = sequence.AddRing(ringWorldPosition, ringWorldGroundTransform.Orientation, ringWorldGroundTransform.Location, ringWorldGroundTransform.Up, ringPlacement.Parent, ringPlacement.Substreaks, ringPlacement.TrackPosition);
			if (ringID.IsValid)
			{
				Track.Info.RegisterEntity(new TrackRing(ringID, TrackEntity.Kind.Ring, ringPlacement.Lane), ringPlacement.TrackPosition);
			}
			if (FrameTimeSentinal.IsFramerateImportant)
			{
				yield return null;
			}
		}
		sequence.OnFinishedAddingRings();
	}

	private GameplayTemplate GetSequentialGroupTemplate(GameplayTemplate.Group groupID, int templateCount)
	{
		int countOfTemplatesForGroupID = m_database.GetCountOfTemplatesForGroupID(groupID);
		if (templateCount < countOfTemplatesForGroupID)
		{
			return m_database.GetIndexedTemplateForGroupID(groupID, templateCount);
		}
		return null;
	}

	private GameplayTemplate PickTutorialTemplate(int templateIndex)
	{
		GameplayTemplate indexedTemplateForGroupID = m_database.GetIndexedTemplateForGroupID(GameplayTemplate.Group.Tutorial, templateIndex);
		if (indexedTemplateForGroupID != null)
		{
			return indexedTemplateForGroupID;
		}
		return null;
	}

	private GameplayTemplate PickBossBattleTemplate(int templateIndex)
	{
		GameplayTemplate indexedTemplateForGroupID = m_database.GetIndexedTemplateForGroupID(GameplayTemplate.Group.BossBattle, templateIndex);
		if (indexedTemplateForGroupID != null)
		{
			return indexedTemplateForGroupID;
		}
		return null;
	}

	private IList<GameplayTemplate> GetAllTemplatesForDistance(GameplayTemplate.Group groupID, float distance, Func<GameplayTemplate, bool> customIsSuitable)
	{
		float difficultyAtDistance = Difficulty.GetDifficultyAtDistance(distance);
		Pair<float, float> minMaxDifficulty = Difficulty.GetMinMaxDifficulty();
		float value = Utils.NormalDistribution(m_rng, difficultyAtDistance, m_difficultyScatter, 2);
		value = Mathf.Clamp(value, minMaxDifficulty.First, minMaxDifficulty.Second);
		List<GameplayTemplate> list = new List<GameplayTemplate>();
		foreach (GameplayTemplate item in m_database)
		{
			if (item.ContainsGroup(groupID) && (customIsSuitable == null || customIsSuitable(item)) && item.IsDifficultyMatch(value))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private GameplayTemplate PickTemplateForDistance(GameplayTemplate.Group groupID, float distance, uint excludedTemplates, Failure canFail)
	{
		if (groupID == GameplayTemplate.Group.SetPiece)
		{
			return PickOrderedGameplayTemplate(groupID);
		}
		Func<GameplayTemplate, bool> customIsSuitable = null;
		if ((excludedTemplates & 2u) != 0)
		{
			customIsSuitable = (GameplayTemplate template) => !template.HasGaps;
		}
		IList<GameplayTemplate> allTemplatesForDistance = GetAllTemplatesForDistance(groupID, distance, customIsSuitable);
		return m_database.PickRandomTemplate(allTemplatesForDistance, m_rng, m_lastTemplate);
	}

	private GameplayTemplate PickOrderedGameplayTemplate(GameplayTemplate.Group group)
	{
		if (group == GameplayTemplate.Group.SetPiece)
		{
			IEnumerable<GameplayTemplate> source = m_database.Where((GameplayTemplate template) => template.ContainsGroup(group));
			if (!source.Any())
			{
				return null;
			}
			int nextOrderedSetPieceIndex = m_nextOrderedSetPieceIndex;
			m_nextOrderedSetPieceIndex = (m_nextOrderedSetPieceIndex + 1) % source.Count();
			return source.ElementAt(nextOrderedSetPieceIndex);
		}
		return null;
	}

	private GameObject CreateChopper(Spline spline, LightweightTransform transform, Enemy.Direction direction, float trackDistance)
	{
		if (m_enemyGenerator == null || !m_enemyGenerator.enabled)
		{
			return null;
		}
		return m_enemyGenerator.GenerateEnemy(typeof(Chopper), transform, Track, spline, direction, trackDistance);
	}

	private GameObject CreatePowerup(Spline spline, LightweightTransform transform, TrackEntity.Kind powerupType)
	{
		if (m_powerupGenerator == null || !m_powerupGenerator.enabled)
		{
			return null;
		}
		GameObject gameObject = m_powerupGenerator.GenerateSpawnable(typeof(Capsule), transform, Track, spline);
		gameObject.GetComponent<Capsule>().PowerupType = powerupType;
		return gameObject;
	}

	private GameObject CreateChallengePiece(Spline spline, LightweightTransform transform)
	{
		if (m_powerupGenerator == null || !m_powerupGenerator.enabled)
		{
			return null;
		}
		return m_powerupGenerator.GenerateDCPiece(DCs.GetNextPieceNumber(), transform, Track, spline);
	}

	private GameObject CreateRedStarRing(Spline spline, LightweightTransform transform)
	{
		if (m_powerupGenerator == null || !m_powerupGenerator.enabled)
		{
			return null;
		}
		return m_powerupGenerator.GenerateSpawnable(typeof(RSR), transform, Track, spline);
	}

	private GameObject CreateGCCollectable(Spline spline, LightweightTransform transform)
	{
		if (m_powerupGenerator == null || !m_powerupGenerator.enabled)
		{
			return null;
		}
		return m_powerupGenerator.GenerateGCCollectable(transform, Track, spline);
	}

	private GameObject CreateObstacle(TrackEntity.Kind obstacleType, Spline spline, LightweightTransform worldTransform, ref int desiredLaneCount)
	{
		if (m_obstacleGenerator == null || !m_obstacleGenerator.enabled)
		{
			return null;
		}
		SpawnPool spawnPool = m_obstacleGenerator.SpawnPool;
		uint obstacleCharacteristics = 0u;
		if ((obstacleType & (TrackEntity.Kind)571) != 0)
		{
			obstacleCharacteristics |= 1u;
		}
		if ((obstacleType & (TrackEntity.Kind)98) != 0)
		{
			obstacleCharacteristics |= 2u;
		}
		Type desiredType = obstacleType switch
		{
			TrackEntity.Kind.SpikePit => typeof(ObstacleSpikes), 
			TrackEntity.Kind.Mine => typeof(ObstacleMine), 
			_ => typeof(Wall), 
		};
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(spline);
		int segmentSubzoneIndex = segmentOfSpline.Template.SubzoneIndex;
		IEnumerable<Obstacle> source = from prefab in spawnPool.prefabs.Values
			select prefab.GetComponent<Obstacle>() into obstacle
			where obstacle != null
			select obstacle;
		IEnumerable<Obstacle> source2 = source.Where((Obstacle obstacleComponent) => obstacleComponent.IsExactlyGameplayType(obstacleCharacteristics) && desiredType.IsAssignableFrom(obstacleComponent.GetType()));
		int bestLaneCount = desiredLaneCount;
		IEnumerable<Obstacle> enumerable = source2.Where((Obstacle obstacle) => obstacle.NumLanesOccupied == bestLaneCount);
		if (!enumerable.Any())
		{
			enumerable = source2.Where((Obstacle obstacle) => obstacle.NumLanesOccupied == 1);
			desiredLaneCount = 1;
		}
		IEnumerable<Obstacle> enumerable2 = enumerable.Where((Obstacle obstacleComponent) => obstacleComponent.WorksOnAnySubzone || obstacleComponent.SubzoneIndex == segmentSubzoneIndex);
		IEnumerable<Obstacle> enumerable4;
		if (enumerable2.Any())
		{
			IEnumerable<Obstacle> enumerable3 = enumerable2;
			enumerable4 = enumerable3;
		}
		else
		{
			enumerable4 = enumerable;
		}
		IEnumerable<Obstacle> source3 = enumerable4;
		Obstacle obstacle2 = source3.ElementAt(m_rng.Next(source3.Count()));
		Transform prefab2 = obstacle2.transform;
		return m_obstacleGenerator.GenerateObstacle(prefab2, worldTransform, Track, spline);
	}

	private GameObject CreateGroundEnemy(TrackEntity.Kind enemyType, Enemy.Direction dir, Spline spline, LightweightTransform worldTransform, float trackDistance)
	{
		if (m_enemyGenerator == null || !m_enemyGenerator.enabled)
		{
			return null;
		}
		dir = ((dir != 0 && dir != Enemy.Direction.AwayFromPlayer) ? dir : Enemy.Direction.Stationary);
		Type enemyType2 = ((enemyType != TrackEntity.Kind.Spikes) ? typeof(Crabmeat) : typeof(Spikes));
		Quaternion rot = Quaternion.LookRotation(-worldTransform.Forwards, worldTransform.Up);
		LightweightTransform atTransform = new LightweightTransform(worldTransform.Location, rot);
		return m_enemyGenerator.GenerateEnemy(enemyType2, atTransform, Track, spline, dir, trackDistance);
	}

	private GameObject CreateDashPad(Spline spline, LightweightTransform worldTransform)
	{
		if (m_miscGenerator == null || !m_miscGenerator.enabled)
		{
			return null;
		}
		return m_miscGenerator.GenerateSpawnable(typeof(DashPad), worldTransform, Track, spline);
	}

	private GameObject CreateDistanceTV(Spline spline, LightweightTransform worldTransform, int TextDistance)
	{
		if (m_miscGenerator == null || !m_miscGenerator.enabled)
		{
			return null;
		}
		GameObject gameObject = m_miscGenerator.GenerateSpawnable(typeof(DistanceTV), worldTransform, Track, spline);
		((DistanceTV)gameObject.GetComponent(typeof(DistanceTV))).SetDistanceValue(TextDistance);
		return gameObject;
	}

	private GameObject CreateSpring(Spline spline, LightweightTransform worldTransform, Track.Lane trackLane)
	{
		if (m_miscGenerator == null || !m_miscGenerator.enabled)
		{
			return null;
		}
		Spring spring = m_miscGenerator.GenerateSpawnable<Spring>(worldTransform, Track, spline);
		SpringTV newTV = m_miscGenerator.GenerateSpawnable<SpringTV>(worldTransform, Track, spline);
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(spline);
		CalculateNextSpringType(spring, newTV, segmentOfSpline, trackLane);
		return spring.gameObject;
	}

	private SpringTV.Destination OnChoice(int choiceIndex)
	{
		SpringTV.Type item = m_choiceTypeOrder[choiceIndex];
		m_choiceTypeOrder.RemoveAt(choiceIndex);
		m_choiceTypeOrder.Add(item);
		SpringTV.Destination destination = m_choiceDestinationOrder[choiceIndex];
		m_choiceDestinationOrder.RemoveAt(choiceIndex);
		m_choiceDestinationOrder.Add(destination);
		return destination;
	}

	private void CalculateNextSpringType(Spring newSpring, SpringTV newTV, TrackSegment segment, Track.Lane trackLane)
	{
		TrackGenerator trackGenerator = Sonic.Tracker.Track as TrackGenerator;
		SpringTV.Destination destination = trackGenerator.CalculateCurrentSubzoneIndex() switch
		{
			0 => SpringTV.Destination.Grass, 
			1 => SpringTV.Destination.Temple, 
			_ => SpringTV.Destination.Beach, 
		};
		bool flag = false;
		float trackPosition = segment.TrackPosition;
		SpringTV.Type type = SpringTV.Type.Random;
		SpringTV.CreateFlags createFlags = SpringTV.CreateFlags.None;
		if (trackGenerator.CanHaveBossBattle)
		{
			createFlags |= SpringTV.CreateFlags.BossBattle;
		}
		string empty = string.Empty;
		BossBattleSystem bossBattleSystem = BossBattleSystem.Instance();
		if (bossBattleSystem != null && bossBattleSystem.IsEnabled() && bossBattleSystem.GetCurrentPhase() == 0)
		{
			type = SpringTV.Type.Boss;
			flag = true;
		}
		else if (m_isNextSpringABankSpring != null && m_isNextSpringABankSpring.Any() && m_isNextSpringABankSpring.Dequeue())
		{
			type = SpringTV.Type.Bank;
			m_springBankIsSpawned = true;
			flag = true;
		}
		else if ((float)m_rng.NextDouble() < DCs.GetChallengeSpringChance(trackPosition))
		{
			DCs.ChallengeSpringCreated = true;
			type = SpringTV.Type.DailyChallenge;
			flag = true;
		}
		else
		{
			int num = ((!m_springBankIsSpawned) ? m_springLane : (m_springLane - 1));
			float num2 = (float)m_rng.NextDouble();
			float num3 = 0f;
			empty = $" Rnd = {(int)(num2 * 100f)}%";
			for (int i = 0; i < m_choiceTypeOrder.Count; i++)
			{
				if (m_choiceTypeOrder[i] == SpringTV.Type.ChangeZone)
				{
					if (m_choiceDestinationOrder[i] != destination && !m_springChangeZoneIsSpawned)
					{
						num3 += m_parameters.ChangeSubzoneSpringChance(num, m_choiceDestinationOrder[i]);
						if (num2 < num3)
						{
							type = SpringTV.Type.ChangeZone;
							destination = OnChoice(i);
							m_springChangeZoneIsSpawned = true;
							flag = true;
							break;
						}
					}
				}
				else if (m_choiceTypeOrder[i] == SpringTV.Type.SetPiece)
				{
					if (trackPosition - m_lastSetPieceSpringDistance >= m_parameters.MinDistanceBetweenSetPieceSprings)
					{
						num3 += m_parameters.SetPieceSpringChance(num);
						if (num2 < num3)
						{
							type = SpringTV.Type.SetPiece;
							OnChoice(i);
							flag = true;
							break;
						}
					}
				}
				else if (m_choiceTypeOrder[i] == SpringTV.Type.Random)
				{
					num3 += m_parameters.RandomSpringChance(num);
					if (num2 < num3)
					{
						type = SpringTV.Type.Random;
						OnChoice(i);
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			for (int j = 0; j < m_choiceTypeOrder.Count; j++)
			{
				if (m_choiceTypeOrder[j] == SpringTV.Type.Random)
				{
					OnChoice(j);
					flag = true;
					break;
				}
			}
		}
		newTV.PlaceOnSpring(newSpring, m_rng, type, destination, createFlags);
		if (type == SpringTV.Type.SetPiece)
		{
			m_lastSetPieceSpringDistance = segment.TrackPosition;
		}
		m_springLane++;
	}

	private float CalculateTemplateSpacing()
	{
		return m_parameters.RowLength;
	}

	private TemplateInstantiator CreateInstantiator(GameplayTemplate template)
	{
		QueueUpBankSprings(template.SpringCount, BankSpringsNextTemplate, template.Name);
		BankSpringsNextTemplate = -1;
		m_lastTemplate = template;
		float templateSpacing = CalculateTemplateSpacing();
		float jumpHeight = Sonic.Handling.JumpHeight;
		return new TemplateInstantiator(template, templateSpacing, jumpHeight, this);
	}

	private void QueueUpBankSprings(int totalSpringCount, int bankSpringCount, string templateName)
	{
		m_isNextSpringABankSpring = null;
		if (bankSpringCount >= 0)
		{
			bool[] array = new bool[totalSpringCount];
			for (int i = 0; i < totalSpringCount; i++)
			{
				array[i] = i < bankSpringCount;
			}
			IEnumerable<bool> collection = Utils.Shuffle(array, RNG);
			m_springLane = 0;
			m_springBankIsSpawned = false;
			m_springChangeZoneIsSpawned = false;
			m_isNextSpringABankSpring = new Queue<bool>(collection);
		}
	}
}
