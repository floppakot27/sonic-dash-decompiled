using UnityEngine;

public class TimeScaler : MonoBehaviour
{
	private static float m_systemScale = 1f;

	private static float m_userScale = 1f;

	private static float m_gameplayScale = 1f;

	private static float m_bulletTimeScale = 1f;

	private static float m_bossIntroTimeScale = 1f;

	public static float Scale
	{
		get
		{
			return m_userScale;
		}
		set
		{
			m_userScale = value;
		}
	}

	public static float GameplayScale
	{
		set
		{
			m_gameplayScale = value;
		}
	}

	public static float BulletTimeScale
	{
		set
		{
			m_bulletTimeScale = value;
		}
	}

	public static float BossIntroTimeScale
	{
		set
		{
			m_bossIntroTimeScale = value;
		}
	}

	private void Start()
	{
		SetTimeScale();
	}

	private void Update()
	{
		SetTimeScale();
	}

	private static void SetTimeScale()
	{
		Time.timeScale = m_systemScale * m_userScale * m_gameplayScale * m_bulletTimeScale * m_bossIntroTimeScale;
	}
}
