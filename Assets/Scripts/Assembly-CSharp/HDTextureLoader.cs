using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HDTextureLoader : MonoBehaviour
{
	private static HDTextureLoader s_instance;

	[SerializeField]
	private string[] m_textureProperies;

	[SerializeField]
	private string[] m_textureNames;

	public static IEnumerator ReplaceHDVariants(string bundleName, bool loadAsFastAsPossible, bool unregisterBundle)
	{
		if (Application.isEditor || !FeatureSupport.IsSupported("HD Textures"))
		{
			yield break;
		}
		Material[] materialList = Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[];
		List<Texture2D> texturesReplaced = new List<Texture2D>(materialList.Length);
		if (!loadAsFastAsPossible)
		{
			yield return null;
		}
		foreach (Material thisMaterial in materialList)
		{
			for (int propertyIndex = 0; propertyIndex < s_instance.m_textureProperies.Length; propertyIndex++)
			{
				string thisProperty = s_instance.m_textureProperies[propertyIndex];
				if (!thisMaterial.HasProperty(thisProperty))
				{
					continue;
				}
				Texture2D thisTexture = thisMaterial.GetTexture(thisProperty) as Texture2D;
				if (thisTexture == null)
				{
					continue;
				}
				string hdTextureName = $"{thisTexture.name.ToLowerInvariant()}-hd";
				for (int textureIndex = 0; textureIndex < s_instance.m_textureNames.Length; textureIndex++)
				{
					if (s_instance.m_textureNames[textureIndex] != hdTextureName)
					{
						continue;
					}
					string texturePath = $"Asset Bundles/{bundleName}/{hdTextureName}";
					Texture2D hdTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
					if (!(hdTexture != null))
					{
						continue;
					}
					thisMaterial.SetTexture(thisProperty, hdTexture);
					bool alreadyStored = false;
					for (int storedIndex = 0; storedIndex < texturesReplaced.Count; storedIndex++)
					{
						if (texturesReplaced[storedIndex] == thisTexture)
						{
							alreadyStored = true;
						}
					}
					if (!alreadyStored)
					{
						texturesReplaced.Add(thisTexture);
					}
					if (!loadAsFastAsPossible)
					{
						yield return null;
					}
					break;
				}
			}
		}
		for (int textureIndex = 0; textureIndex < texturesReplaced.Count; textureIndex++)
		{
			Resources.UnloadAsset(texturesReplaced[textureIndex]);
			if (!loadAsFastAsPossible)
			{
				yield return null;
			}
		}
		if (unregisterBundle)
		{
			BundleResource.UnregisterBundle(bundleName, deleteAllLoadedObjects: false);
		}
	}

	private void Start()
	{
		s_instance = this;
		if (FeatureSupport.IsSupported("HD Textures"))
		{
			EventDispatch.RegisterInterest("OnAllAssetsLoaded", this, EventDispatch.Priority.Lowest);
		}
	}

	private void Event_OnAllAssetsLoaded()
	{
		StartCoroutine(ReplaceHDVariants("HD Textures - Universal", loadAsFastAsPossible: true, unregisterBundle: true));
	}
}
