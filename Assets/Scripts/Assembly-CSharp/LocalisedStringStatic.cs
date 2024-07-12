public class LocalisedStringStatic : LocalisedString
{
	public void UpdateGuiText()
	{
		string localisedString = base.Properties.GetLocalisedString();
		base.Properties.SetActiveString(localisedString);
	}

	protected override void UpdateGuiText(UpdateState updateState)
	{
		if (updateState == UpdateState.Start)
		{
			string localisedString = base.Properties.GetLocalisedString();
			base.Properties.SetActiveString(localisedString);
		}
	}
}
