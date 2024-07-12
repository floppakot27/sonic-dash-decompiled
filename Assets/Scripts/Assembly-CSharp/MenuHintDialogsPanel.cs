using System.Collections.Generic;
using UnityEngine;

public class MenuHintDialogsPanel : MonoBehaviour
{
	private int m_currentHint;

	private int m_currentStore = -1;

	private List<DialogContent_Hints.Hint> m_hintList;

	private void Start()
	{
		m_currentHint = -1;
		m_currentStore = -1;
	}

	private void SelectNextHint()
	{
		bool flag = true;
		if (m_currentStore != -1)
		{
			DialogContent_Hints.Hint hint = m_hintList[m_currentHint];
			for (int i = m_currentStore + 1; i < 4; i++)
			{
				if (hint.m_storeEntry[i] != null && hint.m_storeEntry[i].Length != 0)
				{
					StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(hint.m_storeEntry[i], StoreContent.Identifiers.Name);
					if (storeEntry != null)
					{
						m_currentStore = i;
						flag = false;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			return;
		}
		m_currentHint++;
		if (m_hintList != null && m_currentHint >= m_hintList.Count)
		{
			m_currentHint = 0;
		}
		DialogContent_Hints.Hint hint2 = m_hintList[m_currentHint];
		if ((hint2.m_state & DialogContent_Hints.Hint.State.UseStore) == DialogContent_Hints.Hint.State.UseStore)
		{
			for (int j = 0; j < 4; j++)
			{
				if (hint2.m_storeEntry[j] != null && hint2.m_storeEntry[j].Length != 0)
				{
					StoreContent.StoreEntry storeEntry2 = StoreContent.GetStoreEntry(hint2.m_storeEntry[j], StoreContent.Identifiers.Name);
					if (storeEntry2 != null)
					{
						m_currentStore = j;
						break;
					}
				}
			}
		}
		else
		{
			m_currentStore = -1;
		}
	}

	private void Trigger_ShowNextHint()
	{
		if (m_hintList == null)
		{
			m_hintList = DialogContent_Hints.HintList;
		}
		if (m_currentHint == -1)
		{
			SelectNextHint();
		}
		DialogContent_Hints.Hint hintToUse = m_hintList[m_currentHint];
		DialogContent_Hints.Display(hintToUse, m_currentStore);
		SelectNextHint();
	}
}
