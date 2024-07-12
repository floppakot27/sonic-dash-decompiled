using System;
using System.Collections;
using UnityEngine;

public class GameUnloader : MonoBehaviour
{
	public enum ReloadTrigger
	{
		Unpaused,
		SDKInactive
	}

	private static GameUnloader s_singleton;

	private bool m_isWaitingForUnpause;

	public static bool HasReloaded { get; private set; }

	public static void DoWhileSafe(Action thingToDo, ReloadTrigger reloadTrigger)
	{
		s_singleton.StopAllCoroutines();
		s_singleton.StartCoroutine(s_singleton.DoActionWhileSafe(thingToDo, reloadTrigger));
	}

	private void Awake()
	{
		if (s_singleton == null)
		{
			s_singleton = this;
			m_isWaitingForUnpause = false;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private T NukeEverythingExcept<T>() where T : Component
	{
		T val = (T)null;
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = (GameObject)array[i];
			if (gameObject.transform.parent != null || gameObject == base.gameObject)
			{
				continue;
			}
			if (val == null)
			{
				T component = gameObject.GetComponent<T>();
				if (component != null)
				{
					val = component;
					continue;
				}
			}
			gameObject.BroadcastMessage("OnNukingLevel", SendMessageOptions.DontRequireReceiver);
			UnityEngine.Object.Destroy(gameObject);
		}
		GC.Collect();
		EventDispatch.ClearDispatcher();
		return val;
	}

	private IEnumerator DoActionWhileSafe(Action thingToDo, ReloadTrigger reloadTrigger)
	{
		if (FeatureSupport.IsSupported("ReduceMemUsageForSDKs"))
		{
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			NukeEverythingExcept<PlugInController>();
			yield return null;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			yield return Resources.UnloadUnusedAssets();
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			GC.Collect();
			yield return null;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			SetReloadTrigger(reloadTrigger);
		}
		thingToDo();
	}

	private void SetReloadTrigger(ReloadTrigger reloadTrigger)
	{
		switch (reloadTrigger)
		{
		case ReloadTrigger.SDKInactive:
			EventDispatch.RegisterInterest("3rdPartyInactive", this);
			break;
		case ReloadTrigger.Unpaused:
			m_isWaitingForUnpause = true;
			break;
		}
	}

	private IEnumerator ReloadGame()
	{
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		HasReloaded = true;
		EventDispatch.UnregisterAllInterest(base.gameObject);
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		for (int i = 0; i < array.Length; i++)
		{
			GameObject go = (GameObject)array[i];
			if (!(go.transform.parent != null) && !(go == base.gameObject))
			{
				go.BroadcastMessage("OnNukingLevel", SendMessageOptions.DontRequireReceiver);
				UnityEngine.Object.Destroy(go);
			}
		}
		yield return null;
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		yield return Resources.UnloadUnusedAssets();
		Time.timeScale = 1f;
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		GC.Collect();
		Application.LoadLevelAdditiveAsync(SceneIdentifiers.Names[1]);
	}

	private void Event_3rdPartyInactive()
	{
		StartCoroutine(ReloadGame());
	}

	private void OnApplicationPause(bool isPausing)
	{
		if (!isPausing && m_isWaitingForUnpause)
		{
			m_isWaitingForUnpause = false;
			StartCoroutine(ReloadGame());
		}
	}
}
