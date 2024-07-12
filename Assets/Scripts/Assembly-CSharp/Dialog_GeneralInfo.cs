using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog_GeneralInfo : MonoBehaviour
{
	private const string TitleTag = "GeneralDialog_Title";

	private const string InfoTag = "GeneralDialog_Information";

	private GameObject m_titleObject;

	private GameObject m_infoObject;

	private Stack<DialogContent_GeneralInfo.Content> m_stackedContent = new Stack<DialogContent_GeneralInfo.Content>(5);

	public void SetContent(DialogContent_GeneralInfo.Type dialogType)
	{
		DialogContent_GeneralInfo.Content content = DialogContent_GeneralInfo.GetContent(dialogType);
		m_stackedContent.Push(content);
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void OnDisable()
	{
	}

	private void CacheDialogComponents()
	{
		if (!(m_titleObject != null) || !(m_infoObject != null))
		{
			m_titleObject = Utils.FindTagInChildren(base.gameObject, "GeneralDialog_Title");
			m_infoObject = Utils.FindTagInChildren(base.gameObject, "GeneralDialog_Information");
		}
	}

	private void UpdateContent()
	{
		if (base.gameObject.activeInHierarchy && !(m_titleObject == null) && !(m_infoObject == null))
		{
			DialogContent_GeneralInfo.Content content = m_stackedContent.Peek();
			LocalisedStringProperties localisedStringProperties = m_titleObject.GetComponentInChildren(typeof(LocalisedStringProperties)) as LocalisedStringProperties;
			LocalisedStringProperties localisedStringProperties2 = m_infoObject.GetComponentInChildren(typeof(LocalisedStringProperties)) as LocalisedStringProperties;
			localisedStringProperties.SetLocalisationID(content.m_titleLoc);
			localisedStringProperties2.SetLocalisationID(content.m_infoLoc);
			LocalisedStringStatic component = m_titleObject.GetComponent<LocalisedStringStatic>();
			LocalisedStringStatic component2 = m_infoObject.GetComponent<LocalisedStringStatic>();
			component.UpdateGuiText();
			component2.UpdateGuiText();
		}
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		CacheDialogComponents();
		UpdateContent();
	}

	private void DialogPopped()
	{
		m_stackedContent.Pop();
	}
}
