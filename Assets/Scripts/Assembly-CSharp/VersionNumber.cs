using UnityEngine;

public class VersionNumber : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		string @string = LanguageStrings.First.GetString("OPTIONS_VERSION");
		string text = string.Format(@string, "1.8.0");
		UILabel component = GetComponent<UILabel>();
		if ((bool)component)
		{
			component.text = text;
		}
	}
}
