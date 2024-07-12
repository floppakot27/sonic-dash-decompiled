using UnityEngine;

public class NGUITextureAnimator : MonoBehaviour
{
	public int m_textureFrameDimention = 4;

	public int m_framesPerSecond = 15;

	public bool m_useSystemDelta;

	private int m_totalFrameCount;

	private int m_currentFrame;

	private float m_oneOverRows;

	private float m_oneOverFps;

	private float m_fpsTimer;

	private UITexture m_nguiTexture;

	private void Start()
	{
		m_totalFrameCount = m_textureFrameDimention * m_textureFrameDimention;
		m_currentFrame = 0;
		m_oneOverRows = 1f / (float)m_textureFrameDimention;
		m_oneOverFps = 1f / (float)m_framesPerSecond;
		m_fpsTimer = 0f;
		m_nguiTexture = GetComponent<UITexture>();
	}

	private void Update()
	{
		if (!(m_nguiTexture == null) && UpdateCurrentFrame())
		{
			UpdateCurrentUVs();
		}
	}

	private bool UpdateCurrentFrame()
	{
		bool result = false;
		float num = ((!m_useSystemDelta) ? IndependantTimeDelta.Delta : Time.deltaTime);
		m_fpsTimer += num;
		if (m_fpsTimer >= m_oneOverFps)
		{
			m_currentFrame++;
			if (m_currentFrame >= m_totalFrameCount)
			{
				m_currentFrame = 0;
			}
			m_fpsTimer -= m_oneOverFps;
			result = true;
		}
		return result;
	}

	private void UpdateCurrentUVs()
	{
		Rect uvRect = m_nguiTexture.uvRect;
		float num = m_currentFrame % m_textureFrameDimention;
		float num2 = Mathf.Floor((float)m_currentFrame / (float)m_textureFrameDimention);
		uvRect.xMin = m_oneOverRows * num;
		uvRect.xMax = m_oneOverRows * (num + 1f);
		uvRect.yMin = m_oneOverRows * num2;
		uvRect.yMax = m_oneOverRows * (num2 + 1f);
		m_nguiTexture.uvRect = uvRect;
	}
}
