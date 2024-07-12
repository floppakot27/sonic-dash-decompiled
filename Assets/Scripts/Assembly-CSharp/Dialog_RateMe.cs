using UnityEngine;

public class Dialog_RateMe : MonoBehaviour
{
	public static void Display()
	{
		if (DialogContent_RateMe.Instance.validToDisplay())
		{
			DialogStack.ShowDialog("Rate Me Dialog");
		}
	}

	private void Start()
	{
	}

	private void Trigger_NoThanksRateMe()
	{
		DialogContent_RateMe.Instance.Trigger_NoThanks();
	}

	private void Trigger_RemindRateMe()
	{
		DialogContent_RateMe.Instance.Trigger_Remind();
	}

	private void Trigger_YesRateMe()
	{
		DialogContent_RateMe.Instance.Trigger_Yes();
	}
}
