using System.Collections;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
	public enum Mode
	{
		Global,
		Boot,
		Game
	}

	[SerializeField]
	private Mode m_entryMode;

	private void Awake()
	{
		EntryPoint[] array = Object.FindObjectsOfType(typeof(EntryPoint)) as EntryPoint[];
		if (array.Length > 1)
		{
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		StartCoroutine(LoadGameScenes(m_entryMode));
	}

	private IEnumerator LoadGameScenes(Mode entryMode)
	{
		LoadScenes(entryMode);
		yield return null;
		yield return null;
		NotifyGame(entryMode);
	}

	private void LoadScenes(Mode entryMode)
	{
		switch (entryMode)
		{
		case Mode.Global:
			Application.LoadLevelAdditive(SceneIdentifiers.Names[1]);
			break;
		case Mode.Boot:
			Application.LoadLevelAdditive(SceneIdentifiers.Names[0]);
			break;
		case Mode.Game:
			Application.LoadLevelAdditive(SceneIdentifiers.Names[0]);
			break;
		}
	}

	private void NotifyGame(Mode entryMode)
	{
		if (entryMode != Mode.Boot && entryMode != 0)
		{
			GameState.RequestReset(GameState.Mode.Menu);
		}
	}
}
