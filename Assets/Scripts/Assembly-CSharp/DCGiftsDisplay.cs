using System.Collections;
using UnityEngine;

public class DCGiftsDisplay : MonoBehaviour
{
	[SerializeField]
	private MeshFilter[] m_meshEntries;

	[SerializeField]
	private UILabel[] m_quantityEntries;

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		for (int i = 0; i < 5; i++)
		{
			int quantityToAward = 0;
			StoreContent.StoreEntry entryToAward = DCRewards.GetFinalDayReward(i, out quantityToAward, getFinalQuantity: true);
			PopulateGiftDisplay(i, entryToAward, quantityToAward);
		}
	}

	private void PopulateGiftDisplay(int index, StoreContent.StoreEntry entry, int quantity)
	{
		if (quantity > 1)
		{
			m_quantityEntries[index].text = quantity.ToString();
		}
		else
		{
			m_quantityEntries[index].text = string.Empty;
		}
		m_meshEntries[index].mesh = entry.m_mesh;
	}
}
