using UnityEngine;

public class ReferenceAtlas : MonoBehaviour
{
	[SerializeField]
	public string m_baseAtlas;

	public void UpdateTexture()
	{
		if (HighDefAtlasSupport())
		{
			string path = $"Asset Bundles/HD Textures - GUI/{m_baseAtlas}-hd";
			UIAtlas replacement = Resources.Load(path, typeof(UIAtlas)) as UIAtlas;
			UIAtlas component = GetComponent<UIAtlas>();
			component.replacement = replacement;
		}
	}

	public void UpdateFont()
	{
		if (HighDefAtlasSupport())
		{
			string path = $"Asset Bundles/HD Textures - GUI/{m_baseAtlas}-hd";
			UIFont replacement = Resources.Load(path, typeof(UIFont)) as UIFont;
			UIFont component = GetComponent<UIFont>();
			component.replacement = replacement;
		}
	}

	private bool HighDefAtlasSupport()
	{
		if (Application.isEditor)
		{
			return false;
		}
		if (!FeatureSupport.IsSupported("HD Textures"))
		{
			return false;
		}
		return true;
	}
}
