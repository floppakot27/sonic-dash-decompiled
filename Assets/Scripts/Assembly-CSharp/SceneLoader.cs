using System.Collections;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
	private bool m_scenesLoaded;

	private bool m_loadingScene;

	private SceneIdentifiers.ID[] m_highendScenesToLoad = new SceneIdentifiers.ID[4]
	{
		SceneIdentifiers.ID.Water_High,
		SceneIdentifiers.ID.Global,
		SceneIdentifiers.ID.Global,
		SceneIdentifiers.ID.Global
	};

	private SceneIdentifiers.ID[] m_scenesToLoad = new SceneIdentifiers.ID[4]
	{
		SceneIdentifiers.ID.Water,
		SceneIdentifiers.ID.Reef,
		SceneIdentifiers.ID.BackgroundScenics,
		SceneIdentifiers.ID.Skydome
	};

	private SceneIdentifiers.ID[] m_simpleScenesToLoad = new SceneIdentifiers.ID[4]
	{
		SceneIdentifiers.ID.Water_Simple,
		SceneIdentifiers.ID.Global,
		SceneIdentifiers.ID.Global,
		SceneIdentifiers.ID.Global
	};

	public bool ScenesLoaded => m_scenesLoaded;

	private void Start()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnAllAssetsLoaded", this);
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		if (resetState == GameState.Mode.Menu && !m_loadingScene && !m_scenesLoaded)
		{
			m_loadingScene = true;
			StartCoroutine(LoadAdditionalScenes());
		}
	}

	private IEnumerator LoadAdditionalScenes()
	{
		for (int i = 0; i < m_scenesToLoad.Length; i++)
		{
			SceneIdentifiers.ID baseScene = m_scenesToLoad[i];
			SceneIdentifiers.ID simpleScene = m_simpleScenesToLoad[i];
			SceneIdentifiers.ID highendScene = m_highendScenesToLoad[i];
			string sceneToLoad = null;
			if (highendScene != 0 && ShouldSceneBeLoaded(highendScene))
			{
				sceneToLoad = SceneIdentifiers.Names[(int)highendScene];
			}
			else if (ShouldSceneBeLoaded(baseScene))
			{
				sceneToLoad = SceneIdentifiers.Names[(int)baseScene];
			}
			else if (simpleScene != 0)
			{
				sceneToLoad = SceneIdentifiers.Names[(int)simpleScene];
			}
			if (sceneToLoad != null)
			{
				AsyncOperation loadedScenicScene = Application.LoadLevelAdditiveAsync(sceneToLoad);
				do
				{
					yield return null;
				}
				while (!loadedScenicScene.isDone);
			}
		}
		m_scenesLoaded = true;
		m_loadingScene = false;
	}

	private bool ShouldSceneBeLoaded(SceneIdentifiers.ID scene)
	{
		string supportID = $"{scene.ToString()} Scene";
		return FeatureSupport.IsSupported(supportID);
	}

	private void Event_OnAllAssetsLoaded()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Lost World Assets");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			gameObject.SetActive(GameState.GetLostWorldEventActive());
		}
	}
}
