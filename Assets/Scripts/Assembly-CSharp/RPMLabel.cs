using UnityEngine;

public class RPMLabel : MonoBehaviour
{
	private void Update()
	{
		UILabel component = GetComponent<UILabel>();
		component.text = RingPerMinute.Current.ToString("N2");
	}
}
