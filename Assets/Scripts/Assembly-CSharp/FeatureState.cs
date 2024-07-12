using System;
using System.Collections;
using UnityEngine;

public class FeatureState : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Ready = 1
	}

	private const FileDownloader.Files FileLocation = FileDownloader.Files.FeatureState;

	private static FeatureState s_featureState;

	private static LSON.Root[] s_lsonRoot;

	private static State s_state;

	public static bool Ready => (s_state & State.Ready) == State.Ready;

	public static bool Valid => s_lsonRoot != null;

	public static LSON.Property GetStateProperty(string rootName, string propertyName)
	{
		if (s_lsonRoot == null)
		{
			return null;
		}
		for (int i = 0; i < s_lsonRoot.Length; i++)
		{
			LSON.Root root = s_lsonRoot[i];
			if (root.m_name != rootName)
			{
				continue;
			}
			for (int j = 0; j < root.m_properties.Length; j++)
			{
				LSON.Property property = root.m_properties[j];
				if (!(property.m_name != propertyName))
				{
					return property;
				}
			}
		}
		return null;
	}

	public static void Restart()
	{
		s_lsonRoot = null;
		s_state = (State)0;
		s_featureState.StartCoroutine(s_featureState.DownloadServerFile());
	}

	private void Start()
	{
		s_featureState = this;
	}

	private IEnumerator DownloadServerFile()
	{
		FileDownloader fdownloader = new FileDownloader(FileDownloader.Files.FeatureState, keepAndUseLocalCopy: true);
		yield return fdownloader.Loading;
		if (fdownloader.Error == null)
		{
			s_lsonRoot = LSONReader.Parse(fdownloader.Text);
		}
		s_state |= State.Ready;
		EventDispatch.GenerateEvent("FeatureStateReady");
	}
}
