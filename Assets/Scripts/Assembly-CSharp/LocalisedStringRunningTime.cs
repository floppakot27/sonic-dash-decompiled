using UnityEngine;

public class LocalisedStringRunningTime : LocalisedString
{
	protected override void UpdateGuiText(UpdateState updateState)
	{
		string activeString = string.Format(base.Properties.GetLocalisedString(), Time.time.ToString("n2"));
		base.Properties.SetActiveString(activeString);
	}
}
