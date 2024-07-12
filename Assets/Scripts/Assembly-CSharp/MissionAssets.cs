using System;
using UnityEngine;

public class MissionAssets : MonoBehaviour
{
	[Serializable]
	public class Assets
	{
		public string m_description = string.Empty;

		public Mesh m_mesh;

		public int m_cost = 1;
	}

	[Serializable]
	public class AssetLists
	{
		public Assets[] m_missionOneAssets;

		public Assets[] m_missionTwoAssets;

		public Assets[] m_missionThreeAssets;
	}

	private static MissionAssets m_missionAssets;

	[SerializeField]
	private AssetLists m_assetLists;

	public static AssetLists ListOfAssets
	{
		get
		{
			m_missionAssets.InitialiseAssetLists();
			return m_missionAssets.m_assetLists;
		}
	}

	private void Start()
	{
		m_missionAssets = this;
	}

	private void InitialiseAssetLists()
	{
		if (m_assetLists == null)
		{
			int enumCount = Utils.GetEnumCount<MissionTracker.MissionLevel1>();
			m_assetLists = new AssetLists();
			m_assetLists.m_missionOneAssets = new Assets[enumCount];
			m_assetLists.m_missionTwoAssets = new Assets[enumCount];
			m_assetLists.m_missionThreeAssets = new Assets[enumCount];
			InitialiseSingleAssetList(m_assetLists.m_missionOneAssets);
			InitialiseSingleAssetList(m_assetLists.m_missionTwoAssets);
			InitialiseSingleAssetList(m_assetLists.m_missionThreeAssets);
		}
	}

	private void InitialiseSingleAssetList(Assets[] assetList)
	{
		for (int i = 0; i < assetList.Length; i++)
		{
			assetList[i] = new Assets();
		}
	}
}
