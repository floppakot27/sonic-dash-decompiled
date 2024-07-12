using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenHints : MonoBehaviour
{
	private const string m_indexProperty = "LoadingHintsIndex";

	[SerializeField]
	private GameObject m_hintLabel;

	[SerializeField]
	private UISprite m_hintImage;

	[SerializeField]
	private List<string> m_hints;

	[SerializeField]
	private List<string> m_images;

	private int m_index;

	private bool m_loaded;

	private void Awake()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	private void OnEnable()
	{
		if (m_loaded)
		{
			LocalisedStringProperties.SetLocalisedString(m_hintLabel, m_hints[m_index]);
			m_hintImage.spriteName = m_images[m_index];
			if (++m_index >= m_hints.Count)
			{
				m_index = 0;
			}
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("LoadingHintsIndex", m_index);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_index = activeProperties.GetInt("LoadingHintsIndex");
		m_loaded = true;
	}
}
