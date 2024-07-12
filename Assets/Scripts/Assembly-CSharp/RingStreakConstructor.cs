using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RingStreakConstructor
{
	public struct RingPlacement
	{
		private WorldTransformLock m_floorTransformLock;

		public LightweightTransform FloorTransform => m_floorTransformLock.CurrentTransform;

		public GameObject Parent => m_floorTransformLock.World.gameObject;

		public float AdditionalHeight { get; private set; }

		public uint Substreaks { get; set; }

		public float TrackPosition { get; private set; }

		public Track.Lane Lane { get; private set; }

		public RingPlacement(SplineTracker tracker, float additionalHeight, uint substreaks, float trackPosition, Track.Lane lane)
			: this(tracker.CurrentSplineTransform, tracker.Target.gameObject, additionalHeight, substreaks, trackPosition, lane)
		{
		}

		public RingPlacement(LightweightTransform floorTransform, GameObject parent, float additionalHeight, uint substreaks, float trackPosition, Track.Lane lane)
			: this(new WorldTransformLock(parent, floorTransform), additionalHeight, substreaks, trackPosition, lane)
		{
		}

		public RingPlacement(WorldTransformLock floorTransform, float additionalHeight, uint substreaks, float trackPosition, Track.Lane lane)
		{
			m_floorTransformLock = floorTransform;
			AdditionalHeight = additionalHeight;
			Substreaks = substreaks;
			TrackPosition = trackPosition;
			Lane = lane;
		}
	}

	private class RingGroup
	{
		private List<SplineTracker> m_rowKeyframes = new List<SplineTracker>(15);

		private Dictionary<int, Track.Lane> m_gapRows = new Dictionary<int, Track.Lane>();

		private List<List<Node>> m_rows = new List<List<Node>>(15);

		private bool m_isOpen = true;

		private int m_postGroupEmptyRows;

		public bool IsOpen => m_isOpen;

		public int RowCount => m_rows.Count;

		public int SuffixEmptyRowCount => m_postGroupEmptyRows;

		public RingGroup(bool[,] openRow, Track.Lane? missingLane, SplineTracker initialSplineKeyframe)
		{
			List<Node> nodesFromOpenRow = GetNodesFromOpenRow(openRow);
			m_rows.Add(nodesFromOpenRow);
			m_rowKeyframes.Add(initialSplineKeyframe);
			if (missingLane.HasValue)
			{
				m_gapRows.Add(0, missingLane.Value);
			}
		}

		public void AddEmptySuffixRow()
		{
			m_isOpen = false;
			m_postGroupEmptyRows++;
		}

		public IEnumerable<Node> StartLanes(int rowIndex)
		{
			return m_rows[rowIndex];
		}

		public void AddOpenRowToGroup(bool[,] openRow, Track.Lane? missingLane, SplineTracker rowKeyframe)
		{
			List<Node> nodesFromOpenRow = GetNodesFromOpenRow(openRow);
			List<Node> list = m_rows.Last();
			Track.Lane? previousMissingLane = ((!m_gapRows.ContainsKey(m_rows.Count - 1)) ? null : new Track.Lane?(m_gapRows[m_rows.Count - 1]));
			foreach (Node item in list)
			{
				List<Node> list2 = new List<Node>();
				int num = -1;
				foreach (Node item2 in nodesFromOpenRow)
				{
					int num2 = CalculateTransitionScore(item, previousMissingLane, item2);
					if (num2 != int.MaxValue)
					{
						if (num == -1 || num > num2)
						{
							list2.Clear();
							list2.Add(item2);
							num = num2;
						}
						else if (num == num2)
						{
							list2.Add(item2);
						}
					}
				}
				item.AddChildren(list2);
			}
			if (missingLane.HasValue)
			{
				m_gapRows[m_rows.Count] = missingLane.Value;
			}
			m_rows.Add(nodesFromOpenRow);
			m_rowKeyframes.Add(rowKeyframe);
		}

		public SplineTracker GetRowKeyframe(int rowIndex)
		{
			m_rowKeyframes[rowIndex].ForceUpdateTransform();
			return m_rowKeyframes[rowIndex];
		}

		public void ApplyJumpCurves(JumpDistanceCurve jumpCurve, float rowSeperation)
		{
			float heightProportionScalar = 1f / jumpCurve.JumpTimeCurve.JumpHeight;
			IEnumerable<Node> source = m_rows.SelectMany((List<Node> row) => row);
			IEnumerable<Node> enumerable = source.Where((Node node) => node.IsHigh && !node.IsOrphan && node.Parents.All((Node parentNode) => !parentNode.IsHigh));
			foreach (Node item in enumerable)
			{
				item.WalkJumpCurveBackwards(jumpCurve, jumpCurve.TotalDistance * 0.5f, heightProportionScalar, rowSeperation);
			}
			IEnumerable<Node> enumerable2 = source.Where((Node node) => node.IsHigh && node.Children.Any() && node.Children.All((Node childNode) => !childNode.IsHigh));
			foreach (Node item2 in enumerable2)
			{
				item2.WalkJumpCurveForwards(jumpCurve, jumpCurve.TotalDistance * 0.5f, heightProportionScalar, rowSeperation);
			}
		}

		public void CalculateSubstreaks()
		{
			int num = 0;
			foreach (List<Node> row in m_rows)
			{
				foreach (Node item in row)
				{
					if (item.IsOrphan)
					{
						int num2 = item.WalkSubstreakNetwork(num);
						num += num2;
					}
				}
			}
		}

		private int CalculateTransitionScore(Node fromNode, Track.Lane? previousMissingLane, Node toNode)
		{
			if (previousMissingLane.HasValue && toNode.Lane == previousMissingLane.GetValueOrDefault() && previousMissingLane.HasValue)
			{
				return int.MaxValue;
			}
			int num = Mathf.Abs(fromNode.Lane - toNode.Lane);
			bool flag = fromNode.IsHigh == toNode.IsHigh;
			if (num > 1 || (num != 0 && !flag))
			{
				return int.MaxValue;
			}
			if (num == 0)
			{
				return (!flag) ? 1 : 0;
			}
			return num * 2;
		}

		private List<Node> GetNodesFromOpenRow(bool[,] openRow)
		{
			List<Node> list = new List<Node>();
			for (int i = 0; i < Track.LaneCount; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					if (openRow[i, j])
					{
						list.Add(new Node((Track.Lane)i, j));
					}
				}
			}
			return list;
		}
	}

	private class Node
	{
		private List<Node> m_parents = new List<Node>(3);

		private List<Node> m_children = new List<Node>(3);

		private uint m_substreakMask;

		public Track.Lane Lane { get; private set; }

		public float Height { get; private set; }

		public bool IsHigh => Height == 1f;

		public IEnumerable<Node> Children => m_children;

		public IEnumerable<Node> Parents => m_parents;

		public bool HasAnyChildren => m_children.Any();

		public uint Substreaks => m_substreakMask;

		public bool IsOrphan => !m_parents.Any();

		public Node(Track.Lane lane, int height)
		{
			Lane = lane;
			Height = height;
			m_substreakMask = 0u;
		}

		public void AddChildren(IEnumerable<Node> newChildren)
		{
			m_children.AddRange(newChildren);
			foreach (Node newChild in newChildren)
			{
				newChild.m_parents.Add(this);
			}
		}

		public int WalkSubstreakNetwork(int initialSubstreakID)
		{
			if (m_children.Count == 0)
			{
				AddSubstreak(initialSubstreakID);
				return 1;
			}
			int num = initialSubstreakID;
			foreach (Node child in m_children)
			{
				int num2 = child.WalkSubstreakNetwork(num);
				num += num2;
			}
			for (int i = initialSubstreakID; i < num; i++)
			{
				AddSubstreak(i);
			}
			return num - initialSubstreakID;
		}

		public void WalkJumpCurveBackwards(JumpDistanceCurve jumpCurve, float jumpPos, float heightProportionScalar, float rowSeperation)
		{
			Height = jumpCurve.CalculateHeight(jumpPos) * heightProportionScalar;
			float num = jumpPos - rowSeperation;
			if (num < 0f)
			{
				return;
			}
			foreach (Node parent in Parents)
			{
				parent.WalkJumpCurveBackwards(jumpCurve, num, heightProportionScalar, rowSeperation);
			}
		}

		public void WalkJumpCurveForwards(JumpDistanceCurve jumpCurve, float jumpPos, float heightProportionScalar, float rowSeperation)
		{
			Height = jumpCurve.CalculateHeight(jumpPos) * heightProportionScalar;
			float num = jumpPos + rowSeperation;
			if (num > jumpCurve.TotalDistance)
			{
				return;
			}
			foreach (Node child in Children)
			{
				child.WalkJumpCurveForwards(jumpCurve, num, heightProportionScalar, rowSeperation);
			}
		}

		private void AddSubstreak(int substreakID)
		{
			m_substreakMask |= (uint)(1 << substreakID);
		}
	}

	private class Edge
	{
		public Node Start { get; private set; }

		public Node End { get; private set; }

		public Edge(Node start, Node end)
		{
			Start = start;
			End = end;
		}
	}

	private List<RingGroup> m_ringGroups = new List<RingGroup>();

	private bool[,] m_openRow;

	private Track.Lane? m_openRowMissingLane;

	private bool m_openRowHasAnyRings;

	private SplineTracker m_openRowKeyframe;

	private Track m_track;

	private int m_prefixEmptyRows;

	public RingStreakConstructor(Track track)
	{
		m_track = track;
	}

	public void BeginRow(Track.Lane? missingRow, SplineTracker rowKeyframe)
	{
		if (m_openRow != null)
		{
			ProcessOpenRow();
		}
		m_openRowHasAnyRings = false;
		m_openRow = new bool[Enum.GetValues(typeof(Track.Lane)).Length, 2];
		m_openRowMissingLane = missingRow;
		m_openRowKeyframe = rowKeyframe.Clone();
	}

	public void RegisterLowRing(Track.Lane lane)
	{
		m_openRow[(int)lane, 0] = true;
		m_openRowHasAnyRings = true;
	}

	public void RegisterHighRing(Track.Lane lane)
	{
		m_openRow[(int)lane, 1] = true;
		m_openRowHasAnyRings = true;
	}

	public IEnumerable<RingPlacement> RingPlacements(float rowSeperation, JumpDistanceCurve jumpDistanceCurve, int ringSkipCellCount)
	{
		if (m_openRow != null)
		{
			ProcessOpenRow();
		}
		foreach (RingGroup ringGroup in m_ringGroups)
		{
			ringGroup.CalculateSubstreaks();
			ringGroup.ApplyJumpCurves(jumpDistanceCurve, rowSeperation);
			int renderFlag = 0;
			for (int rowIndex = 0; rowIndex < ringGroup.RowCount; rowIndex++)
			{
				if (renderFlag == 0)
				{
					IEnumerable<RingPlacement> startRingPlacements = PlaceRingsOnLanes(middleSplineTracker: new SplineTracker(ringGroup.GetRowKeyframe(rowIndex)), laneNodes: ringGroup.StartLanes(rowIndex), jumpHeight: jumpDistanceCurve.JumpTimeCurve.JumpHeight);
					foreach (RingPlacement item in startRingPlacements)
					{
						yield return item;
					}
				}
				renderFlag++;
				renderFlag %= ringSkipCellCount + 1;
			}
		}
	}

	private bool IsTrackerAtMarker(SplineTracker tracker, SplineUtils.SplineParameters marker)
	{
		return tracker.Target == marker.Target && tracker.CurrentDistance >= marker.StartPosition;
	}

	private IEnumerable<RingPlacement> PlaceRingsOnLanes(IEnumerable<Node> laneNodes, SplineTracker middleSplineTracker, float jumpHeight)
	{
		WorldTransformLock[] laneTransforms = new WorldTransformLock[Enum.GetValues(typeof(Track.Lane)).Length];
		ref WorldTransformLock reference = ref laneTransforms[0];
		reference = GetTransformOnLane(middleSplineTracker, Track.Lane.Left);
		ref WorldTransformLock reference2 = ref laneTransforms[1];
		reference2 = GetTransformOnLane(middleSplineTracker, Track.Lane.Middle);
		ref WorldTransformLock reference3 = ref laneTransforms[2];
		reference3 = GetTransformOnLane(middleSplineTracker, Track.Lane.Right);
		float currentTrackPosition = Track.CalculateTrackPositionOfTracker(middleSplineTracker);
		foreach (Node node in laneNodes)
		{
			WorldTransformLock myFloorTransform = laneTransforms[(int)node.Lane];
			float myJumpHeight = jumpHeight * node.Height;
			if (node.IsOrphan)
			{
				yield return new RingPlacement(myFloorTransform, myJumpHeight, node.Substreaks, currentTrackPosition, node.Lane);
				continue;
			}
			foreach (Node parent in node.Parents)
			{
				uint commonSubstreaks = parent.Substreaks & node.Substreaks;
				WorldTransformLock parentFloorTransform = laneTransforms[(int)parent.Lane];
				LightweightTransform blendedFloorTransform = LightweightTransform.Lerp(myFloorTransform.CurrentTransform, parentFloorTransform.CurrentTransform, 0.5f);
				yield return new RingPlacement(blendedFloorTransform, middleSplineTracker.Target.gameObject, myJumpHeight, commonSubstreaks, currentTrackPosition, node.Lane);
			}
		}
	}

	private WorldTransformLock GetTransformOnLane(SplineTracker middleSpline, Track.Lane lane)
	{
		int num;
		switch (lane)
		{
		case Track.Lane.Middle:
			return new WorldTransformLock(middleSpline.Target, middleSpline.CurrentSplineTransform);
		case Track.Lane.Right:
			num = 1;
			break;
		default:
			num = 0;
			break;
		}
		SideDirection toDirection = (SideDirection)num;
		SplineUtils.SplineParameters splineToSideOf = m_track.GetSplineToSideOf(middleSpline.Target, middleSpline.CurrentSplineTransform, toDirection);
		AssertStrafe(splineToSideOf, lane, middleSpline);
		LightweightTransform transform = splineToSideOf.Target.GetTransform(splineToSideOf.StartPosition);
		return new WorldTransformLock(splineToSideOf.Target, transform);
	}

	private void AssertStrafe(SplineUtils.SplineParameters strafeResult, Track.Lane targetLane, SplineTracker middleSource)
	{
	}

	private void ProcessOpenRow()
	{
		if (m_openRowHasAnyRings)
		{
			if (!m_ringGroups.Any() || !m_ringGroups.Last().IsOpen)
			{
				RingGroup item = new RingGroup(m_openRow, m_openRowMissingLane, m_openRowKeyframe);
				m_ringGroups.Add(item);
			}
			else
			{
				m_ringGroups.Last().AddOpenRowToGroup(m_openRow, m_openRowMissingLane, m_openRowKeyframe);
			}
		}
		else if (m_ringGroups.Any())
		{
			m_ringGroups.Last().AddEmptySuffixRow();
		}
		else
		{
			m_prefixEmptyRows++;
		}
	}
}
