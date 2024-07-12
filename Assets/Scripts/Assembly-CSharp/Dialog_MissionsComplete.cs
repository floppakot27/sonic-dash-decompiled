using UnityEngine;

public class Dialog_MissionsComplete : MonoBehaviour
{
	private const string SingleRing = "REWARD_SINGLE_STAR";

	private const string MultipleRing = "REWARD_MULTIPLE_STAR";

	private static Dialog_MissionsComplete instance;

	[SerializeField]
	private UILabel m_dialogText;

	public static void Display()
	{
		LocalisedStringProperties component = instance.m_dialogText.GetComponent<LocalisedStringProperties>();
		string text;
		if (MissionTracker.RSRRewardPerSet == 1)
		{
			text = component.SetLocalisationID("REWARD_SINGLE_STAR");
		}
		else
		{
			string format = component.SetLocalisationID("REWARD_MULTIPLE_STAR");
			text = string.Format(format, MissionTracker.RSRRewardPerSet);
		}
		instance.m_dialogText.text = text;
		DialogStack.ShowDialog("Missions Complete");
	}

	private void Start()
	{
		instance = this;
	}
}
