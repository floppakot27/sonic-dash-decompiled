using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackGameObject : TrackEntity
{
	private IEnumerable<Renderer> m_cachedRenderers;

	private bool m_startedWithValidObject;

	public override bool IsValid
	{
		get
		{
			if (!m_startedWithValidObject)
			{
				return true;
			}
			if (Object == null || !Object.activeInHierarchy)
			{
				return false;
			}
			if (m_cachedRenderers != null && m_cachedRenderers.Any() && m_cachedRenderers.All((Renderer r) => r != null && !r.enabled))
			{
				return false;
			}
			return true;
		}
	}

	public GameObject Object { get; private set; }

	public TrackGameObject(Kind kind, Track.Lane lane)
		: this(kind, lane, null)
	{
	}

	public TrackGameObject(Kind kind, Track.Lane lane, GameObject obj)
		: base(kind, lane)
	{
		Object = obj;
		m_startedWithValidObject = obj != null;
		m_cachedRenderers = ((!(obj == null)) ? obj.GetComponentsInChildren<Renderer>() : null);
	}

	public override string ToString()
	{
		string text = ((!(Object == null)) ? Object.ToString() : "Entity");
		return text + " of " + base.ToString();
	}
}
