using UnityEngine;

public class ActivateTrigger : MonoBehaviour
{
	public enum Mode
	{
		Trigger = 0,
		Replace = 1,
		Activate = 2,
		Enable = 3,
		Animate = 4,
		Deactivate = 5,
	}

	public Mode action;
	public Object target;
	public GameObject source;
	public int triggerCount;
	public bool repeatTrigger;
}
