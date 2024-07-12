using System.Collections;
using UnityEngine;

public class FrameTimeSentinal : MonoBehaviour
{
	private static float s_realTimeAtEndOfLastFrame;

	public static bool IsFramerateImportant
	{
		get
		{
			bool flag = Application.isPlaying && GameState.GetMode() == GameState.Mode.Game;
			bool flag2 = FrameDurationSoFar > Time.maximumDeltaTime * 0.75f;
			return flag && flag2;
		}
	}

	public static float RealTimeAtEndOfLastFrame
	{
		get
		{
			return s_realTimeAtEndOfLastFrame;
		}
		private set
		{
			s_realTimeAtEndOfLastFrame = value;
		}
	}

	public static float FrameDurationSoFar => Time.realtimeSinceStartup - RealTimeAtEndOfLastFrame;

	private void Awake()
	{
		StartCoroutine(RealTimeAtEndOfLastFrameUpdater());
	}

	private IEnumerator RealTimeAtEndOfLastFrameUpdater()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			RealTimeAtEndOfLastFrame = Time.realtimeSinceStartup;
		}
	}
}
