using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineExtensions;

public class MenuTriggers : MonoBehaviour
{
	private void Trigger_StartGame()
	{
		StartCoroutine(StartGameSequence());
	}

	private void Trigger_ResumeGame()
	{
		GameState.RequestMode(GameState.Mode.Game);
	}

	private void Trigger_RestartGame()
	{
		GameState.RequestReset(GameState.Mode.Game);
		DCs.PreparePiecesToSave(normal: false);
		DCs.SetChallengePieceSpawn(spawn: false);
	}

	private void Trigger_QuitGame()
	{
		GameState.RequestReset(GameState.Mode.Menu);
		DCs.PreparePiecesToSave(normal: false);
		DCs.SetChallengePieceSpawn(spawn: false);
	}

	private void Trigger_QuitWithSpecifiedPage(GameObject callerObject)
	{
		if (!TutorialSystem.instance().isTrackTutorialEnabled())
		{
			MoveToPageProperties component = callerObject.GetComponent<MoveToPageProperties>();
			MenuStack.RequestPage = component.DestinationPage;
			Trigger_QuitGame();
		}
		else
		{
			Trigger_RestartGame();
		}
	}

	private void Trigger_CloseMenuSystem()
	{
		MenuStack component = GetComponent<MenuStack>();
		component.CloseMenuSystem();
	}

	private void Trigger_MoveToPage(GameObject callerObject)
	{
		GuiTrigger menuPage = null;
		iTweenPath transitionPath = null;
		bool transitionPathInReverse = false;
		bool replaceCurrentPage = false;
		if (callerObject != null)
		{
			MoveToPageProperties component = callerObject.GetComponent<MoveToPageProperties>();
			if (component != null)
			{
				menuPage = component.DestinationPage;
				transitionPath = component.TransitionPath;
				transitionPathInReverse = component.TransitionPathInReverse;
				replaceCurrentPage = component.ReplaceCurrentPage;
			}
		}
		MenuStack component2 = GetComponent<MenuStack>();
		component2.MoveToPage(menuPage, transitionPath, transitionPathInReverse, replaceCurrentPage);
	}

	private void Trigger_ToggleSFXVolume(bool ticked)
	{
		Audio.SFXEnabled = ticked;
	}

	private void Trigger_ToggleMusicVolume(bool ticked)
	{
		Audio.MusicEnabled = ticked;
	}

	private void Trigger_ToggleTutorial(bool ticked)
	{
		bool flag = false;
		if (ticked)
		{
			TutorialSystem.instance().Enable();
		}
		else
		{
			TutorialSystem.instance().Disable();
		}
		if (!flag)
		{
			return;
		}
		GameObject gameObject = GameObject.Find("TrackGenerator");
		if ((bool)gameObject)
		{
			TrackGenerator component = gameObject.GetComponent<TrackGenerator>();
			if (!component)
			{
			}
		}
		DialogStack.ShowDialog("TutorialPleaseWait");
	}

	private void Trigger_ResetTutorial()
	{
		Boosters.ClearSelected();
		TutorialSystem.instance().Enable();
		GameState.RequestReset(GameState.Mode.Menu);
	}

	private void Trigger_SaveGame()
	{
		PropertyStore.Save();
	}

	private void Trigger_RestorePurchases()
	{
		DialogStack.ShowDialog("Restore Purchase Warning");
	}

	private void Trigger_RestorePurchasesConfirmed()
	{
		StorePurchases.RestorePurchases();
	}

	private void Trigger_CameraTransition(GameObject callerObject)
	{
		iTweenPath transitionPath = null;
		float transitionTime = 0f;
		if (callerObject != null)
		{
			CameraTransitionProperties component = callerObject.GetComponent<CameraTransitionProperties>();
			if (component != null)
			{
				transitionPath = component.TransitionPath;
				transitionTime = component.TransitionTime;
			}
		}
		MenuCamera component2 = GetComponent<MenuCamera>();
		component2.TransitionTime = transitionTime;
		component2.StartCameraTransition(transitionPath, iTween.EaseType.easeInQuart, iTween.EaseType.linear, CameraTypeSpline.Direction.Forward);
	}

	private void Trigger_DeleteSaveData()
	{
		WheelOfFortuneSettings.Instance.Reset();
		PropertyStore.Reset();
		PropertyStore.Save();
	}

	private void Trigger_DeleteBundleCache()
	{
		Caching.CleanCache();
	}

	private void Trigger_DisplayAchievements()
	{
		EventDispatch.GenerateEvent("RequestAchievementDisplay");
	}

	private void Trigger_DisplayLeaderboards()
	{
		EventDispatch.GenerateEvent("RequestLeaderboardDisplay");
	}

	private void Trigger_PrivacySelected()
	{
		string urlStr = "http://www.sega.com/mprivacy";
		if (Language.GetLanguage() == Language.ID.Japanese || Language.GetLocale() == Language.Locale.Japan)
		{
			urlStr = "http://sega.jp/privacypolicy/";
		}
		SLWebView.OpenWebWindow(urlStr);
	}

	private void Trigger_RequestContinue()
	{
		EventDispatch.GenerateEvent("OnRequestContinue");
	}

	private void Trigger_CancelContinue()
	{
		EventDispatch.GenerateEvent("OnCancelContinue");
	}

	private void Trigger_ShowMoreGames()
	{
		if (SLAds.IsMoreGamesAvailable())
		{
			SLAds.ShowMoreGames();
		}
	}

	private void Trigger_ShowGameOffers()
	{
		if (SLAds.IsGameOffersAvailable())
		{
			SLAds.ShowGameOffers();
		}
	}

	private void Trigger_LogIntoSegaID()
	{
		if (!SLSegaID.IsLoggedIn())
		{
			GameUnloader.DoWhileSafe(SLSegaID.Login, GameUnloader.ReloadTrigger.Unpaused);
		}
	}

	private void Trigger_OpenGlobalChallenge()
	{
		if (Internet.CheckNetworkConnection())
		{
			GCState.GCButtonPressed(internet: true);
			SLWebView.OpenWebWindow("http://www.sonicthehedgehog.com/dash");
		}
		else
		{
			GCState.GCButtonPressed(internet: false);
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.GlobalChallengeNoConnection);
		}
		PropertyStore.Save();
	}

	private void Trigger_GC2PageVisitedFromResult()
	{
		GC3Progress.PageVisitedFromResult = true;
	}

	private void Trigger_ToggleInvincibility(bool ticked)
	{
		SonicSplineTracker sonicSplineTracker = Object.FindObjectOfType(typeof(SonicSplineTracker)) as SonicSplineTracker;
		sonicSplineTracker.Invulnerable = ticked;
	}

	private void Trigger_ToggleHUD(bool ticked)
	{
		HudDisplay.DisplayHud = !ticked;
	}

	private void Trigger_ToggleGameplayOrder(bool ticked)
	{
		GameplayTemplateGenerator.IsOrderedGameplayEnabled = ticked;
	}

	private void Trigger_ToggleHiddenStoreItems(bool ticked)
	{
		StoreContent.ShowHiddenStoreItems = ticked;
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		for (int i = 0; i < stockList.Count; i++)
		{
			StoreContent.StoreEntry storeEntry = stockList[i];
			StoreContent.ValidateEntry(storeEntry);
		}
	}

	private void Trigger_ToggleHighlightUnlocalisedText(bool ticked)
	{
		EventDispatch.GenerateEvent("EnableUnlocalisedTextHighlight", ticked);
	}

	private void Trigger_EnterShopAnalyticsMain()
	{
		GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.ShopMainMenu);
		GameAnalytics.EnterShop();
	}

	private void Trigger_EnterShopAnalyticsFreeRingsButton()
	{
		GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.FreeRingsButton);
		GameAnalytics.EnterShop();
	}

	private void Trigger_EnterShopAnalyticsPause()
	{
		GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.ShopPauseScreen);
		GameAnalytics.EnterShop();
	}

	private void Trigger_EnterShopAnalyticsResult()
	{
		GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.ShopResultScreen);
		GameAnalytics.EnterShop();
	}

	private void Trigger_EnterShopAnalyticsTop()
	{
		GameAnalytics.SetPurchaseLocation(GameAnalytics.GetPurchaseLocation() + GameAnalytics.LocationsTop);
		GameAnalytics.EnterShop();
	}

	private void Trigger_SetFacebookLoginLocation_Popup()
	{
		GameAnalytics.SetFacebookLocation(GameAnalytics.FacebookLoginLocations.Popup);
	}

	private void Trigger_SetFacebookLoginLocation_LeaderboardMenu()
	{
		GameAnalytics.SetFacebookLocation(GameAnalytics.FacebookLoginLocations.LeaderboardMenu);
	}

	private void Trigger_SetFacebookLoginLocation_LeaderboardResult()
	{
		GameAnalytics.SetFacebookLocation(GameAnalytics.FacebookLoginLocations.LeaderboardResult);
	}

	private void Trigger_SetLanguage_US()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.English_US);
	}

	private void Trigger_SetLanguage_G()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.German);
	}

	private void Trigger_SetLanguage_F()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.French);
	}

	private void Trigger_SetLanguage_I()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.Italian);
	}

	private void Trigger_SetLanguage_S()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.Spanish);
	}

	private void Trigger_SetLanguage_Ru()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.Russian);
	}

	private void Trigger_SetLanguage_Br()
	{
		LanguageDebugging.Debugger.ForceLanguage(Language.ID.Portuguese_Brazil);
	}

	private IEnumerator StartGameSequence()
	{
		CharacterManager.Singleton.Load();
		while (CharacterManager.Singleton.Loading)
		{
			yield return null;
		}
		if (Sonic.Tracker.InternalTracker == null)
		{
			yield return StartCoroutine(Sonic.Tracker.WaitForCleanTrack());
		}
		MenuTriggers owner = this;
		IEnumerator[] array = new IEnumerator[1];
		object obj;
		if ((bool)Sonic.MenuAnimationControl)
		{
			IEnumerator enumerator = Sonic.MenuAnimationControl.GameIntroSequence();
			obj = enumerator;
		}
		else
		{
			obj = null;
		}
		array[0] = (IEnumerator)obj;
		yield return owner.JoinCoroutines(array);
		GameState.RequestMode(GameState.Mode.Game);
	}
}
