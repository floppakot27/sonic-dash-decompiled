using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameplayTemplate : IEnumerable, IEnumerable<GameplayTemplate.Row>
{
	public enum Group
	{
		Standard,
		Tutorial,
		FixedStart,
		EndTrackSpring,
		SetPiece,
		Corner,
		Hilly,
		HillyCorner,
		BossBattle
	}

	public enum Type
	{
		Straight,
		Gap,
		Elevation,
		Corner
	}

	public enum Lane
	{
		LeftFish,
		Left,
		Middle,
		Right,
		RightFish
	}

	[Serializable]
	public class Entity
	{
		[SerializeField]
		private int m_type;

		[SerializeField]
		private Enemy.Direction m_direction;

		[SerializeField]
		private bool m_isPossiblyMultiLane;

		public bool IsValid => (long)m_type != 0;

		public uint Type => (uint)m_type;

		public Enemy.Direction Direction => m_direction;

		public bool IsPossiblyMultiLane
		{
			get
			{
				return m_isPossiblyMultiLane;
			}
			set
			{
				m_isPossiblyMultiLane = value;
			}
		}

		public Entity()
			: this(0u)
		{
		}

		public Entity(TrackEntity.Kind kind)
			: this((uint)kind)
		{
		}

		public Entity(TrackEntity.Kind kind, Enemy.Direction dir)
			: this((uint)kind, dir)
		{
		}

		public Entity(uint type)
			: this(type, Enemy.Direction.Stationary)
		{
		}

		public Entity(uint type, Enemy.Direction dir)
		{
			m_type = (int)type;
			m_direction = dir;
			m_isPossiblyMultiLane = false;
		}

		public override string ToString()
		{
			string text = (Enum.IsDefined(typeof(TrackEntity.Kind), m_type) ? ((TrackEntity.Kind)m_type).ToString() : ((Type == 3) ? "GroundEnemy" : ((Type == 66) ? "SpinThrough" : ((Type != 539) ? "INVALID" : "JumpOver"))));
			string text2 = ((m_direction != Enemy.Direction.Stationary) ? (":" + m_direction) : string.Empty);
			return text + text2;
		}
	}

	[Serializable]
	public class Cell
	{
		[SerializeField]
		private Entity m_lowEntity;

		[SerializeField]
		private Entity m_highEntity;

		[SerializeField]
		private bool m_isGapEnd;

		public bool HasEntities => LowEntity.IsValid || HighEntity.IsValid;

		public bool IsGapEnd
		{
			get
			{
				return m_isGapEnd;
			}
			set
			{
				m_isGapEnd = value;
			}
		}

		public Entity LowEntity => m_lowEntity;

		public Entity HighEntity => m_highEntity;

		public Cell()
			: this(new Entity())
		{
		}

		public Cell(Entity lowEntity)
			: this(lowEntity, new Entity())
		{
		}

		public Cell(Entity lowEntity, Entity highEntity)
		{
			m_lowEntity = lowEntity;
			m_highEntity = highEntity;
			m_isGapEnd = false;
		}

		public override string ToString()
		{
			return string.Concat("cell low:", LowEntity, ", high:", HighEntity);
		}
	}

	[Serializable]
	public class Row : IEnumerable<Cell>, IEnumerable
	{
		public enum GapRowType
		{
			None,
			Jumpable,
			Chopper
		}

		[SerializeField]
		private Cell[] m_cells = new Cell[Enum.GetValues(typeof(Lane)).Length];

		[SerializeField]
		private GapRowType m_gapRowType;

		public bool IsFullGap => IsGapInLane(Lane.Left) && IsGapInLane(Lane.Middle) && IsGapInLane(Lane.Right);

		public bool IsEmptyAir => m_gapRowType != GapRowType.None;

		public GapRowType GapRow
		{
			get
			{
				return m_gapRowType;
			}
			set
			{
				m_gapRowType = value;
			}
		}

		public bool IsEventRow => IsEmptyAir || IsFullGap;

		public Cell this[Lane lane] => m_cells[(int)lane];

		public IEnumerable<Cell> CharacterLanesOnly => this.Skip(1).Take(3);

		public Row(GapRowType gapType)
		{
			m_gapRowType = gapType;
		}

		public Row(Cell leftFishCell, Cell leftLaneCell, Cell middleLaneCell, Cell rightLaneCell, Cell rightFishCell)
		{
			m_gapRowType = GapRowType.None;
			m_cells[0] = AddDirectionToFish(leftFishCell, Lane.LeftFish);
			m_cells[1] = leftLaneCell;
			m_cells[2] = middleLaneCell;
			m_cells[3] = rightLaneCell;
			m_cells[4] = AddDirectionToFish(rightFishCell, Lane.RightFish);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_cells.GetEnumerator();
		}

		public void Validate(string templateName, int rowIndex)
		{
			bool flag = IsGapCell(m_cells[2]);
			bool flag2 = IsGapCell(m_cells[1]);
			bool flag3 = IsGapCell(m_cells[3]);
			bool flag4 = flag && flag2 && flag3;
		}

		public bool IsGapInLane(Lane lane)
		{
			return IsGapCell(this[lane]);
		}

		public void SetGapType(GapRowType newType)
		{
			m_gapRowType = newType;
		}

		public bool LaneHasEntities(Lane lane)
		{
			return m_cells[(int)lane] != null && m_cells[(int)lane].HasEntities;
		}

		public IEnumerator<Cell> GetEnumerator()
		{
			return ((IEnumerable<Cell>)m_cells).GetEnumerator();
		}
	}

	public const uint JumpOverEntities = 539u;

	public const uint RollThroughEntities = 66u;

	public static readonly int LaneCount = Utils.GetEnumCount<Lane>();

	public static Lane[] TrackLanes = new Lane[3]
	{
		Lane.Left,
		Lane.Middle,
		Lane.Right
	};

	[SerializeField]
	private string m_name;

	[SerializeField]
	private float m_minimumDifficultyInc;

	[SerializeField]
	private float m_maximumDifficultyExc;

	[SerializeField]
	private int m_length;

	[SerializeField]
	private List<Row> m_rows = new List<Row>();

	[SerializeField]
	private List<Group> m_groupIDs;

	[SerializeField]
	private int m_springCount;

	private bool m_hasGaps;

	public bool HasGaps => m_hasGaps;

	public IEnumerable<Group> GroupIDs => m_groupIDs;

	public string Name
	{
		get
		{
			return m_name;
		}
		private set
		{
			m_name = value;
		}
	}

	public int Length
	{
		get
		{
			return m_length;
		}
		private set
		{
			m_length = value;
		}
	}

	public bool HasFishLanes => this.Any((Row row) => row.LaneHasEntities(Lane.LeftFish) || row.LaneHasEntities(Lane.RightFish));

	public bool IsOnlyEnemies
	{
		get
		{
			using (IEnumerator<Row> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Row current = enumerator.Current;
					foreach (Cell item in current)
					{
						if (!CellIsEmptyOrEnemies(item))
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	public bool HasAtLeastOneSpring
	{
		get
		{
			using (IEnumerator<Row> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Row current = enumerator.Current;
					foreach (Cell item in current)
					{
						if (item == null || item.LowEntity == null || item.LowEntity.Type != 8192)
						{
							continue;
						}
						return true;
					}
				}
			}
			return false;
		}
	}

	public int SpringCount => m_springCount;

	public int RowCount => m_rows.Count;

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_rows.GetEnumerator();
	}

	public bool ContainsGroup(Group gToFind)
	{
		for (int i = 0; i < m_groupIDs.Count; i++)
		{
			if (m_groupIDs[i] == gToFind)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsDifficultyMatch(float difficulty)
	{
		return difficulty >= m_minimumDifficultyInc && difficulty < m_maximumDifficultyExc;
	}

	public string CalculateDifficultyString()
	{
		string text = ((m_minimumDifficultyInc != float.MinValue) ? Mathf.CeilToInt(m_minimumDifficultyInc).ToString() : "*");
		string text2 = ((m_maximumDifficultyExc != float.MaxValue) ? Mathf.FloorToInt(m_maximumDifficultyExc).ToString() : "*");
		return (m_maximumDifficultyExc - m_minimumDifficultyInc <= 1f) ? Mathf.FloorToInt(m_maximumDifficultyExc).ToString() : ((!(text == text2)) ? (text + ".." + text2) : text);
	}

	public string CalculateGroupsString()
	{
		return string.Join("|", GroupIDs.Select((Group g) => g.ToString()).ToArray());
	}

	public override string ToString()
	{
		string text = CalculateDifficultyString();
		string text2 = CalculateGroupsString();
		return "Template \"" + Name + "\", diff " + text + ", group " + text2;
	}

	private void calculateSpringCount()
	{
		m_springCount = 0;
		for (int i = 0; i < RowCount; i++)
		{
			Row row = GetRow(i);
			for (int j = 0; j < LaneCount; j++)
			{
				Cell cell = row[(Lane)j];
				if (cell != null && cell.LowEntity != null && cell.LowEntity.Type == 8192)
				{
					m_springCount++;
				}
			}
		}
	}

	public static bool CellIsEmptyOrEnemies(Cell c)
	{
		return c == null || (EntityIsNullOrEnemy(c.LowEntity) && EntityIsNullOrEnemy(c.HighEntity));
	}

	public static bool EntityIsNullOrEnemy(Entity e)
	{
		return e == null || !e.IsValid || ContainsOnlyEnemies(e.Type);
	}

	public static bool ContainsOnlyEnemies(uint entityType)
	{
		return entityType != 0 && (entityType & 0xFFFFFFF8u) == 0;
	}

	public static bool IsSpecificGroundEnemy(uint entityType)
	{
		return TrackEntity.IsConcreteOfType(entityType, 3u);
	}

	public static bool IsSatisfiedByGroundEnemy(uint entityType)
	{
		return (entityType & 3) != 0;
	}

	public static bool IsSpecificObstacle(uint entityType)
	{
		return TrackEntity.IsConcreteOfType(entityType, 760u);
	}

	public static bool IsSatisfiedByObstacle(uint entityType)
	{
		return (entityType & 0x2F8) != 0;
	}

	public static TrackEntity.Kind PickRandomGroundEnemy(System.Random rng)
	{
		return (rng.NextDouble() < 0.5) ? TrackEntity.Kind.Spikes : TrackEntity.Kind.Crabmeat;
	}

	public static TrackEntity.Kind PickSuitableObstacle(uint requestedGameplay, System.Random rng)
	{
		uint num = requestedGameplay & 0x2F8u;
		int bitCount = Utils.GetBitCount(num);
		int num2 = rng.Next(bitCount);
		int num3 = num2;
		foreach (int value in Enum.GetValues(typeof(TrackEntity.Kind)))
		{
			if (((uint)value & num) != 0)
			{
				if (num3 == 0)
				{
					return (TrackEntity.Kind)value;
				}
				num3--;
			}
		}
		return TrackEntity.Kind.Invalid;
	}

	public static Track.Lane ToTrackLane(Lane lane)
	{
		return lane switch
		{
			Lane.Left => Track.Lane.Left, 
			Lane.Right => Track.Lane.Right, 
			_ => Track.Lane.Middle, 
		};
	}

	public static bool IsGapCell(Cell c)
	{
		return c != null && c.LowEntity != null && c.LowEntity.IsValid && c.LowEntity.Type == 4096;
	}

	public Entity FirstEntityOfType(Cell c, uint typeMask)
	{
		return (c == null) ? null : ((c.LowEntity != null && c.LowEntity.IsValid && (c.LowEntity.Type & typeMask) != 0) ? c.LowEntity : ((c.HighEntity == null || !c.HighEntity.IsValid || (c.HighEntity.Type & typeMask) == 0) ? null : c.HighEntity));
	}

	public bool CalculateHasGaps()
	{
		using (IEnumerator<Row> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Row current = enumerator.Current;
				foreach (Cell item in current)
				{
					if (IsGapCell(item))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void OnEnable()
	{
		m_hasGaps = CalculateHasGaps();
	}

	private static Cell AddDirectionToFish(Cell cellIn, Lane fishLane)
	{
		if (cellIn == null || !cellIn.HasEntities)
		{
			return null;
		}
		Enemy.Direction dir = ((fishLane == Lane.LeftFish) ? Enemy.Direction.ToPlayersRight : Enemy.Direction.ToPlayersLeft);
		return new Cell(new Entity(4u, dir));
	}

	public IEnumerator<Row> GetEnumerator()
	{
		return m_rows.GetEnumerator();
	}

	public Row GetRow(int rowIndex)
	{
		return m_rows[rowIndex];
	}

	private void Validate(int rowIndex, GameplayTemplateParameters parameters)
	{
		Row row = m_rows[rowIndex];
		row.Validate(Name, rowIndex);
		if (row.IsFullGap)
		{
			ValidateIsStartOrEndGap(rowIndex);
		}
		else if (row.IsGapInLane(Lane.Left))
		{
			ValidateGapLaneLength(rowIndex, Lane.Left, parameters);
		}
		else if (row.IsGapInLane(Lane.Right))
		{
			ValidateGapLaneLength(rowIndex, Lane.Right, parameters);
		}
	}

	private void ValidateIsStartOrEndGap(int rowIndex)
	{
		bool flag = rowIndex > 0 && (m_rows[rowIndex - 1].IsFullGap || m_rows[rowIndex - 1].IsEmptyAir);
		bool flag2 = rowIndex < m_rows.Count - 1 && (m_rows[rowIndex + 1].IsFullGap || m_rows[rowIndex + 1].IsEmptyAir);
	}

	private void ValidateGapLaneLength(int rowIndex, Lane gapLane, GameplayTemplateParameters parameters)
	{
		if (rowIndex <= 0 || !m_rows[rowIndex - 1].IsGapInLane(gapLane))
		{
			int num = parameters.MinTemplateRowCount * 2;
			int minTemplateRowCount = parameters.MinTemplateRowCount;
			for (int i = 1; rowIndex + i < m_rows.Count && m_rows[rowIndex + i].IsGapInLane(gapLane); i++)
			{
			}
		}
	}

	private void CacheMultiLaneObstacles()
	{
		using IEnumerator<Row> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Row current = enumerator.Current;
			Entity left = FirstEntityOfType(current[Lane.Left], 240u);
			Entity middle = FirstEntityOfType(current[Lane.Middle], 240u);
			Entity right = FirstEntityOfType(current[Lane.Right], 240u);
			CalculateIsMultiLanePossible(left, middle, right);
		}
	}

	private void CalculateIsMultiLanePossible(Entity left, Entity middle, Entity right)
	{
		if (left != null && left.IsValid && middle != null && middle.IsValid && right != null && right.IsValid && left.Type == middle.Type && right.Type == middle.Type)
		{
			bool flag2 = (right.IsPossiblyMultiLane = true);
			flag2 = (left.IsPossiblyMultiLane = flag2);
			middle.IsPossiblyMultiLane = flag2;
		}
	}

	private void AddEmptyAirRows()
	{
		bool flag = false;
		int num = 0;
		for (int i = 0; i < m_rows.Count; i++)
		{
			if (m_rows[i].IsFullGap)
			{
				flag = !flag;
				num = 0;
			}
			else if (flag)
			{
				num++;
				m_rows[i].GapRow = Row.GapRowType.Chopper;
			}
		}
		for (int j = 0; j < m_rows.Count; j++)
		{
			if (m_rows[j].IsFullGap)
			{
				if (m_rows[j + 1].GapRow != Row.GapRowType.Chopper)
				{
					InsertEmptyAirRow(j + 1);
				}
				j += 2;
			}
		}
	}

	private void MarkLaneGapEnds(GameplayTemplateParameters parameters)
	{
		int minTemplateRowCount = parameters.MinTemplateRowCount;
		int currentEndGapCount = minTemplateRowCount;
		int currentEndGapCount2 = minTemplateRowCount;
		for (int num = m_rows.Count - 1; num >= 0; num--)
		{
			Row row = m_rows[num];
			currentEndGapCount = UpdateEndGapCount(row, Lane.Left, currentEndGapCount, minTemplateRowCount);
			currentEndGapCount2 = UpdateEndGapCount(row, Lane.Right, currentEndGapCount2, minTemplateRowCount);
		}
	}

	private int UpdateEndGapCount(Row row, Lane gapLane, int currentEndGapCount, int endGapCount)
	{
		if (row.IsGapInLane(gapLane))
		{
			if (currentEndGapCount > 0)
			{
				row[gapLane].IsGapEnd = true;
				currentEndGapCount--;
			}
		}
		else
		{
			currentEndGapCount = endGapCount;
		}
		return currentEndGapCount;
	}

	private void InsertEmptyAirRow(int atRowIndex)
	{
		Row item = new Row(Row.GapRowType.Jumpable);
		m_rows.Insert(atRowIndex, item);
	}
}
