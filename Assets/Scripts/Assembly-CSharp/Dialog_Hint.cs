using System.Collections;
using UnityEngine;

public class Dialog_Hint : MonoBehaviour
{
	private const string TitleTag = "Hint_Title";

	private const string DescriptionTag = "Hint_Description";

	private const string MeshTag = "Hint_Mesh";

	private const string StoreTag = "Hint_Store";

	private static DialogContent_Hints.Hint s_currentHint;

	private static int s_currentStoreIndex = -1;

	private GameObject m_titleObject;

	private GameObject m_descriptionObject;

	private StoreMonitor m_storeMonitor;

	private MeshFilter m_mesh;

	public static void SetNextContent(DialogContent_Hints.Hint thisHint, int storeIndex)
	{
		s_currentHint = thisHint;
		s_currentStoreIndex = storeIndex;
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void CacheDialogComponents()
	{
		if (m_titleObject != null && m_descriptionObject != null)
		{
			return;
		}
		m_titleObject = Utils.FindTagInChildren(base.gameObject, "Hint_Title");
		m_descriptionObject = Utils.FindTagInChildren(base.gameObject, "Hint_Description");
		if (m_mesh == null)
		{
			GameObject gameObject = Utils.FindTagInChildren(base.gameObject, "Hint_Mesh");
			MeshFilter[] componentsInChildren = gameObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				m_mesh = componentsInChildren[0];
			}
		}
		if (m_storeMonitor == null)
		{
			GameObject gameObject2 = Utils.FindTagInChildren(base.gameObject, "Hint_Store");
			if (gameObject2 != null)
			{
				m_storeMonitor = Utils.GetComponentInChildren<StoreMonitor>(gameObject2);
			}
		}
	}

	private void UpdateContent()
	{
		if (base.gameObject.activeInHierarchy && !(m_titleObject == null) && !(m_descriptionObject == null))
		{
			UpdateTitleAndDescription();
			UpdateMesh();
			UpdateStore();
		}
	}

	private void UpdateTitleAndDescription()
	{
		LocalisedStringProperties localisedStringProperties = m_titleObject.GetComponentInChildren(typeof(LocalisedStringProperties)) as LocalisedStringProperties;
		LocalisedStringProperties localisedStringProperties2 = m_descriptionObject.GetComponentInChildren(typeof(LocalisedStringProperties)) as LocalisedStringProperties;
		localisedStringProperties.SetLocalisationID(s_currentHint.m_title);
		localisedStringProperties2.SetLocalisationID(s_currentHint.m_description);
		LocalisedStringStatic component = m_titleObject.GetComponent<LocalisedStringStatic>();
		LocalisedStringStatic component2 = m_descriptionObject.GetComponent<LocalisedStringStatic>();
		component.UpdateGuiText();
		component2.UpdateGuiText();
	}

	private void UpdateMesh()
	{
		if (s_currentHint.m_mesh == null || m_mesh == null)
		{
			if (m_mesh != null)
			{
				m_mesh.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_mesh.mesh = s_currentHint.m_mesh;
			m_mesh.gameObject.SetActive(value: true);
		}
	}

	private void UpdateStore()
	{
		if (s_currentStoreIndex == -1 || m_storeMonitor == null)
		{
			if (m_storeMonitor != null)
			{
				m_storeMonitor.gameObject.SetActive(value: false);
			}
			return;
		}
		string text = s_currentHint.m_storeEntry[s_currentStoreIndex];
		if (text == null || text.Length == 0)
		{
			m_storeMonitor.gameObject.SetActive(value: false);
		}
		else
		{
			m_storeMonitor.EntryID = text;
		}
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		CacheDialogComponents();
		UpdateContent();
	}
}
