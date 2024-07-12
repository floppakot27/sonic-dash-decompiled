using System.Collections;
using UnityEngine;

public class ObjectDisabler : MonoBehaviour
{
	private void OnEnable()
	{
		StartCoroutine(DelayDisable());
	}

	private IEnumerator DelayDisable()
	{
		yield return null;
		base.gameObject.SetActive(value: false);
	}
}
