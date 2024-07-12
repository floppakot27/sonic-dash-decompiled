using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dash/Track/Track Database")]
public class TrackDatabase : MonoBehaviour
{
	public enum SubzoneCapability
	{
		Elevations = 1,
		Gaps = 2,
		SBends = 4,
		SetPieces = 8,
		OnTrackScenics = 0x10,
		GameStart = 0x20
	}

	[Serializable]
	public class SubzoneInfo
	{
		public string Name = string.Empty;

		public int Capabilities;

		public bool Supports(SubzoneCapability capability)
		{
			return (int)((uint)Capabilities & (uint)capability) > 0;
		}

		public bool Supports(PieceType piece, ElevationType elevation)
		{
			return (piece != PieceType.SBend || Supports(SubzoneCapability.SBends)) && (!IsSetPiece(piece) || Supports(SubzoneCapability.SetPieces)) && (!IsGapType(piece) || Supports(SubzoneCapability.Gaps)) && (elevation == ElevationType.Low || Supports(SubzoneCapability.Elevations)) && (piece != PieceType.GameStart || Supports(SubzoneCapability.GameStart));
		}
	}

	[Serializable]
	public class ZoneInfo
	{
		[SerializeField]
		private string m_zoneFolderName = string.Empty;

		[SerializeField]
		private string m_zoneFilePrefix = string.Empty;

		[SerializeField]
		private List<SubzoneInfo> m_subzones = new List<SubzoneInfo>();

		public string ZoneFolderName
		{
			get
			{
				return m_zoneFolderName;
			}
			set
			{
				m_zoneFolderName = value;
			}
		}

		public string ZoneFilePrefix
		{
			get
			{
				return m_zoneFilePrefix;
			}
			set
			{
				m_zoneFilePrefix = value;
			}
		}

		public List<SubzoneInfo> Subzones => m_subzones;

		public SubzoneInfo this[int subzoneIndex] => m_subzones[subzoneIndex];
	}

	public enum PieceType
	{
		Standard,
		Left,
		Right,
		GapLeft,
		GapLeftStart,
		GapLeftEnd,
		GapRight,
		GapRightStart,
		GapRightEnd,
		SBend,
		TrackEnd,
		TrackStart,
		EmptyAir,
		GameStart,
		SetPieceLoop,
		SetPieceCorkscrew,
		SetPieceBend
	}

	public enum ElevationType
	{
		Low,
		RampUp,
		High,
		RampDown
	}

	[Serializable]
	public class PieceInfo
	{
		public string Name;

		public PieceType Type;

		public ElevationType Elevation;

		public int VariationCount;

		public PieceInfo()
		{
			Name = string.Empty;
			Type = PieceType.Standard;
			Elevation = ElevationType.Low;
			VariationCount = 1;
		}

		public override string ToString()
		{
			return Name + " (" + Enum.GetNames(typeof(PieceType))[(int)Type] + ")";
		}
	}

	[Serializable]
	public class TrackPiece
	{
		public string FullPath;

		public ZoneInfo Zone;

		public int SubzoneIndex = -1;

		public PieceInfo PieceType;

		public bool IsUseAnywhere;

		public TrackPiece()
		{
			PieceType = new PieceInfo();
			IsUseAnywhere = true;
		}

		public TrackPiece(TrackPiece trackPiece)
		{
			FullPath = trackPiece.FullPath;
			Zone = trackPiece.Zone;
			SubzoneIndex = trackPiece.SubzoneIndex;
			PieceType = trackPiece.PieceType;
			IsUseAnywhere = trackPiece.IsUseAnywhere;
		}

		public TrackPiece(string fullPath, ZoneInfo zone, int subzoneIndex, PieceInfo pieceType, bool useAnywhere)
		{
			Set(fullPath, zone, subzoneIndex, pieceType, useAnywhere);
		}

		public void Set(string fullPath, ZoneInfo zone, int subzoneIndex, PieceInfo pieceType, bool useAnywhere)
		{
			FullPath = fullPath;
			Zone = zone;
			SubzoneIndex = subzoneIndex;
			PieceType = pieceType;
			IsUseAnywhere = useAnywhere;
		}

		public bool Supports(SubzoneCapability capability)
		{
			return (capability != SubzoneCapability.OnTrackScenics || !IsSetPiece(PieceType.Type)) && (capability != SubzoneCapability.OnTrackScenics || !IsGapType(PieceType.Type)) && Zone[SubzoneIndex].Supports(capability);
		}

		public override string ToString()
		{
			string text = ((!IsUseAnywhere) ? (" in zone " + Zone.ZoneFilePrefix + " (subzone " + Zone[SubzoneIndex].Name + ")") : string.Empty);
			return PieceType.ToString() + text + " Prefab:" + PieceType.Name;
		}

		public bool IsDifficultyRelevantPiece()
		{
			return !IsSetPiece(PieceType.Type) && PieceType.Type != TrackDatabase.PieceType.EmptyAir;
		}
	}

	public class TrackPiecePrefab : TrackPiece
	{
		private UnityEngine.Object m_object;

		public UnityEngine.Object Object
		{
			get
			{
				return m_object;
			}
			set
			{
				m_object = value;
			}
		}

		public TrackPiecePrefab(TrackPiece piece)
			: base(piece)
		{
			Object = Resources.Load(piece.FullPath, typeof(UnityEngine.Object));
		}
	}

	[SerializeField]
	private SpawnPool m_pool;

	private Transform m_despawnedPiecesContainer;

	[SerializeField]
	private List<ZoneInfo> m_zones;

	[SerializeField]
	private List<PieceInfo> m_pieces;

	[SerializeField]
	private string m_prefabsRootDir = "Dash Assets/GameplayAssets/TrackPiecePrefab";

	[SerializeField]
	private List<TrackPiece> m_trackPieces = new List<TrackPiece>();

	[SerializeField]
	private TrackPiece m_subzoneTransitionPiece;

	private TrackPiecePrefab m_subzoneTransitionPrefab;

	private int m_subzoneIndex = -1;

	private List<TrackPiecePrefab> m_subzonePrefabs;

	public IEnumerable<ZoneInfo> Zones => m_zones;

	public TrackPiecePrefab SubzoneTransitionPrefab => m_subzoneTransitionPrefab;

	public IEnumerable<TrackPiece> TrackPieces => m_trackPieces;

	public int SubzoneIndex => m_subzoneIndex;

	public List<TrackPiecePrefab> SubzonePrefabs => m_subzonePrefabs;

	public void Awake()
	{
		EnsureInitialised();
	}

	public static bool IsSetPiece(PieceType t)
	{
		return t == PieceType.SetPieceCorkscrew || t == PieceType.SetPieceLoop || t == PieceType.SetPieceBend;
	}

	public static bool IsGapType(PieceType t)
	{
		return t == PieceType.GapLeft || t == PieceType.GapLeftStart || t == PieceType.GapLeftEnd || t == PieceType.GapRight || t == PieceType.GapRightStart || t == PieceType.GapRightEnd;
	}

	public static Track.Lane GetGapLane(PieceType t)
	{
		return (t != PieceType.GapLeft && t != PieceType.GapLeftStart && t != PieceType.GapLeftEnd) ? Track.Lane.Right : Track.Lane.Left;
	}

	public static bool IsGapInLane(PieceType t, Track.Lane lane)
	{
		return t == PieceType.EmptyAir || (IsGapType(t) && GetGapLane(t) == lane);
	}

	public static bool IsTurn(PieceType t)
	{
		return t == PieceType.Left || t == PieceType.Right;
	}

	public GameObject MakeTrackPiece(GameObject piecePrefab)
	{
		Transform transform = m_pool.Spawn(piecePrefab.transform);
		return transform.gameObject;
	}

	public void KillTrackPiece(TrackSegment spawnedPiece)
	{
		spawnedPiece.transform.parent = m_despawnedPiecesContainer;
		m_pool.Despawn(spawnedPiece.transform);
	}

	public void KillAllWorldPieces()
	{
		foreach (Transform item in m_pool)
		{
			item.transform.parent = m_despawnedPiecesContainer;
		}
		m_pool.DespawnAll();
	}

	public void ClearPooledTrackPieces()
	{
		m_pool.CullAll();
	}

	public void Clear()
	{
		KillAllWorldPieces();
		ClearPooledTrackPieces();
		m_pool.Clear();
		for (int i = 0; i < m_subzonePrefabs.Count; i++)
		{
			m_subzonePrefabs[i].Object = null;
		}
		m_subzonePrefabs.Clear();
		m_subzonePrefabs = new List<TrackPiecePrefab>(m_pieces.Count / 2);
		m_subzoneIndex = -1;
	}

	public IEnumerator LoadPrefabs(int subzoneIndex)
	{
		if (m_subzoneIndex == subzoneIndex)
		{
			yield break;
		}
		Clear();
		m_subzoneIndex = subzoneIndex;
		for (int i = 0; i < m_trackPieces.Count; i++)
		{
			TrackPiece trackPiece = m_trackPieces[i];
			if (trackPiece.SubzoneIndex == m_subzoneIndex)
			{
				m_subzonePrefabs.Add(new TrackPiecePrefab(trackPiece));
				yield return null;
			}
		}
	}

	private void Start()
	{
		PreprocessPieces();
	}

	private void EnsureInitialised()
	{
		if (m_zones == null)
		{
			m_zones = new List<ZoneInfo>();
		}
		if (m_pieces == null)
		{
			m_pieces = new List<PieceInfo>();
		}
		if (m_pool == null)
		{
			if (PoolManager.Pools.ContainsKey("Track Pieces"))
			{
				m_pool = PoolManager.Pools["Track Pieces"];
			}
			else
			{
				m_pool = PoolManager.Pools.Create("Track Pieces", base.gameObject);
				m_pool.poolName = "Track Pieces";
				m_pool.forceDestroyOnDespawn = true;
			}
		}
		if (m_despawnedPiecesContainer == null && Application.isPlaying)
		{
			GameObject gameObject = new GameObject("despawned pieces");
			m_despawnedPiecesContainer = gameObject.transform;
			m_despawnedPiecesContainer.parent = base.transform;
		}
	}

	private void PreprocessPieces()
	{
		m_subzonePrefabs = new List<TrackPiecePrefab>(m_pieces.Count / 2);
		m_subzoneTransitionPrefab = new TrackPiecePrefab(m_subzoneTransitionPiece);
	}
}
