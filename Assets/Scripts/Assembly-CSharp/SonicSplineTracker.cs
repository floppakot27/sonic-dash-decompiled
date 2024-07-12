using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[AddComponentMenu("Dash/Sonic/Spline Tracker")]
public class SonicSplineTracker : MonoBehaviour
{
	[StructLayout(0, Size = 1)]
	public struct MovementInfo
	{
		public float Delta { get; set; }

		public float PrevTrackPosition { get; set; }

		public float NewTrackPosition { get; set; }

		public Track.Lane Lane { get; set; }
	}

	[StructLayout(0, Size = 1)]
	private struct DeathState
	{
		public bool IsDead { get; set; }

		public SplineKnot.EndBehaviour DeathType { get; set; }
	}

	public const string RootObjectName = "World";

	public const string YOnlyRootObjectName = "YOnlyWorld";

	private bool m_forceSnapRotation;

	private float m_forceRotationTimer = -1f;

	private float m_distanceToNextGap = 1000000f;

	private bool m_collisionsEnabled = true;

	private System.Random m_rng;

	private bool m_sonicDiedWithActiveChoppers;

	private bool m_readyToUpdate;

	private Transform m_YOnlyRootTransform;

	private IList<TrackEntity> m_entityScratchList = new List<TrackEntity>();

	private TrackGenerator m_track;

	[SerializeField]
	private bool m_isInvulnerable;

	private SplineTracker m_internalTracker;

	private float m_targetSpeed;

	private LightweightTransform m_lastSplineTransform;

	private MotionStateMachine m_motionStateMachine;

	private SonicPhysics m_physics;

	private CameraTypeMain m_camera;

	private LightweightTransform m_lastCameraTransform;

	private static Transform s_worldTransform;

	private static Transform s_movableOnlyInYWorldTransform;

	private GameObject m_toggleOnAboveObject;

	private GameObject m_toggleOnBelowObject;

	private GameObject m_toggleOnAboveWater;

	private GameObject m_toggleOnBelowWater;

	private float m_distanceTravelled;

	private LightweightTransform m_gameStartTransform;

	private LightweightTransform m_gameStartTrackTransform;

	private bool m_onSetPiece;

	private bool m_overGap;

	private bool m_overSmallIsland;

	private bool m_InsideGapTriggerBox;

	private Vector3 m_currentVelocity = Vector3.zero;

	private float m_ghostedTime;

	[SerializeField]
	private float m_respawnClearRange = 50f;

	[SerializeField]
	private float m_respawnGapClearRange = 90f;

	private Fader m_fader;

	public bool IsReadyToUpdate => m_readyToUpdate;

	public static bool AllowRunning { get; set; }

	public bool IsSpringJumping
	{
		get
		{
			if (m_motionStateMachine == null)
			{
				return false;
			}
			return (m_motionStateMachine.HasActiveState && m_motionStateMachine.CurrentState.IsSpringing()) || (m_motionStateMachine.PendingState != null && m_motionStateMachine.PendingState.IsSpringing());
		}
	}

	public bool IsRolling => m_motionStateMachine != null && m_motionStateMachine.HasActiveState && m_motionStateMachine.CurrentState is MotionRollState;

	public SonicHandling Handling => Sonic.Handling;

	public bool Invulnerable
	{
		set
		{
			m_isInvulnerable = value;
		}
	}

	public float Speed => (!base.enabled || m_internalTracker == null) ? 0f : m_internalTracker.TrackSpeed;

	public float TargetSpeed => m_physics.TargetSpeed;

	public Spline CurrentSpline => (!base.enabled || m_internalTracker == null) ? null : m_internalTracker.Target;

	public float DistanceTravelled => m_distanceTravelled;

	public LightweightTransform IdealPosition => m_internalTracker.CurrentSplineTransform;

	public Track Track => m_track;

	public float JumpHeight
	{
		get
		{
			if (m_physics == null)
			{
				return 0f;
			}
			return m_physics.JumpHeight;
		}
	}

	public SplineTracker InternalTracker => m_internalTracker;

	public MotionStateMachine InternalMotionState => m_motionStateMachine;

	public float HeightAboveLowGround => base.transform.position.y - s_worldTransform.position.y;

	public float MaxTrackPositionReached { get; private set; }

	public float TrackPosition { get; private set; }

	public bool IsTrackerAvailable => m_internalTracker != null && m_internalTracker.CanUpdate;

	public bool TrackerOverGap
	{
		get
		{
			return m_overGap && !m_isInvulnerable;
		}
		set
		{
			m_overGap = value;
		}
	}

	public bool TrackerOverSmallIsland
	{
		get
		{
			return m_overSmallIsland;
		}
		set
		{
			m_overSmallIsland = value;
		}
	}

	public Vector3 CurrentVelocity => m_currentVelocity;

	public static Transform FindRootTransform()
	{
		GameObject gameObject = GameObject.Find("World");
		return (!(gameObject == null)) ? gameObject.transform : null;
	}

	public static Transform FindYOnlyRootTransform()
	{
		GameObject gameObject = GameObject.Find("YOnlyWorld");
		return (!(gameObject == null)) ? gameObject.transform : null;
	}

	private void SetGhosted()
	{
		m_ghostedTime = Sonic.Handling.InvulnerableDuration;
	}

	public bool GetIsGhosted()
	{
		return m_ghostedTime > 0f;
	}

	public void enableCollisions()
	{
		m_collisionsEnabled = true;
	}

	public void disableCollisions()
	{
		m_collisionsEnabled = false;
	}

	public void Awake()
	{
		m_track = UnityEngine.Object.FindObjectOfType(typeof(TrackGenerator)) as TrackGenerator;
		m_gameStartTransform = new LightweightTransform(base.transform);
		Component component = UnityEngine.Object.FindObjectOfType(typeof(Track)) as Component;
		m_gameStartTrackTransform = new LightweightTransform(component.transform);
		m_camera = UnityEngine.Object.FindObjectOfType(typeof(CameraTypeMain)) as CameraTypeMain;
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnTrackGenerationComplete", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		m_rng = new System.Random();
		s_worldTransform = CreateWorldTransform(s_worldTransform, "World", WorldCollector.FindAllMovableGameObjects());
		m_ghostedTime = 0f;
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
		Sonic.Clear();
	}

	public void Start()
	{
		m_fader = UnityEngine.Object.FindObjectOfType(typeof(Fader)) as Fader;
	}

	public void OnEnable()
	{
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		int subzoneIndex = m_track.ConvertDestinationToSubzoneIndex(request.Destination);
		s_worldTransform.position = new Vector3(0f, m_track.GenerationParams.SubzoneWorldHeightOffset(subzoneIndex), 0f);
		m_lastCameraTransform.Location = m_camera.transform.position;
		m_lastCameraTransform.Orientation = m_camera.transform.rotation;
		SetToggleObjectsActive();
		OnTrackDestroyed();
		m_physics.HaltSonic();
		Quaternion quaternion = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
		Track componentInChildren = s_worldTransform.GetComponentInChildren<Track>();
		componentInChildren.transform.rotation = quaternion * m_gameStartTrackTransform.Orientation;
	}

	private void Event_OnNewGameStarted()
	{
		m_gameStartTransform.ApplyTo(base.transform);
		m_forceSnapRotation = false;
		m_forceRotationTimer = -1f;
		m_sonicDiedWithActiveChoppers = false;
	}

	private void Event_OnSonicDeath()
	{
		if (TargetManager.instance().GetNumberOfActiveChoppers() > 0)
		{
			m_sonicDiedWithActiveChoppers = true;
		}
		else
		{
			m_sonicDiedWithActiveChoppers = false;
		}
	}

	private void OnTrackDestroyed()
	{
		m_internalTracker = null;
	}

	public void OnGameReset(GameState.Mode mode)
	{
		ResetTracker();
		if (mode == GameState.Mode.Game)
		{
			base.transform.position = Vector3.zero;
			base.transform.rotation = Quaternion.identity;
		}
	}

	public void ResetTracker()
	{
		OnTrackDestroyed();
		TrackPosition = 0f;
		MaxTrackPositionReached = 0f;
		m_onSetPiece = false;
		TrackerOverGap = false;
		TrackerOverSmallIsland = false;
		m_InsideGapTriggerBox = false;
		Physics.IgnoreLayerCollision(16, 24, ignore: true);
		m_distanceTravelled = 0f;
		if (m_motionStateMachine != null)
		{
			m_motionStateMachine.ShutDown();
		}
		m_motionStateMachine = new MotionStateMachine(new MotionSprintState());
		m_physics = new SonicPhysics();
		s_worldTransform = CreateWorldTransform(s_worldTransform, "World", WorldCollector.FindAllMovableGameObjects());
		m_lastCameraTransform.Location = m_camera.transform.position;
		m_lastCameraTransform.Orientation = m_camera.transform.rotation;
		Track track = ((!(s_worldTransform != null)) ? null : s_worldTransform.GetComponentInChildren<Track>());
		if ((bool)track)
		{
			track.transform.rotation = m_gameStartTrackTransform.Orientation;
		}
		m_ghostedTime = 0f;
		m_readyToUpdate = true;
	}

	private void Event_OnTrackGenerationComplete()
	{
		if (m_toggleOnAboveObject == null)
		{
			string setPieceHeightToggleOnAbove = m_track.GenerationParams.SetPieceHeightToggleOnAbove;
			if (setPieceHeightToggleOnAbove != null)
			{
				m_toggleOnAboveObject = GameObject.Find(setPieceHeightToggleOnAbove);
			}
		}
		if (m_toggleOnAboveWater == null)
		{
			m_toggleOnAboveWater = GameObject.FindGameObjectWithTag("WaterTop");
		}
		if (m_toggleOnBelowObject == null)
		{
			string setPieceHeightToggleOnBelow = m_track.GenerationParams.SetPieceHeightToggleOnBelow;
			if (setPieceHeightToggleOnBelow != null)
			{
				m_toggleOnBelowObject = GameObject.Find(setPieceHeightToggleOnBelow);
			}
		}
		if (m_toggleOnBelowWater == null)
		{
			m_toggleOnBelowWater = GameObject.FindGameObjectWithTag("WaterBottom");
			if ((bool)m_toggleOnBelowWater)
			{
				m_toggleOnBelowWater.SetActive(value: false);
			}
		}
		SetToggleObjectsActive();
		TrackerOverGap = false;
		TrackerOverSmallIsland = false;
		s_movableOnlyInYWorldTransform = CreateWorldTransform(s_movableOnlyInYWorldTransform, "YOnlyWorld", WorldCollector.FindAllMovableOnlyInYGameObjects());
		StartCoroutine(WaitForCleanTrack());
	}

	public void Update()
	{
		if (!AllowRunning || !m_readyToUpdate)
		{
			return;
		}
		UpdateWorldContainer();
		m_physics.PreUpdate();
		bool flag = m_internalTracker != null || IsSpringJumping;
		if (flag)
		{
			m_motionStateMachine.Update();
		}
		if (m_forceSnapRotation && m_forceRotationTimer > 0f)
		{
			m_forceRotationTimer -= IndependantTimeDelta.Delta;
			if (!(m_forceRotationTimer > 0f))
			{
				m_forceSnapRotation = false;
				m_forceRotationTimer = -1f;
			}
		}
		if (m_internalTracker != null)
		{
			if (m_forceSnapRotation)
			{
				m_internalTracker.ForceSnapRotation();
			}
			else
			{
				m_internalTracker.DisableForceSnapRotation();
			}
		}
		LightweightTransform lightweightTransform = new LightweightTransform(base.transform.position, base.transform.rotation);
		MotionState.TransformParameters transformParameters = default(MotionState.TransformParameters);
		transformParameters.Tracker = m_internalTracker;
		transformParameters.StateMachine = m_motionStateMachine;
		transformParameters.CurrentTransform = lightweightTransform;
		transformParameters.Physics = m_physics;
		transformParameters.Track = m_track;
		transformParameters.OverGap = TrackerOverGap;
		transformParameters.OverSmallIsland = TrackerOverSmallIsland;
		MotionState.TransformParameters tParams = transformParameters;
		LightweightTransform lightweightTransform2 = ((!flag) ? lightweightTransform : m_motionStateMachine.CurrentState.CalculateNewTransform(tParams));
		m_physics.Update();
		if (m_internalTracker != null)
		{
			m_internalTracker.TrackSpeed = m_physics.CurrentSpeed;
			if (m_internalTracker.IsReversed && m_internalTracker.RunBackwards)
			{
				m_internalTracker.TrackSpeed *= Handling.RewindSpeedModifier;
			}
		}
		if (flag && !m_motionStateMachine.CurrentState.IsFalling())
		{
			Vector3 vector = base.transform.position - lightweightTransform2.Location;
			UpdateWorldWithDelta(vector);
			base.transform.rotation = lightweightTransform2.Orientation;
			if ((double)Time.deltaTime > 1E-08)
			{
				m_currentVelocity = -vector / Time.deltaTime;
			}
		}
		if (m_internalTracker == null)
		{
			return;
		}
		m_internalTracker.ForceUpdateTransform();
		UpdateTransitionToNewSpline();
		DeathState deathState = getDeathState();
		if (deathState.IsDead)
		{
			UpdateDeath(deathState.DeathType);
		}
		float previousDelta = m_internalTracker.PreviousDelta;
		float num = Track.CalculateTrackPositionOfTracker(m_internalTracker);
		if (num > MaxTrackPositionReached)
		{
			m_distanceTravelled += previousDelta;
			MaxTrackPositionReached = num;
		}
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(CurrentSpline);
		if (segmentOfSpline != null)
		{
			MovementInfo movementInfo = default(MovementInfo);
			movementInfo.Delta = previousDelta;
			movementInfo.PrevTrackPosition = TrackPosition;
			movementInfo.NewTrackPosition = num;
			movementInfo.Lane = Track.GetLaneOfSpline(CurrentSpline);
			MovementInfo info = movementInfo;
			Sonic.fireMovementEvent(info);
		}
		TrackEventsPassed(num);
		TrackPosition = num;
		TrackSegment segmentOfSpline2 = TrackSegment.GetSegmentOfSpline(CurrentSpline);
		CheckForSetPiece(segmentOfSpline2);
		CheckForGaps(segmentOfSpline2);
		UpdateAudioControl(segmentOfSpline2);
		TutorialSystem.instance().UpdateSystem(TrackPosition);
		BossBattleSystem bossBattleSystem = BossBattleSystem.Instance();
		if (bossBattleSystem != null)
		{
			bossBattleSystem.UpdateSystem(TrackPosition);
		}
		if (m_ghostedTime > 0f)
		{
			m_ghostedTime -= Time.deltaTime;
			if (m_ghostedTime < 0f)
			{
				m_ghostedTime = 0f;
			}
		}
	}

	private void UpdateAudioControl(TrackSegment segment)
	{
		if ((bool)segment)
		{
			Sonic.AudioControl.SetMaterialType((segment.Template.SubzoneIndex != 0) ? SonicAudioControl.MaterialType.Stone : SonicAudioControl.MaterialType.Grass);
		}
	}

	private void CheckForSetPiece(TrackSegment segment)
	{
		if (!(CurrentSpline != null))
		{
			return;
		}
		if (!m_onSetPiece)
		{
			if (TrackDatabase.IsSetPiece(segment.Template.PieceType.Type))
			{
				base.gameObject.SendMessage("OnEnterSetPiece");
				m_onSetPiece = true;
			}
		}
		else if (!TrackDatabase.IsSetPiece(segment.Template.PieceType.Type))
		{
			base.gameObject.SendMessage("OnLeftSetPiece");
			m_onSetPiece = false;
		}
	}

	private void CheckForGaps(TrackSegment segment)
	{
		m_overGap = false;
		m_overSmallIsland = false;
		if (CurrentSpline != null)
		{
			Track.Lane laneOfSpline = Track.GetLaneOfSpline(CurrentSpline);
			if (segment.IsMissingLane(laneOfSpline) && !m_InsideGapTriggerBox)
			{
				m_overGap = true;
			}
		}
		if (!m_overGap && CurrentSpline != null)
		{
			Track.Lane laneOfSpline2 = Track.GetLaneOfSpline(CurrentSpline);
			if (segment.IsSmallIsland(laneOfSpline2))
			{
				m_overSmallIsland = true;
			}
		}
		m_distanceToNextGap = 1000000f;
		if (CurrentSpline != null)
		{
			Track.Lane laneOfSpline3 = Track.GetLaneOfSpline(CurrentSpline);
			if (segment != null)
			{
				m_distanceToNextGap = segment.GetDistanceToNextGap(laneOfSpline3, TrackPosition);
			}
		}
	}

	private bool HasGapWithinRange(Track.Lane lane, TrackSegment segment, float range)
	{
		while (segment != null)
		{
			if (segment.NextSegment == null || segment.NextSegment.NextSegment == null)
			{
				return false;
			}
			float num = segment.TrackPosition - TrackPosition;
			if (num > range)
			{
				return false;
			}
			if (segment.IsMissingLane(lane))
			{
				Chopper[] componentsInChildren = segment.gameObject.GetComponentsInChildren<Chopper>();
				if (componentsInChildren == null || componentsInChildren.Length == 0)
				{
					return true;
				}
			}
			segment = segment.NextSegment;
		}
		return false;
	}

	public TrackSegment GetTrackSegmentForRevive()
	{
		Track.Lane laneOfSpline = Track.GetLaneOfSpline(CurrentSpline);
		TrackSegment trackSegment = TrackSegment.GetSegmentOfSpline(CurrentSpline);
		while (null != trackSegment)
		{
			if (trackSegment.Template.PieceType.Type == TrackDatabase.PieceType.EmptyAir)
			{
				Chopper[] componentsInChildren = trackSegment.gameObject.GetComponentsInChildren<Chopper>();
				Chopper[] array = componentsInChildren;
				foreach (Chopper chopper in array)
				{
					if (chopper.isAnimating())
					{
						return trackSegment.NextSegment;
					}
				}
				break;
			}
			trackSegment = trackSegment.NextSegment;
		}
		trackSegment = TrackSegment.GetSegmentOfSpline(CurrentSpline);
		while (null != trackSegment && HasGapWithinRange(laneOfSpline, trackSegment, m_respawnGapClearRange))
		{
			trackSegment = trackSegment.NextSegment;
		}
		return trackSegment;
	}

	public TrackSegment GetTrackSegmentForReset()
	{
		if (CurrentSpline != null)
		{
			Track.Lane laneOfSpline = Track.GetLaneOfSpline(CurrentSpline);
			TrackSegment trackSegment = TrackSegment.GetSegmentOfSpline(CurrentSpline);
			while (null != trackSegment && trackSegment.IsMissingLane(laneOfSpline))
			{
				trackSegment = trackSegment.PreviousSegment;
			}
			return trackSegment;
		}
		return null;
	}

	public TrackSegment GetTrackSegmentForRevive_Simple()
	{
		if (CurrentSpline != null)
		{
			if (!m_sonicDiedWithActiveChoppers || TutorialSystem.instance().isTrackTutorialEnabled())
			{
				Track.Lane laneOfSpline = Track.GetLaneOfSpline(CurrentSpline);
				TrackSegment trackSegment = TrackSegment.GetSegmentOfSpline(CurrentSpline);
				while (null != trackSegment && trackSegment.IsMissingLane(laneOfSpline))
				{
					trackSegment = trackSegment.NextSegment;
				}
				return trackSegment;
			}
			Track.Lane laneOfSpline2 = Track.GetLaneOfSpline(CurrentSpline);
			TrackSegment trackSegment2 = TrackSegment.GetSegmentOfSpline(CurrentSpline);
			TrackSegment result = trackSegment2;
			if (null != trackSegment2)
			{
				if (trackSegment2.IsMissingLane(laneOfSpline2))
				{
					while (null != trackSegment2 && trackSegment2.IsMissingLane(laneOfSpline2))
					{
						trackSegment2 = trackSegment2.NextSegment;
					}
					return trackSegment2;
				}
				while (null != trackSegment2 && !trackSegment2.IsMissingLane(laneOfSpline2))
				{
					trackSegment2 = trackSegment2.NextSegment;
				}
				if (null == trackSegment2)
				{
					return result;
				}
				while (null != trackSegment2 && trackSegment2.IsMissingLane(laneOfSpline2))
				{
					trackSegment2 = trackSegment2.NextSegment;
				}
				if (null == trackSegment2)
				{
					return result;
				}
				return trackSegment2;
			}
		}
		return null;
	}

	public bool gapRespawnIsPermitted()
	{
		if (CurrentSpline == null || GetTrackSegmentForRevive_Simple() == null)
		{
			return false;
		}
		return true;
	}

	private bool IsCurrentLaneAGap()
	{
		if (IsSpringJumping)
		{
			return false;
		}
		if (CurrentSpline != null)
		{
			Track.Lane laneOfSpline = Track.GetLaneOfSpline(CurrentSpline);
			TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(CurrentSpline);
			if (segmentOfSpline.IsMissingLane(laneOfSpline))
			{
				return true;
			}
		}
		return false;
	}

	private void TeleportToEndOfGap()
	{
		if (CurrentSpline != null)
		{
			Track.Lane laneOfSpline = Track.GetLaneOfSpline(CurrentSpline);
			TrackSegment trackSegment = TrackSegment.GetSegmentOfSpline(CurrentSpline);
			while (null != trackSegment && trackSegment.IsMissingLane(laneOfSpline))
			{
				trackSegment = trackSegment.NextSegment;
			}
			if (null != trackSegment)
			{
				Spline spline = null;
				float num = 100000000f;
				float initialPosition = 0f;
				Spline spline2 = trackSegment.GetSpline((int)laneOfSpline);
				Spline[] componentsInChildren = spline2.transform.parent.GetComponentsInChildren<Spline>();
				Spline[] array = componentsInChildren;
				foreach (Spline spline3 in array)
				{
					Utils.ClosestPoint closestPoint = spline3.EstimateDistanceAlongSpline(Vector3.zero);
					if (spline == null || closestPoint.SqrError < num)
					{
						num = closestPoint.SqrError;
						spline = spline3;
						initialPosition = closestPoint.LineDistance;
					}
				}
				m_internalTracker.Target = spline;
				m_internalTracker.Start(m_internalTracker.TrackSpeed, initialPosition, Direction_1D.Forwards);
				base.gameObject.SendMessage("OnStrafe", SideDirection.Left, SendMessageOptions.DontRequireReceiver);
				m_internalTracker.requestTeleport();
			}
		}
		m_overGap = false;
		m_overSmallIsland = false;
	}

	public void triggerRespawn(bool usedPowerUp)
	{
		Sonic.AnimationControl.OnRespawn();
		if (usedPowerUp)
		{
			Sonic.AnimationControl.TriggerRespawnPowerupEffect(freeRevive: false);
			PowerUpsInventory.ModifyPowerUpStock(PowerUps.Type.Respawn, -1);
			Sonic.AudioControl.PlayRespawnSFX();
		}
		ClearTrackForRespawn();
		m_fader.flash(0.2f);
		if (IsCurrentLaneAGap())
		{
			TeleportToEndOfGap();
		}
		EventDispatch.GenerateEvent("OnSonicRespawn");
	}

	private void ClearTrackForRespawn()
	{
		float num = 5f;
		uint entityMask = 763u;
		Track.Info.EntitiesInRange(entityMask, TrackPosition - num, TrackPosition + m_respawnClearRange, ref m_entityScratchList);
		for (int i = 0; i < m_entityScratchList.Count; i++)
		{
			TrackEntity trackEntity = m_entityScratchList[i];
			if (!(trackEntity is TrackGameObject { IsValid: not false } trackGameObject))
			{
				continue;
			}
			Enemy component = trackGameObject.Object.GetComponent<Enemy>();
			if (!component || !component.isChopper())
			{
				if (DashMonitor.instance().isDashing())
				{
					PlayerStats.DashThroughObstacle(trackGameObject.Object);
				}
				object[] value = new object[2] { this, true };
				trackGameObject.Object.SendMessage("OnDeath", value);
				if ((bool)component)
				{
					object[] parameters = new object[2]
					{
						component,
						Enemy.Kill.Other
					};
					EventDispatch.GenerateEvent("OnEnemyKilled", parameters);
				}
			}
		}
	}

	private void OnEnterSetPiece()
	{
		m_motionStateMachine.ForceState(m_motionStateMachine.CurrentState.OnSetPiece());
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(m_internalTracker.Target);
		PlayerStats.EnterSetPiece(segmentOfSpline.Template.PieceType.Type);
		GetComponentInChildren<DashFX>().ForceIsDashFXActive(isActive: true);
		m_forceSnapRotation = true;
		m_forceRotationTimer = -1f;
	}

	private void OnLeftSetPiece()
	{
		m_motionStateMachine.CurrentState.OnSetPieceEnd(m_motionStateMachine);
		GetComponentInChildren<DashFX>().ForceIsDashFXActive(isActive: false);
		m_forceRotationTimer = 1f;
	}

	public void OnTriggerStay(Collider other)
	{
		if (base.enabled && !m_isInvulnerable && m_motionStateMachine.CurrentState.GetType() == typeof(MotionGroundStrafeState))
		{
			Hazard component = other.GetComponent<Hazard>();
			if ((bool)component && component.GetType() == typeof(Wall))
			{
				bool heldRings = RingStorage.HeldRings > 0;
				CollisionResolver.ResolutionType hazardCollisionResolution = component.CollisionResolver.Resolve(m_motionStateMachine.CurrentState, heldRings, GetIsGhosted());
				ResolveCollision(component, hazardCollisionResolution);
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!base.enabled || !m_collisionsEnabled)
		{
			return;
		}
		Powerup component = other.GetComponent<Powerup>();
		if ((bool)component)
		{
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerupsPicked_Total, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerupsPicked_Session, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerupsPicked_Run, 1);
			bool flag = false;
			bool flag2 = false;
			if (component.PowerupType == TrackEntity.Kind.RandomPowerup)
			{
				int magnetChance = MagnetMonitor.instance().getMagnetChance();
				int shieldChance = ShieldMonitor.instance().getShieldChance();
				int num = m_rng.Next() % 100;
				flag = num < magnetChance;
				flag2 = num >= magnetChance && num < magnetChance + shieldChance;
			}
			else if (component.PowerupType == TrackEntity.Kind.MagnetPowerup)
			{
				flag = true;
			}
			else if (component.PowerupType == TrackEntity.Kind.ShieldPowerup)
			{
				flag2 = true;
			}
			if (flag)
			{
				MagnetMonitor.instance().notifyMagnetPickup();
				MagnetMonitor.instance().requestMagnet();
				Sonic.AudioControl.PlayPowerUpMagnetPickupSFX();
			}
			else if (flag2)
			{
				ShieldMonitor.instance().notifyShieldPickup();
				ShieldMonitor.instance().requestShield();
				Sonic.AudioControl.PlayPowerUpShieldPickupSFX();
			}
			else
			{
				int ringsForRingsPickup = PowerUps.GetRingsForRingsPickup();
				PowerUps.DoRingPowerupAction(ringsForRingsPickup);
				Sonic.AudioControl.PlayPowerUpRingsPickupSFX();
			}
			component.notifyCollection();
			return;
		}
		DCPiece component2 = other.GetComponent<DCPiece>();
		if ((bool)component2)
		{
			component2.notifyCollection();
			return;
		}
		RSR component3 = other.GetComponent<RSR>();
		if ((bool)component3)
		{
			component3.notifyCollection();
			return;
		}
		GCCollectable component4 = other.GetComponent<GCCollectable>();
		if ((bool)component4)
		{
			component4.notifyCollection();
			return;
		}
		Hazard component5 = other.GetComponent<Hazard>();
		if ((bool)component5)
		{
			if (!m_isInvulnerable)
			{
				bool heldRings = RingStorage.HeldRings > 0;
				CollisionResolver.ResolutionType hazardCollisionResolution = component5.CollisionResolver.Resolve(m_motionStateMachine.CurrentState, heldRings, GetIsGhosted());
				ResolveCollision(component5, hazardCollisionResolution);
			}
			return;
		}
		Spring component6 = other.transform.parent.GetComponent<Spring>();
		if (component6 != null)
		{
			if (component6.GetSpringType() == SpringTV.Type.DailyChallenge)
			{
				DCs.SetChallengePieceSpawn(spawn: true);
			}
			else
			{
				DCs.SetChallengePieceSpawn(spawn: false);
			}
			m_motionStateMachine.ForceState(m_motionStateMachine.CurrentState.OnSpring(Track, Sonic.Handling, m_physics, component6.GetSpringType(), component6.GetDestination(), component6.GetCreateFlags()));
			component6.OnCollision();
		}
		if (other.gameObject.layer == 25)
		{
			m_InsideGapTriggerBox = true;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 25)
		{
			m_InsideGapTriggerBox = false;
		}
	}

	private void ResolveCollision(Hazard hazard, CollisionResolver.ResolutionType hazardCollisionResolution)
	{
		Enemy component = hazard.gameObject.GetComponent<Enemy>();
		CollisionResolver.ResolutionType resolutionType = hazardCollisionResolution;
		bool flag = false;
		bool isBallShown = Sonic.AnimationControl.IsBallShown;
		if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting() || ShieldMonitor.instance().isShielded())
		{
			if (hazardCollisionResolution == CollisionResolver.ResolutionType.SonicDeath || hazardCollisionResolution == CollisionResolver.ResolutionType.SonicDieForwards || hazardCollisionResolution == CollisionResolver.ResolutionType.SonicStumble)
			{
				resolutionType = CollisionResolver.ResolutionType.EnemyDeath;
				if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
				{
					triggerRespawn(usedPowerUp: false);
					flag = true;
				}
				else if (!isBallShown || !(null != component) || !component.isAttackableFromAir())
				{
					triggerRespawn(usedPowerUp: false);
					flag = true;
					ShieldMonitor.instance().EndShielding();
				}
			}
		}
		else
		{
			if (hazard is ObstacleMine)
			{
				PlayerStats.IncreaseStat(PlayerStats.StatNames.MinesTriped_Total, 1);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.MinesTriped_Run, 1);
			}
			if (hazardCollisionResolution == CollisionResolver.ResolutionType.SonicDeath || hazardCollisionResolution == CollisionResolver.ResolutionType.SonicDieForwards)
			{
				resolutionType = ((!isBallShown || !(null != component) || !component.isAttackableFromAir()) ? hazardCollisionResolution : CollisionResolver.ResolutionType.EnemyDeath);
			}
			if (hazardCollisionResolution == CollisionResolver.ResolutionType.SonicStumble)
			{
				resolutionType = ((!isBallShown || !(null != component) || !component.isAttackableFromAir()) ? hazardCollisionResolution : CollisionResolver.ResolutionType.EnemyDeath);
			}
		}
		if (flag)
		{
			return;
		}
		switch (resolutionType)
		{
		case CollisionResolver.ResolutionType.Nothing:
			break;
		case CollisionResolver.ResolutionType.SonicDeath:
			hazard.SendMessage("OnSonicKill", this);
			m_motionStateMachine.ForceState(m_motionStateMachine.CurrentState.OnSplat(Sonic.Handling, m_internalTracker, SonicAnimationControl.SplatType.Stationary, hazard));
			break;
		case CollisionResolver.ResolutionType.EnemyDeath:
		{
			object[] value = new object[2] { this, false };
			hazard.SendMessage("OnDeath", value);
			if (hazard is Enemy)
			{
				object[] parameters2 = ((m_motionStateMachine.CurrentState.GetType() != typeof(MotionDiveState)) ? new object[2]
				{
					hazard,
					Enemy.Kill.Rolling
				} : new object[2]
				{
					hazard,
					Enemy.Kill.Diving
				});
				EventDispatch.GenerateEvent("OnEnemyKilled", parameters2);
			}
			break;
		}
		case CollisionResolver.ResolutionType.SonicStumble:
			hazard.SendMessage("OnStumble", this);
			m_motionStateMachine.RequestState(m_motionStateMachine.CurrentState.OnStumble(base.gameObject, Sonic.Handling));
			if (hazard is Enemy)
			{
				object[] parameters = new object[2]
				{
					hazard,
					Enemy.Kill.Other
				};
				EventDispatch.GenerateEvent("OnEnemyKilled", parameters);
			}
			SetGhosted();
			EventDispatch.GenerateEvent("OnRingExplosion");
			break;
		case CollisionResolver.ResolutionType.SonicKnockedLeft:
			m_motionStateMachine.RequestState(m_motionStateMachine.CurrentState.OnKnockedSideways(m_internalTracker, m_track, SideDirection.Left, Sonic.Handling.StrafeDuration, base.gameObject));
			break;
		case CollisionResolver.ResolutionType.SonicKnockedRight:
			m_motionStateMachine.RequestState(m_motionStateMachine.CurrentState.OnKnockedSideways(m_internalTracker, m_track, SideDirection.Right, Sonic.Handling.StrafeDuration, base.gameObject));
			break;
		case CollisionResolver.ResolutionType.SonicDieForwards:
			hazard.SendMessage("OnSonicKill", this);
			m_motionStateMachine.ForceState(m_motionStateMachine.CurrentState.OnSplat(Sonic.Handling, m_internalTracker, SonicAnimationControl.SplatType.Forwards, hazard));
			break;
		}
	}

	private bool IsReadyForGesture(MotionState.GestureType gestureType)
	{
		if (base.enabled && m_motionStateMachine != null && m_motionStateMachine.HasActiveState)
		{
			BossBattleSystem bossBattleSystem = BossBattleSystem.Instance();
			if (bossBattleSystem != null && bossBattleSystem.IsEnabled())
			{
				return bossBattleSystem.IsGestureValid(gestureType);
			}
			return true;
		}
		return false;
	}

	private bool IsStrafeFlipped()
	{
		BossBattleSystem bossBattleSystem = BossBattleSystem.Instance();
		if (bossBattleSystem != null && bossBattleSystem.IsEnabled())
		{
			return bossBattleSystem.IsGestureStrafeFlipped();
		}
		return false;
	}

	public void Strafe(SideDirection direction)
	{
		if (IsReadyForGesture(MotionState.GestureType.Strafe))
		{
			if (IsStrafeFlipped())
			{
				direction = ((direction == SideDirection.Left) ? SideDirection.Right : SideDirection.Left);
			}
			MotionState newState = m_motionStateMachine.CurrentState.OnStrafe(m_internalTracker, m_track, direction, Sonic.Handling.StrafeDuration, base.gameObject);
			m_motionStateMachine.RequestState(newState);
		}
	}

	public void OnStrafe(SideDirection direction)
	{
		Sonic.fireStrafeEvent(m_internalTracker);
	}

	public void Roll()
	{
		if (!IsReadyForGesture(MotionState.GestureType.Roll))
		{
			return;
		}
		MotionState motionState = m_motionStateMachine.CurrentState.OnRoll(base.gameObject, m_physics, Sonic.Handling);
		if (motionState != null)
		{
			m_motionStateMachine.RequestState(motionState);
			if (getLane().Equals(Track.Lane.Middle))
			{
				PlayerStats.IncreaseStat(PlayerStats.StatNames.RollsMiddle_Total, 1);
			}
		}
	}

	public void Dive()
	{
		if (IsReadyForGesture(MotionState.GestureType.Dive))
		{
			MotionState motionState = m_motionStateMachine.CurrentState.OnDive(m_physics, 0f, base.gameObject, Sonic.Handling);
			m_motionStateMachine.RequestState(motionState);
			if (IsTrackerAvailable && getLane() == Track.Lane.Middle && motionState is MotionDiveState)
			{
				PlayerStats.IncreaseStat(PlayerStats.StatNames.RollsMiddle_Total, 1);
			}
		}
	}

	public void Jump()
	{
		if (IsReadyForGesture(MotionState.GestureType.Jump) && (m_internalTracker != null || IsSpringJumping))
		{
			float initialGroundHeight = ((m_internalTracker != null) ? m_internalTracker.CurrentSplineTransform.Location.y : 0f);
			m_motionStateMachine.RequestState(m_motionStateMachine.CurrentState.OnJump(m_physics, initialGroundHeight, base.gameObject, Sonic.Handling));
		}
	}

	public void Tap(float[] array)
	{
		if (IsReadyForGesture(MotionState.GestureType.Tap))
		{
			float tapX = array[0];
			float tapY = array[1];
			m_motionStateMachine.RequestState(m_motionStateMachine.CurrentState.OnAttack(m_physics, 0f, base.gameObject, Sonic.Handling, tapX, tapY));
		}
	}

	public SplineTracker CloneTracker()
	{
		return new SplineTracker(m_internalTracker);
	}

	private void SetToggleObjectsActive()
	{
		float y = m_camera.transform.position.y;
		float setPieceHeightToggleTrigger = m_track.GenerationParams.SetPieceHeightToggleTrigger;
		bool flag = y > setPieceHeightToggleTrigger;
		if (m_toggleOnAboveObject != null)
		{
			m_toggleOnAboveObject.SetActive(flag);
		}
		if (m_toggleOnAboveWater != null)
		{
			m_toggleOnAboveWater.SetActive(flag);
		}
		if (m_toggleOnBelowObject != null)
		{
			m_toggleOnBelowObject.SetActive(!flag);
		}
		if (m_toggleOnBelowWater != null)
		{
			m_toggleOnBelowWater.SetActive(!flag);
		}
		if ((bool)BehindCamera.Instance && (bool)BehindCamera.Instance.Camera)
		{
			if (flag)
			{
				BehindCamera.Instance.Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Underwater"));
			}
			else
			{
				BehindCamera.Instance.Camera.cullingMask |= 1 << LayerMask.NameToLayer("Underwater");
			}
		}
	}

	private void UpdateWorldWithDelta(Vector3 frameDelta)
	{
		base.gameObject.transform.position -= frameDelta;
		float y = m_camera.transform.position.y;
		float y2 = m_lastCameraTransform.Location.y;
		float setPieceHeightToggleTrigger = m_track.GenerationParams.SetPieceHeightToggleTrigger;
		if ((y2 < setPieceHeightToggleTrigger && y > setPieceHeightToggleTrigger) || (y2 > setPieceHeightToggleTrigger && y < setPieceHeightToggleTrigger))
		{
			SetToggleObjectsActive();
		}
		m_lastCameraTransform.Location = m_camera.transform.position;
		m_lastCameraTransform.Orientation = m_camera.transform.rotation;
		if (null == m_YOnlyRootTransform)
		{
			m_YOnlyRootTransform = FindYOnlyRootTransform();
		}
		Vector3 position = base.gameObject.transform.position;
		position.y = 0f;
		m_YOnlyRootTransform.position = position;
	}

	public IEnumerator WaitForCleanTrack()
	{
		yield return null;
		TrackSegment firstSegment = TrackSegment.GetSegmentOfSpline(m_track.StartSpline);
		float cleanDistance = 50f;
		while (!firstSegment.IsTrackReady(cleanDistance))
		{
			yield return null;
		}
		m_internalTracker = new SplineTracker();
		m_internalTracker.TrackerType = SplineTracker.Type.OneShot;
		m_internalTracker.Target = m_track.StartSpline;
		m_internalTracker.Target.OnStartTracking();
		m_internalTracker.Start(0f);
		m_internalTracker.OnStop += m_track.OnSplineStop;
		m_internalTracker.IsVIP = true;
		Vector3 trackStartPos = m_internalTracker.CurrentSplineTransform.Location;
		Vector3 trackToSonic = Sonic.Transform.position - trackStartPos;
		trackToSonic.y = 0f;
		UpdateWorldWithDelta(trackToSonic);
	}

	private void UpdateWorldContainer()
	{
		if (!WorldCollector.IsRescanRequired)
		{
			return;
		}
		foreach (GameObject item in WorldCollector.FindAllWhitelistedGameObjects())
		{
			if (item != null && item.transform.parent != s_worldTransform)
			{
				item.transform.parent = s_worldTransform;
			}
		}
	}

	private Transform CreateWorldTransform(Transform currentTransform, string name, IEnumerable<GameObject> potentialChildren)
	{
		if (currentTransform != null)
		{
			m_gameStartTransform.ApplyTo(currentTransform);
			while (currentTransform.childCount > 0)
			{
				currentTransform.GetChild(0).parent = null;
			}
		}
		if (currentTransform == null)
		{
			GameObject gameObject = new GameObject(name);
			currentTransform = gameObject.transform;
			m_gameStartTransform.ApplyTo(currentTransform);
		}
		foreach (GameObject potentialChild in potentialChildren)
		{
			potentialChild.transform.parent = currentTransform;
		}
		return currentTransform;
	}

	private void UpdateTransitionToNewSpline()
	{
		if (!m_internalTracker.Tracking && m_motionStateMachine.CurrentState.IsFlying())
		{
			SplineUtils.SplineParameters splineStartNearest = m_track.GetSplineStartNearest(base.transform.position, 3f);
			if (splineStartNearest.IsValid && splineStartNearest.Target != m_internalTracker.Target)
			{
				splineStartNearest.ApplyTo(m_internalTracker);
			}
		}
	}

	private DeathState getDeathState()
	{
		SplineKnot.EndBehaviour endBehaviourInCurrentDirection = m_internalTracker.GetEndBehaviourInCurrentDirection();
		bool flag = !m_internalTracker.Tracking && !m_motionStateMachine.CurrentState.IsFlying();
		bool flag2 = flag && endBehaviourInCurrentDirection == SplineKnot.EndBehaviour.Fall;
		bool flag3 = flag && !m_internalTracker.Tracking && endBehaviourInCurrentDirection == SplineKnot.EndBehaviour.Splat;
		DeathState result = default(DeathState);
		result.IsDead = flag2 || flag3;
		result.DeathType = endBehaviourInCurrentDirection;
		return result;
	}

	public bool IsDead()
	{
		if (m_internalTracker == null)
		{
			return false;
		}
		return getDeathState().IsDead;
	}

	private void UpdateDeath(SplineKnot.EndBehaviour deathType)
	{
		MotionState newState = ((deathType != SplineKnot.EndBehaviour.Fall) ? m_motionStateMachine.CurrentState.OnSplat(Sonic.Handling, m_internalTracker, SonicAnimationControl.SplatType.Stationary, null) : m_motionStateMachine.CurrentState.OnFall());
		m_motionStateMachine.ForceState(newState);
	}

	private void DebugVariables()
	{
	}

	private void TrackEventsPassed(float newTrackPosition)
	{
		bool flag = false;
		Track.Info.EntitiesInRange(TrackPosition, newTrackPosition, ref m_entityScratchList);
		for (int i = 0; i < m_entityScratchList.Count; i++)
		{
			TrackEntity trackEntity = m_entityScratchList[i];
			if (trackEntity.IsValid)
			{
				Type type = m_motionStateMachine.CurrentState.GetType();
				Track.Lane lane = getLane();
				if ((trackEntity.InstanceKind == TrackEntity.Kind.MedWall || trackEntity.InstanceKind == TrackEntity.Kind.TallWall) && m_motionStateMachine.CurrentState.GetType() == typeof(MotionRollState) && ((TrackGameObject)trackEntity).Object.GetComponent<Wall>().NumLanesOccupied == 3 && !flag)
				{
					PlayerStats.IncreaseStat(PlayerStats.StatNames.BridgesRolled_Total, 1);
					PlayerStats.IncreaseStat(PlayerStats.StatNames.BridgesRolled_Run, 1);
					flag = true;
				}
				else if (trackEntity.InstanceKind == TrackEntity.Kind.Spikes && ((TrackGameObject)trackEntity).Object.GetComponent<Spikes>().getLane() == lane && type == typeof(MotionJumpState))
				{
					PlayerStats.IncreaseStat(PlayerStats.StatNames.SpikysJumpedOver_Total, 1);
				}
				else if (trackEntity.InstanceKind == TrackEntity.Kind.Crabmeat && ((TrackGameObject)trackEntity).Object.GetComponent<Crabmeat>().getLane() == lane && type == typeof(MotionJumpState))
				{
					PlayerStats.IncreaseStat(PlayerStats.StatNames.CrabmeatJumpedOver_Run, 1);
				}
				else if (trackEntity.InstanceKind == TrackEntity.Kind.MedWall && trackEntity.Lane == lane && type == typeof(MotionJumpState) && ((TrackGameObject)trackEntity).Object != null)
				{
					PlayerStats.JumpOverObstacle(((TrackGameObject)trackEntity).Object);
				}
			}
		}
	}

	public bool isReadyForDash()
	{
		if (gapRespawnIsPermitted() && m_motionStateMachine.CurrentState != null)
		{
			return m_motionStateMachine.CurrentState.IsReadyForDash();
		}
		return false;
	}

	public bool isReadyForMagnet()
	{
		return true;
	}

	public bool isReadyForShield()
	{
		return true;
	}

	public Track.Lane getLane()
	{
		if (Track == null || m_internalTracker == null || m_internalTracker.Target == null)
		{
			return Track.Lane.Middle;
		}
		return Track.GetLaneOfSpline(m_internalTracker.Target);
	}

	public bool isJumping()
	{
		if (m_physics != null)
		{
			return m_physics.IsJumping;
		}
		return false;
	}

	public SonicPhysics getPhysics()
	{
		return m_physics;
	}

	public bool IsFalling()
	{
		if (m_motionStateMachine != null && m_motionStateMachine.HasActiveState)
		{
			return m_motionStateMachine.CurrentState.IsFalling();
		}
		return false;
	}

	public void ResetOnTrack()
	{
		m_overGap = false;
		m_overSmallIsland = false;
		TrackSegment trackSegmentForReset = GetTrackSegmentForReset();
		if (null != trackSegmentForReset)
		{
			Spline middleSpline = trackSegmentForReset.MiddleSpline;
			float lineDistance = middleSpline.EstimateDistanceAlongSpline(Sonic.Transform.position).LineDistance;
			m_internalTracker.Target = middleSpline;
			m_physics.HaltSonic();
			m_internalTracker.Start(0f, lineDistance, Direction_1D.Forwards);
			m_internalTracker.requestTeleport();
			m_internalTracker.ForceUpdateTransform();
			TrackPosition = Track.CalculateTrackPositionOfTracker(m_internalTracker);
			Vector3 location = m_internalTracker.CurrentSplineTransform.Location;
			Vector3 frameDelta = m_gameStartTransform.Location - location;
			UpdateWorldWithDelta(frameDelta);
		}
		Sonic.AnimationControl.OnRespawn();
		Sonic.AudioControl.PlayRespawnSFX();
		m_fader.flash(0.2f);
		EventDispatch.GenerateEvent("OnSonicRespawn");
	}

	public void Resurrect(bool freeRevive)
	{
		if (m_sonicDiedWithActiveChoppers)
		{
		}
		m_overGap = false;
		m_overSmallIsland = false;
		InternalMotionState.PopTopState();
		TrackSegment trackSegmentForRevive_Simple = GetTrackSegmentForRevive_Simple();
		if (null != trackSegmentForRevive_Simple)
		{
			Spline middleSpline = trackSegmentForRevive_Simple.MiddleSpline;
			float lineDistance = middleSpline.EstimateDistanceAlongSpline(Sonic.Transform.position).LineDistance;
			m_internalTracker.Target = middleSpline;
			m_physics.HaltSonic();
			m_internalTracker.Start(m_internalTracker.TrackSpeed, lineDistance, Direction_1D.Forwards);
			m_internalTracker.requestTeleport();
			TrackPosition = Track.CalculateTrackPositionOfTracker(m_internalTracker);
			Vector3 location = m_internalTracker.CurrentSplineTransform.Location;
			Vector3 frameDelta = Sonic.Transform.position - location;
			UpdateWorldWithDelta(frameDelta);
		}
		Sonic.AnimationControl.OnRespawn();
		Sonic.AnimationControl.TriggerRespawnPowerupEffect(freeRevive);
		Sonic.AudioControl.PlayRespawnSFX();
		ClearTrackForRespawn();
		m_fader.flash(0.2f);
		EventDispatch.GenerateEvent("OnSonicRespawn");
		EventDispatch.GenerateEvent("OnSonicResurrection");
	}

	public void FaderFlash()
	{
		m_fader.flash(0.2f);
	}

	public bool GetIsOnSetPiece()
	{
		return m_onSetPiece;
	}

	public float GetDistanceToNextGap()
	{
		return m_distanceToNextGap;
	}
}
