using UnityEngine;

public class BundleIdentifier : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		string text = string.Format("Bundle: {0}", "** Local Build **");
		GUIText component = GetComponent<GUIText>();
		if ((bool)component)
		{
			component.text = text;
		}
		UILabel component2 = GetComponent<UILabel>();
		if ((bool)component2)
		{
			component2.text = text;
		}
	}
}
