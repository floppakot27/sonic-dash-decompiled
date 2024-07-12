using System;
using System.Collections.Generic;

public class OneDTree<T>
{
	private class Node
	{
		public T Data { get; set; }

		public float Position { get; set; }

		public Node Left { get; set; }

		public Node Right { get; set; }

		public Node(T data, float location)
		{
			Data = data;
			Position = location;
		}

		public bool IsPointToRight(float point)
		{
			return point > Position;
		}

		public Node FindLeaf(float positionToFind)
		{
			if (IsPointToRight(positionToFind))
			{
				return (Right != null) ? Right.FindLeaf(positionToFind) : this;
			}
			return (Left != null) ? Left.FindLeaf(positionToFind) : this;
		}

		public void InOrderVisit(Action<Node, int> visitor, int level)
		{
			if (Left != null)
			{
				Left.InOrderVisit(visitor, level + 1);
			}
			visitor(this, level);
			if (Right != null)
			{
				Right.InOrderVisit(visitor, level + 1);
			}
		}
	}

	public delegate bool DataWhereClause(T data);

	private delegate bool NodeWhereClause(Node n);

	private Stack<Node> m_anyNodesOpenScratchList = new Stack<Node>();

	private IList<Pair<float, T>> m_currentDistCaptureList;

	private IList<T> m_currentCaptureList;

	private Node m_root;

	public void Insert(T value, float position)
	{
		Node node = new Node(value, position);
		if (m_root == null)
		{
			m_root = node;
			return;
		}
		Node node2 = FindLeaf(position);
		if (node2.IsPointToRight(position))
		{
			node2.Right = node;
		}
		else
		{
			node2.Left = node;
		}
	}

	public void RemoveAllBeforePosition(float deletionInclusiveUpperLimit)
	{
		if (m_root == null)
		{
			return;
		}
		List<Node> listSoFar = new List<Node>();
		AllInOrder(m_root, ref listSoFar);
		int num = 0;
		foreach (Node item in listSoFar)
		{
			if (item.Position > deletionInclusiveUpperLimit)
			{
				break;
			}
			num++;
		}
		m_root = MakeTree(listSoFar, num, listSoFar.Count);
	}

	public void Rebalance()
	{
		if (m_root != null)
		{
			List<Node> listSoFar = new List<Node>();
			AllInOrder(m_root, ref listSoFar);
			m_root = MakeTree(listSoFar, 0, listSoFar.Count);
		}
	}

	public bool IsInRange(float minInclusive, float maxInclusive, DataWhereClause isDesiredData)
	{
		return AnyNodesInRange(minInclusive, maxInclusive, (Node node) => isDesiredData(node.Data));
	}

	public bool AnyNodesInRange(float minInclusive, float maxInclusive, DataWhereClause processData)
	{
		return AnyNodesInRange(minInclusive, maxInclusive, (Node node) => processData(node.Data));
	}

	public void AllDistanceNodesInRange(float minInclusive, float maxInclusive, ref IList<Pair<float, T>> distDataOut, Func<T, bool> dataValidator)
	{
		distDataOut.Clear();
		m_currentDistCaptureList = distDataOut;
		AnyNodesInRange(minInclusive, maxInclusive, delegate(Node n)
		{
			if (dataValidator(n.Data))
			{
				m_currentDistCaptureList.Add(new Pair<float, T>(n.Position, n.Data));
			}
			return false;
		});
	}

	public void AllNodesInRange(float minInclusive, float maxInclusive, ref IList<T> dataOut)
	{
		SaveCaptureList(ref dataOut);
		AnyNodesInRange(minInclusive, maxInclusive, CaptureNode);
	}

	public void AllNodesInRange(float minInclusive, float maxInclusive, ref IList<T> dataOut, DataWhereClause tValidator)
	{
		SaveCaptureList(ref dataOut);
		AnyNodesInRange(minInclusive, maxInclusive, delegate(Node n)
		{
			if (tValidator(n.Data))
			{
				CaptureNode(n);
			}
			return false;
		});
	}

	public float CalculateBalancedFactor()
	{
		int deepestLevel = 0;
		int totalNodes = 0;
		InOrderVisit(delegate(Node node, int level)
		{
			totalNodes++;
			deepestLevel = ((level <= deepestLevel) ? deepestLevel : level);
		});
		float num = (float)Math.Ceiling(Math.Log((double)totalNodes + 1.0, 2.0));
		return (float)deepestLevel / num;
	}

	private void SaveCaptureList(ref IList<T> dataOut)
	{
		m_currentCaptureList = dataOut;
		m_currentCaptureList.Clear();
	}

	private bool CaptureNode(Node n)
	{
		m_currentCaptureList.Add(n.Data);
		return false;
	}

	private bool AnyNodesInRange(float minInclusive, float maxInclusive, NodeWhereClause processNode)
	{
		if (m_root == null)
		{
			return false;
		}
		Stack<Node> anyNodesOpenScratchList = m_anyNodesOpenScratchList;
		anyNodesOpenScratchList.Clear();
		anyNodesOpenScratchList.Push(m_root);
		while (anyNodesOpenScratchList.Count > 0)
		{
			Node node = anyNodesOpenScratchList.Pop();
			if (node.Position <= maxInclusive && node.Position >= minInclusive && processNode(node))
			{
				return true;
			}
			if (node.Position >= minInclusive && node.Left != null)
			{
				anyNodesOpenScratchList.Push(node.Left);
			}
			if (node.Position <= maxInclusive && node.Right != null)
			{
				anyNodesOpenScratchList.Push(node.Right);
			}
		}
		return false;
	}

	private void InOrderVisit(Action<Node, int> visitor)
	{
		if (m_root != null)
		{
			m_root.InOrderVisit(visitor, 1);
		}
	}

	private Node MakeTree(List<Node> nodes, int minIncIndex, int maxExcIndex)
	{
		if (minIncIndex + 1 == maxExcIndex)
		{
			Node node = nodes[minIncIndex];
			node.Left = null;
			node.Right = null;
			return node;
		}
		int num = (minIncIndex + maxExcIndex) / 2;
		Node node2 = nodes[num];
		node2.Left = ((num <= minIncIndex) ? null : MakeTree(nodes, minIncIndex, num));
		node2.Right = ((num >= maxExcIndex - 1) ? null : MakeTree(nodes, num + 1, maxExcIndex));
		return node2;
	}

	private void AllInOrder(Node currentNode, ref List<Node> listSoFar)
	{
		if (currentNode != null)
		{
			AllInOrder(currentNode.Left, ref listSoFar);
			listSoFar.Add(currentNode);
			AllInOrder(currentNode.Right, ref listSoFar);
		}
	}

	private Node FindLeaf(float position)
	{
		if (m_root == null)
		{
			return null;
		}
		return m_root.FindLeaf(position);
	}
}
