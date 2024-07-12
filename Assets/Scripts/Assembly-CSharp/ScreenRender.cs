using System;
using UnityEngine;

public class ScreenRender : MonoBehaviour
{
	public CameraTypeMain m_targetCamera;

	public UITexture m_targetTexture;

	private RenderTexture m_renderTexture;

	private int m_targetReduction = 2;

	private void Start()
	{
		if (ScreenTextureRequired())
		{
			CreateRenderTexture();
			AssignRenderTexture();
		}
		else if (m_targetTexture != null)
		{
			UnityEngine.Object.Destroy(m_targetTexture.gameObject);
		}
	}

	private void CreateRenderTexture()
	{
		int renderTextureSize = GetRenderTextureSize();
		m_renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
		m_renderTexture.useMipMap = false;
		m_renderTexture.isCubemap = false;
		bool flag = m_renderTexture.Create();
	}

	private void AssignRenderTexture()
	{
		Camera component = m_targetCamera.GetComponent<Camera>();
		component.targetTexture = m_renderTexture;
		m_targetTexture.mainTexture = m_renderTexture;
	}

	private int GetRenderTextureSize()
	{
		int num = Math.Max(Screen.height, Screen.width);
		return HighestPow2(num / m_targetReduction);
	}

	private bool ScreenTextureRequired()
	{
		if (!SystemInfo.supportsRenderTextures)
		{
			return false;
		}
		bool flag = FeatureSupport.IsSupported("Full Screen Buffer");
		return !flag;
	}

	private int HighestPow2(int val)
	{
		val--;
		val |= val >> 1;
		val |= val >> 2;
		val |= val >> 4;
		val |= val >> 8;
		val |= val >> 16;
		val++;
		return val;
	}
}
