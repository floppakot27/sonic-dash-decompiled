using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallpapers : MonoBehaviour
{
	private class DownloadingWallpapers
	{
		public StoreContent.StoreEntry m_rootEntry;

		public StoreContent.StoreEntry[] m_awardedEntries;

		public AssetBundle[] m_loadedBundles;
	}

	[Flags]
	private enum State
	{
		Downloading = 1,
		Error = 2,
		BlockAwards = 4,
		Awarding = 8
	}

	private const string WallpaperPath = "Wallpapers";

	private static Wallpapers s_instance;

	private State m_state;

	private List<DownloadingWallpapers> m_papersToDownload = new List<DownloadingWallpapers>();

	private List<DownloadingWallpapers> m_papersToAward = new List<DownloadingWallpapers>();

	private List<StoreContent.StoreEntry> m_wallpaperEntries;

	[SerializeField]
	private string[] m_hiddenWallpapersGC3;

	public static void Award(StoreContent.StoreEntry wallpaperEntry)
	{
		if (wallpaperEntry != null && wallpaperEntry.m_type == StoreContent.EntryType.Wallpaper)
		{
			s_instance.AwardWallpaper(wallpaperEntry);
		}
	}

	private void Start()
	{
		s_instance = this;
		EventDispatch.RegisterInterest("MainMenuActive", this);
		EventDispatch.RegisterInterest("OnNewGameAboutToStart", this);
		EventDispatch.RegisterInterest("OnTransitionStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
	}

	private void Update()
	{
		CheckWallpapersToDownload();
		CheckWallpapersToAward();
	}

	private void AwardWallpaper(StoreContent.StoreEntry wallpaperEntry)
	{
		if (m_wallpaperEntries == null)
		{
			m_wallpaperEntries = StoreContent.StockList.FindAll((StoreContent.StoreEntry entry) => entry.m_type == StoreContent.EntryType.Wallpaper);
		}
		StoreEntriesToDownload(wallpaperEntry);
		CheckRelatedWallpaperStates(wallpaperEntry);
	}

	private void StoreEntriesToDownload(StoreContent.StoreEntry wallpaperEntry)
	{
		DownloadingWallpapers downloadingWallpapers = new DownloadingWallpapers();
		downloadingWallpapers.m_rootEntry = wallpaperEntry;
		downloadingWallpapers.m_rootEntry.m_downloadProgress = 0f;
		downloadingWallpapers.m_rootEntry.m_state |= StoreContent.StoreEntry.State.Downloading;
		int rewardedWallpaperCount = GetRewardedWallpaperCount(wallpaperEntry);
		downloadingWallpapers.m_awardedEntries = new StoreContent.StoreEntry[rewardedWallpaperCount];
		downloadingWallpapers.m_loadedBundles = new AssetBundle[rewardedWallpaperCount];
		for (int i = 0; i < wallpaperEntry.m_wallpaperResources.Length && !string.IsNullOrEmpty(wallpaperEntry.m_wallpaperResources[i]); i++)
		{
			StoreContent.StoreEntry storeEntry = FindWallpaperStoreEntry(wallpaperEntry.m_wallpaperResources[i]);
			if (!FindWallpaperInDownloadList(storeEntry))
			{
				downloadingWallpapers.m_awardedEntries[i] = storeEntry;
				downloadingWallpapers.m_awardedEntries[i].m_downloadProgress = 0f;
				downloadingWallpapers.m_awardedEntries[i].m_state |= StoreContent.StoreEntry.State.Downloading;
			}
		}
		m_papersToDownload.Add(downloadingWallpapers);
	}

	private void CheckRelatedWallpaperStates(StoreContent.StoreEntry wallpaperEntry)
	{
		for (int i = 0; i < wallpaperEntry.m_wallpaperResources.Length && !string.IsNullOrEmpty(wallpaperEntry.m_wallpaperResources[i]); i++)
		{
			string wallpaperName = wallpaperEntry.m_wallpaperResources[i];
			for (int j = 0; j < m_wallpaperEntries.Count; j++)
			{
				StoreContent.StoreEntry storeEntry = m_wallpaperEntries[j];
				if (WallpaperExistsInEntry(wallpaperName, storeEntry))
				{
					int rewardedWallpaperCount = GetRewardedWallpaperCount(storeEntry);
					if (rewardedWallpaperCount == 1)
					{
						StoreUtils.SaveOneShotPurchase(storeEntry);
					}
					else if (BundleWallpapersAllPurchased(storeEntry, m_wallpaperEntries))
					{
						StoreUtils.SaveOneShotPurchase(storeEntry);
					}
				}
			}
		}
	}

	private bool FindWallpaperInDownloadList(StoreContent.StoreEntry wallpaperEntry)
	{
		for (int i = 0; i < m_papersToDownload.Count; i++)
		{
			if (m_papersToDownload[i].m_rootEntry == wallpaperEntry)
			{
				return true;
			}
		}
		return false;
	}

	private StoreContent.StoreEntry FindWallpaperStoreEntry(string wallpaperName)
	{
		for (int i = 0; i < m_wallpaperEntries.Count; i++)
		{
			StoreContent.StoreEntry storeEntry = m_wallpaperEntries[i];
			int rewardedWallpaperCount = GetRewardedWallpaperCount(storeEntry);
			if (rewardedWallpaperCount == 1 && !(storeEntry.m_wallpaperResources[0] != wallpaperName))
			{
				return storeEntry;
			}
		}
		return null;
	}

	private void CheckWallpapersToDownload()
	{
		if (m_papersToDownload.Count != 0)
		{
			bool flag = (m_state & State.Downloading) == State.Downloading;
			bool flag2 = (m_state & State.Awarding) == State.Awarding;
			if (!flag && !flag2)
			{
				StartCoroutine(s_instance.DownloadWallpaperBundles());
			}
		}
	}

	private void CheckWallpapersToAward()
	{
		if ((m_state & State.BlockAwards) != State.BlockAwards && (m_state & State.Downloading) != State.Downloading && m_papersToAward.Count != 0 && (m_state & State.Awarding) != State.Awarding)
		{
			StartCoroutine(UpdatePendingAwards());
		}
	}

	private void UpdateWallpaperVisibility(string[] wallpapers, GCState.Challenges challenge)
	{
		for (int i = 0; i < wallpapers.Length; i++)
		{
			StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(wallpapers[i], StoreContent.Identifiers.Name);
			bool flag = (storeEntry.m_state & StoreContent.StoreEntry.State.Purchased) == StoreContent.StoreEntry.State.Purchased;
			GCState.State state = GCState.ChallengeState(challenge);
			if (state == GCState.State.Finished || flag || StoreContent.ShowHiddenStoreItems)
			{
				storeEntry.m_state &= ~StoreContent.StoreEntry.State.Hidden;
			}
			else
			{
				storeEntry.m_state |= StoreContent.StoreEntry.State.Hidden;
			}
		}
	}

	private bool WallpaperExistsInEntry(string wallpaperName, StoreContent.StoreEntry storeEntry)
	{
		string[] wallpaperResources = storeEntry.m_wallpaperResources;
		for (int i = 0; i < wallpaperResources.Length; i++)
		{
			if (wallpaperResources[i] == wallpaperName)
			{
				return true;
			}
		}
		return false;
	}

	private int GetRewardedWallpaperCount(StoreContent.StoreEntry wallpaperEntry)
	{
		int num = 0;
		for (int i = 0; i < wallpaperEntry.m_wallpaperResources.Length && !string.IsNullOrEmpty(wallpaperEntry.m_wallpaperResources[i]); i++)
		{
			num++;
		}
		return num;
	}

	private int GetDownloadWallpaperCount(DownloadingWallpapers downloadEntry)
	{
		int num = 0;
		for (int i = 0; i < downloadEntry.m_awardedEntries.Length; i++)
		{
			if (downloadEntry.m_awardedEntries[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	private string FindFirstWallpaper(StoreContent.StoreEntry wallpaperEntry)
	{
		for (int i = 0; i < wallpaperEntry.m_wallpaperResources.Length; i++)
		{
			if (!string.IsNullOrEmpty(wallpaperEntry.m_wallpaperResources[i]))
			{
				return wallpaperEntry.m_wallpaperResources[i];
			}
		}
		return null;
	}

	private bool BundleWallpapersAllPurchased(StoreContent.StoreEntry storeEntry, List<StoreContent.StoreEntry> wallpaperEntries)
	{
		bool result = false;
		for (int i = 0; i < wallpaperEntries.Count; i++)
		{
			StoreContent.StoreEntry storeEntry2 = wallpaperEntries[i];
			int rewardedWallpaperCount = GetRewardedWallpaperCount(storeEntry2);
			if (rewardedWallpaperCount != 1)
			{
				continue;
			}
			string wallpaperName = storeEntry2.m_wallpaperResources[0];
			if (WallpaperExistsInEntry(wallpaperName, storeEntry))
			{
				if ((storeEntry2.m_state & StoreContent.StoreEntry.State.Purchased) != StoreContent.StoreEntry.State.Purchased)
				{
					return false;
				}
				result = true;
			}
		}
		return result;
	}

	private IEnumerator DownloadWallpaperBundles()
	{
		m_state |= State.Downloading;
		for (int i = 0; i < m_papersToDownload.Count; i++)
		{
			DownloadingWallpapers papersToDownload = m_papersToDownload[i];
			StoreContent.StoreEntry rootEntry = papersToDownload.m_rootEntry;
			int wallpaperDownloadCount = GetDownloadWallpaperCount(papersToDownload);
			int paperIndex = 0;
			int validEntryCount = 0;
			for (; paperIndex < papersToDownload.m_awardedEntries.Length; paperIndex++)
			{
				StoreContent.StoreEntry thisEntry = papersToDownload.m_awardedEntries[paperIndex];
				if (thisEntry != null)
				{
					string thisName = FindFirstWallpaper(thisEntry);
					string onlineBundlePath = BundleLocations.GetReferencedPath(thisName, "unity3d", "wallpapers");
					WWW wwwBundle = WWW.LoadFromCacheOrDownload(onlineBundlePath, 1);
					while (!wwwBundle.isDone && wwwBundle.error == null)
					{
						thisEntry.m_downloadProgress = wwwBundle.progress;
						rootEntry.m_downloadProgress = ((float)validEntryCount + wwwBundle.progress) / (float)wallpaperDownloadCount;
						yield return null;
					}
					validEntryCount++;
					if (wwwBundle.error != null)
					{
						papersToDownload.m_awardedEntries[paperIndex].m_downloadProgress = 0f;
						m_state |= State.Error;
						break;
					}
					papersToDownload.m_awardedEntries[paperIndex].m_downloadProgress = 1f;
					papersToDownload.m_loadedBundles[paperIndex] = wwwBundle.assetBundle;
				}
			}
			if ((m_state & State.Error) == State.Error)
			{
				rootEntry.m_downloadProgress = 0f;
			}
			else
			{
				rootEntry.m_downloadProgress = 1f;
			}
			if ((m_state & State.Error) == State.Error)
			{
				break;
			}
		}
		for (int i = 0; i < m_papersToDownload.Count; i++)
		{
			m_papersToAward.Add(m_papersToDownload[i]);
		}
		m_papersToDownload.Clear();
		m_state &= ~State.Downloading;
	}

	private IEnumerator UpdatePendingAwards()
	{
		m_state |= State.Awarding;
		bool errored = (m_state & State.Error) == State.Error;
		bool warnAboutAccess = true;
		for (int paperGroupIndex = 0; paperGroupIndex < m_papersToAward.Count; paperGroupIndex++)
		{
			DownloadingWallpapers thisDownload = m_papersToAward[paperGroupIndex];
			for (int awardIndex = 0; awardIndex < thisDownload.m_awardedEntries.Length; awardIndex++)
			{
				StoreContent.StoreEntry entryToAward = thisDownload.m_awardedEntries[awardIndex];
				if (entryToAward == null)
				{
					continue;
				}
				if (!errored)
				{
					AssetBundle thisBundle = thisDownload.m_loadedBundles[awardIndex];
					string wallpaperName = FindFirstWallpaper(entryToAward);
					if (FeatureSupport.IsLowEndDevice())
					{
						wallpaperName = $"{wallpaperName}-low";
					}
					AssetBundleRequest request = thisBundle.LoadAsync(wallpaperName, typeof(Texture2D));
					yield return request;
					Texture2D wallpaperTexture = request.asset as Texture2D;
					SLPlugin.SaveTextureToPhotoAlbum(wallpaperTexture, wallpaperName);
					warnAboutAccess = false;
				}
				if (thisDownload.m_loadedBundles[awardIndex] != null)
				{
					thisDownload.m_loadedBundles[awardIndex].Unload(unloadAllLoadedObjects: true);
					thisDownload.m_loadedBundles[awardIndex] = null;
				}
			}
		}
		yield return Resources.UnloadUnusedAssets();
		if (m_papersToDownload.Count == 0)
		{
			if (errored)
			{
				DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.WallpaperDownloadFail);
			}
			else
			{
				DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.WallpaperDownloadSuccess);
			}
		}
		for (int paperGroupIndex = 0; paperGroupIndex < m_papersToAward.Count; paperGroupIndex++)
		{
			DownloadingWallpapers thisDownload = m_papersToAward[paperGroupIndex];
			thisDownload.m_rootEntry.m_state &= ~StoreContent.StoreEntry.State.Downloading;
			for (int awardIndex = 0; awardIndex < thisDownload.m_awardedEntries.Length; awardIndex++)
			{
				StoreContent.StoreEntry entryToAward = thisDownload.m_awardedEntries[awardIndex];
				if (entryToAward != null)
				{
					entryToAward.m_state &= ~StoreContent.StoreEntry.State.Downloading;
				}
			}
		}
		m_papersToAward.Clear();
		m_state &= ~State.Error;
		m_state &= ~State.Awarding;
	}

	private void Event_MainMenuActive()
	{
		UpdateWallpaperVisibility(m_hiddenWallpapersGC3, GCState.Challenges.gc3);
		m_state &= ~State.BlockAwards;
	}

	private void Event_OnNewGameAboutToStart()
	{
		m_state |= State.BlockAwards;
	}

	private void Event_OnTransitionStarted()
	{
		m_state |= State.BlockAwards;
	}

	private void Event_OnGameFinished()
	{
		UpdateWallpaperVisibility(m_hiddenWallpapersGC3, GCState.Challenges.gc3);
		m_state &= ~State.BlockAwards;
	}
}
