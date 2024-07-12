using UnityEngine;

public class MissionUtils
{
	public const string SkipMissionStoreEntry = "Skip Mission";

	public static float GetMissionProgress(ref MissionTracker.Mission thisMission)
	{
		float num = thisMission.m_amountCurrent - thisMission.m_amountStart;
		float num2 = thisMission.m_amountTarget - thisMission.m_amountStart;
		float value = num / num2;
		return Mathf.Clamp(value, 0f, 1f);
	}

	public static float GetMissionSetProgress()
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			MissionTracker.Mission thisMission = MissionTracker.GetActiveMission(i);
			num += GetMissionProgress(ref thisMission);
		}
		return num / 3f;
	}
}
