using System.Collections;
using UnityEngine;

public class BossLoader : MonoBehaviour
{
	private static BossLoader s_instance;

	public static BossLoader Instance()
	{
		return s_instance;
	}

	private void Awake()
	{
		s_instance = this;
		EventDispatch.RegisterInterest("OnBossBattleEnd", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
	}

	public IEnumerator LoadBoss()
	{
		yield return Application.LoadLevelAdditiveAsync("s_boss");
	}

	public void DestroyBoss()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("BossRoot");
		if ((bool)gameObject)
		{
			Object.Destroy(gameObject);
		}
	}

	private void Event_OnBossBattleEnd()
	{
		DestroyBoss();
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		DestroyBoss();
	}
}
