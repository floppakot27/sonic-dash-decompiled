using System;
using System.Collections;
using UnityEngine;

public class BootState : MonoBehaviour
{
	[Flags]
	private enum State
	{
		AssetsLoaded = 1,
		OnLastIdent = 2,
		DestroyMode = 4
	}

	private State m_state;

	private BundleDownloader m_assetBundleDownloader;

	[SerializeField]
	private LoadingScreenFlow m_screenFlow;

	[SerializeField]
	private UIRoot m_uiRoot;

	private void Start()
	{
		Application.targetFrameRate = 60;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_assetBundleDownloader = GetComponent<BundleDownloader>();
		m_screenFlow.AssetsLoaded = false;
		m_screenFlow.TransitionInEnd = OnIdentTransitionInFinished;
		m_screenFlow.StartFlow(delayPresentation: true);
		EventDispatch.RegisterInterest("ResetGameState", this);
	}

	private void Update()
	{
		if ((m_state & State.DestroyMode) == State.DestroyMode)
		{
			DestroyState();
		}
	}

	private void StartGameTransition()
	{
		if ((m_state & State.AssetsLoaded) == State.AssetsLoaded && (m_state & State.OnLastIdent) == State.OnLastIdent)
		{
			GameState.RequestReset(GameState.Mode.Menu);
		}
	}

	private void DestroyState()
	{
		EventDispatch.UnregisterInterest("ResetGameState", this);
		UnityEngine.Object.Destroy(m_screenFlow.gameObject);
		UnityEngine.Object.Destroy(m_uiRoot.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnIdentTransitionInFinished(int identIndex, int indexCount)
	{
		if (identIndex == indexCount)
		{
			m_state |= State.OnLastIdent;
			StartGameTransition();
		}
		else if (identIndex == 0)
		{
			StartCoroutine(StartBundleDownload());
		}
	}

	private IEnumerator LoadGameScenes()
	{
		AsyncOperation loadedGameScene = Application.LoadLevelAdditiveAsync(SceneIdentifiers.Names[2]);
		do
		{
			yield return null;
		}
		while (!loadedGameScene.isDone);
		m_state |= State.AssetsLoaded;
		StartGameTransition();
	}

	private IEnumerator StartBundleDownload()
	{
		m_assetBundleDownloader.DownloadBundles();
		while (!m_assetBundleDownloader.Finished)
		{
			yield return null;
		}
		StartCoroutine(LoadGameScenes());
	}

	private void Event_ResetGameState(GameState.Mode newMode)
	{
		m_state |= State.DestroyMode;
	}
}
