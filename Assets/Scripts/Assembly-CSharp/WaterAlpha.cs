using UnityEngine;

public class WaterAlpha : MonoBehaviour
{
	public GameObject m_targetPlane;

	private Vector2 m_scrollValueA;

	private Vector2 m_scrollValueB;

	public Vector2 m_scrollSpeedA;

	public Vector2 m_scrollSpeedB;

	private float m_fTextureTileWidth;

	private void Awake()
	{
		WorldCollector.MarkAsMovable(base.gameObject);
		m_scrollValueA = new Vector2(0f, 0f);
		m_scrollValueB = new Vector2(0f, 0f);
		MeshFilter meshFilter = (MeshFilter)m_targetPlane.GetComponent("MeshFilter");
		Mesh mesh = meshFilter.mesh;
		float x = mesh.bounds.size.x;
		Vector2 textureScale = m_targetPlane.renderer.material.GetTextureScale("_MainTex");
		float num = x * m_targetPlane.transform.localScale.x;
		m_fTextureTileWidth = num / textureScale.x;
	}

	private void Update()
	{
		if ((bool)m_targetPlane)
		{
			m_scrollValueA += m_scrollSpeedA * Time.deltaTime;
			Vector2 scrollValueA = m_scrollValueA;
			scrollValueA.x -= base.transform.position.x / m_fTextureTileWidth;
			scrollValueA.y -= base.transform.position.z / m_fTextureTileWidth;
			scrollValueA.x %= 1f;
			scrollValueA.y %= 1f;
			m_targetPlane.renderer.material.SetTextureOffset("_MainTex", scrollValueA);
			m_scrollValueB += m_scrollSpeedB * Time.deltaTime;
			scrollValueA = m_scrollValueB;
			scrollValueA.x -= base.transform.position.x / m_fTextureTileWidth;
			scrollValueA.y -= base.transform.position.z / m_fTextureTileWidth;
			scrollValueA.x %= 1f;
			scrollValueA.y %= 1f;
			m_targetPlane.renderer.material.SetTextureOffset("_SecondTex", scrollValueA);
		}
	}

	private void OnDestroy()
	{
	}
}
