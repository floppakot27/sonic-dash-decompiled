using UnityEngine;

public class DCServerTimeModification : MonoBehaviour
{
	public static int HoursToAddToServerTime { get; set; }

	public void DecreaseHours()
	{
		HoursToAddToServerTime--;
		UpdateLabel();
	}

	public void IncreaseHours()
	{
		HoursToAddToServerTime++;
		UpdateLabel();
	}

	private void Start()
	{
		HoursToAddToServerTime = 0;
	}

	private void UpdateLabel()
	{
		UILabel component = GetComponent<UILabel>();
		component.text = HoursToAddToServerTime.ToString();
	}
}
