using System.Collections;
using UnityEngine;

public class FESceneLoader : MonoBehaviour
{
	private GameObject m_rootScenic;

	private bool m_loadingScene;

	public bool ScenesLoaded => m_rootScenic != null;

	private void Start()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("StartGameState", this);
	}

	private void FindRootScenic()
	{
		GameObject[] array = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.tag == "Frontend Scenic Root")
			{
				m_rootScenic = gameObject;
			}
		}
	}

	private IEnumerator LoadRootScenic()
	{
		SceneIdentifiers.ID sceneToLoad = SceneIdentifiers.ID.FrontEndScenics_Simple;
		if (FeatureSupport.IsSupported("Extended Menus"))
		{
			sceneToLoad = SceneIdentifiers.ID.FrontEndScenics;
		}
		AsyncOperation loadedScenicScene = Application.LoadLevelAdditiveAsync(SceneIdentifiers.Names[(int)sceneToLoad]);
		do
		{
			yield return null;
		}
		while (!loadedScenicScene.isDone);
		FindRootScenic();
		MoveRootScenic();
		m_loadingScene = false;
	}

	private void MoveRootScenic()
	{
		if (!(m_rootScenic == null))
		{
			WorldCollector.MarkAsMovable(m_rootScenic);
		}
	}

	private void UnloadScenicRoot()
	{
		if (!(m_rootScenic == null))
		{
			Object.Destroy(m_rootScenic.gameObject, 1f);
			m_rootScenic = null;
		}
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		if (resetState == GameState.Mode.Menu && !m_loadingScene)
		{
			FindRootScenic();
			if (m_rootScenic == null)
			{
				m_loadingScene = true;
				StartCoroutine(LoadRootScenic());
			}
		}
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		if (state == GameState.Mode.Game)
		{
			UnloadScenicRoot();
		}
	}
}
