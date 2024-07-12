using System.Collections;
using UnityEngine;

public class Dialog_DCFinalReward : MonoBehaviour
{
	private const float TimeToDisplayMesh = 0.2f;

	private Mesh[] m_availableMeshes = new Mesh[5];

	private float m_switchMeshTimer;

	private int m_previousMeshIndex = -1;

	private bool m_cycleMeshes = true;

	[SerializeField]
	private MeshFilter m_displayMesh;

	[SerializeField]
	private Mesh m_questionMarkMesh;

	[SerializeField]
	private AudioClip m_meshFlickClip;

	private void Update()
	{
		if (!(m_availableMeshes[0] == null) && m_cycleMeshes)
		{
			UpdateRandomMesh();
		}
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void UpdateRandomMesh()
	{
		m_switchMeshTimer += IndependantTimeDelta.Delta;
		if (m_switchMeshTimer >= 0.2f)
		{
			DisplayRandomMesh();
			m_switchMeshTimer -= 0.2f;
			Audio.PlayClip(m_meshFlickClip, loop: false);
		}
	}

	private void PopulatePrizeMeshList()
	{
		for (int i = 0; i < 5; i++)
		{
			int quantity;
			StoreContent.StoreEntry finalDayReward = DCRewards.GetFinalDayReward(i, out quantity, getFinalQuantity: true);
			m_availableMeshes[i] = finalDayReward.m_mesh;
		}
		m_switchMeshTimer = 0f;
		m_previousMeshIndex = -1;
		m_cycleMeshes = true;
		DisplayRandomMesh();
	}

	private void DisplayRandomMesh()
	{
		int previousMeshIndex = m_previousMeshIndex;
		do
		{
			previousMeshIndex = Random.Range(0, 5);
		}
		while (previousMeshIndex == m_previousMeshIndex);
		m_previousMeshIndex = previousMeshIndex;
		Mesh mesh = m_availableMeshes[previousMeshIndex];
		m_displayMesh.mesh = mesh;
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		PopulatePrizeMeshList();
	}

	private void Trigger_ClaimReward()
	{
		m_displayMesh.mesh = m_questionMarkMesh;
		m_cycleMeshes = false;
	}
}
