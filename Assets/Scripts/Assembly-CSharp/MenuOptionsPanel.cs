using UnityEngine;

public class MenuOptionsPanel : MonoBehaviour
{
	public UICheckbox checkboxMusic;

	public UICheckbox checkboxSfx;

	public UICheckbox m_checkboxTutorial;

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
	}

	private void Update()
	{
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		if ((bool)checkboxMusic)
		{
			if (activeProperties.DoesPropertyExist("MusicVolume") && activeProperties.GetFloat("MusicVolume") == 0f)
			{
				checkboxMusic.startsChecked = false;
			}
			else
			{
				checkboxMusic.startsChecked = true;
			}
		}
		if ((bool)checkboxSfx)
		{
			if (activeProperties.DoesPropertyExist("SfxVolume") && activeProperties.GetFloat("SfxVolume") == 0f)
			{
				checkboxSfx.startsChecked = false;
			}
			else
			{
				checkboxSfx.startsChecked = true;
			}
		}
		if (activeProperties.DoesPropertyExist("ShowTutorial") && (bool)m_checkboxTutorial)
		{
			UICheckbox uICheckbox = m_checkboxTutorial.GetComponent("UICheckbox") as UICheckbox;
			if (activeProperties.GetInt("ShowTutorial") > 0)
			{
				uICheckbox.startsChecked = true;
			}
			else
			{
				uICheckbox.startsChecked = false;
			}
		}
	}
}
