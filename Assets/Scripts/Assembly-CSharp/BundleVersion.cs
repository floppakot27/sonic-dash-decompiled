using System.Text.RegularExpressions;
using UnityEngine;

public class BundleVersion : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		string buildConfiguration = GetBuildConfiguration();
		string text = string.Format("Version: {0} ({1} Build)", "** Local Build **", buildConfiguration);
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

	private string GetBuildConfiguration()
	{
		string text = BuildConfiguration.Current.ToString();
		return text[0] + Regex.Replace(text.Substring(1), "[A-Z]", " $0");
	}
}
