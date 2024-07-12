using UnityEngine;

public class NGUITextureForcer : MonoBehaviour
{
	public Texture2D m_nguiTexture;

	private void Start()
	{
		if (m_nguiTexture != null)
		{
			UITexture component = GetComponent<UITexture>();
			if ((bool)component)
			{
				component.mainTexture = m_nguiTexture;
			}
		}
	}
}
