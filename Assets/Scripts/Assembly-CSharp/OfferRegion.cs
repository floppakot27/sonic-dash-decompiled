using System;
using UnityEngine;

public class OfferRegion : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Active = 1,
		Initialised = 2
	}

	private State m_state;

	[SerializeField]
	private string m_regionName = string.Empty;

	public bool Active => (m_state & State.Active) == State.Active;

	public string RegionName
	{
		set
		{
			m_regionName = value;
		}
	}

	public static void EndAll()
	{
		SLAds.EndOffer(string.Empty);
	}

	public static void Start(string regionName)
	{
		SLAds.ShowOffer(regionName);
	}

	public static void End(string regionName)
	{
		SLAds.EndOffer(regionName);
	}

	public void Visit()
	{
		InitialiseRegion();
		Start(m_regionName);
		m_state |= State.Active;
		OnRegionActivated();
	}

	public void Leave()
	{
		if ((m_state & State.Active) == State.Active)
		{
			End(m_regionName);
			m_state &= ~State.Active;
			OnRegionDectivated();
		}
	}

	protected virtual void OnRegionActivated()
	{
	}

	protected virtual void OnRegionDectivated()
	{
	}

	private void InitialiseRegion()
	{
		if ((m_state & State.Initialised) != State.Initialised)
		{
			m_state |= State.Initialised;
		}
	}
}
