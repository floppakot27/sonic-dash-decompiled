using System.Collections;
using UnityEngine;

public class MissionUI_MainMenuProgress : MonoBehaviour
{
	[SerializeField]
	private UISlider m_totalProgression;

	[SerializeField]
	private UISprite m_mission1;

	[SerializeField]
	private UISprite m_mission2;

	[SerializeField]
	private UISprite m_mission3;

	[SerializeField]
	private GameObject m_tween;

	private void Start()
	{
	}

	private void OnEnable()
	{
		if (MissionTracker.AllMissionsComplete())
		{
			StartCoroutine(Deactivate());
			return;
		}
		float missionSetProgress = MissionUtils.GetMissionSetProgress();
		m_totalProgression.sliderValue = missionSetProgress;
		MissionTracker.Mission thisMission = MissionTracker.GetActiveMission(0);
		missionSetProgress = MissionUtils.GetMissionProgress(ref thisMission);
		if (missionSetProgress < 1f)
		{
			m_mission1.alpha = 0f;
		}
		else
		{
			m_mission1.alpha = 1f;
		}
		thisMission = MissionTracker.GetActiveMission(1);
		missionSetProgress = MissionUtils.GetMissionProgress(ref thisMission);
		if (missionSetProgress < 1f)
		{
			m_mission2.alpha = 0f;
		}
		else
		{
			m_mission2.alpha = 1f;
		}
		thisMission = MissionTracker.GetActiveMission(2);
		missionSetProgress = MissionUtils.GetMissionProgress(ref thisMission);
		if (missionSetProgress < 1f)
		{
			m_mission3.alpha = 0f;
		}
		else
		{
			m_mission3.alpha = 1f;
		}
	}

	private IEnumerator Deactivate()
	{
		yield return null;
		m_tween.SetActive(value: false);
	}
}
