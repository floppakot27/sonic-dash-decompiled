using UnityEngine;

public class AtlasLoader : MonoBehaviour
{
	public void UpdateAllAtlases()
	{
		UIAtlas[] array = Resources.FindObjectsOfTypeAll(typeof(UIAtlas)) as UIAtlas[];
		for (int i = 0; i < array.Length; i++)
		{
			ReferenceAtlas component = array[i].GetComponent<ReferenceAtlas>();
			if ((bool)component)
			{
				component.UpdateTexture();
			}
		}
		UIFont[] array2 = Resources.FindObjectsOfTypeAll(typeof(UIFont)) as UIFont[];
		for (int j = 0; j < array2.Length; j++)
		{
			ReferenceAtlas component2 = array2[j].GetComponent<ReferenceAtlas>();
			if ((bool)component2)
			{
				component2.UpdateFont();
			}
		}
		Resources.UnloadUnusedAssets();
	}

	private void Start()
	{
		UpdateAllAtlases();
	}
}
