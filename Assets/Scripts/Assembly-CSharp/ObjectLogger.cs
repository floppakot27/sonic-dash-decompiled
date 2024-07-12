using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[AddComponentMenu("Dash/Debug/Object Logger")]
internal class ObjectLogger : MonoBehaviour
{
	private static ObjectLogger s_singleton;

	private Dictionary<Object, IList<string>> m_objectMsgBin;

	[Conditional("UNITY_EDITOR")]
	public static void Log(Object parent, string text)
	{
		s_singleton.AddTextToParent(parent, text);
	}

	[Conditional("UNITY_EDITOR")]
	public void Awake()
	{
		s_singleton = this;
		m_objectMsgBin = new Dictionary<Object, IList<string>>();
	}

	[Conditional("UNITY_EDITOR")]
	public void Update()
	{
		StartCoroutine(ClearBinsAtEndOfFrame());
	}

	private IEnumerator ClearBinsAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		m_objectMsgBin.Clear();
	}

	private void AddTextToParent(Object parent, string text)
	{
		if (parent is MonoBehaviour)
		{
			parent = (parent as MonoBehaviour).gameObject;
		}
		if (parent is GameObject)
		{
			if (!m_objectMsgBin.ContainsKey(parent))
			{
				m_objectMsgBin[parent] = new List<string>();
			}
			m_objectMsgBin[parent].Add(text);
		}
	}
}
