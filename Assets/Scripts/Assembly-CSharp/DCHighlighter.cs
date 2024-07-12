using UnityEngine;

public class DCHighlighter : MonoBehaviour
{
	private const string SavePropertyPostFix = "HighlightDisplayed";

	private bool m_buttonSelected;

	[SerializeField]
	private string m_highlightIdentifier = string.Empty;

	private void Start()
	{
		string saveProperty = GetSaveProperty();
		ActiveProperties activeProperties = PropertyStore.ActiveProperties();
		if (activeProperties.GetBool(saveProperty))
		{
			Object.Destroy(base.gameObject);
		}
		PropertyStore.Store(saveProperty, property: true);
	}

	private void OnDisable()
	{
		RemoveHighlight();
	}

	private void Trigger_DCSelected()
	{
		m_buttonSelected = true;
		RemoveHighlight();
	}

	private void RemoveHighlight()
	{
		if (m_buttonSelected)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private string GetSaveProperty()
	{
		return string.Format("{0}{1}", m_highlightIdentifier, "HighlightDisplayed");
	}
}
