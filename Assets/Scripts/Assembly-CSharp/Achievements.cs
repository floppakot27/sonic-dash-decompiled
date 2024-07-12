using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Achievements
{
	public enum Types
	{
		SonicRampage,
		RingHoarder,
		PowerOverload,
		KnuclesOnTheMove,
		CaChing,
		SkyIsTheLimit,
		Ringmaster,
		SuperSonic,
		MissionMaster,
		OnARoll,
		ActionPacked,
		HotHeels,
		SEGAMember,
		EasyTarget,
		WarmUp
	}

	[Flags]
	private enum State
	{
		None = 0,
		PendingRequest = 1,
		LoadingAchievements = 2
	}

	public static string[] Names = new string[15]
	{
		"CgkI-9_QnpoeEAIQAQ", "CgkI-9_QnpoeEAIQAg", "CgkI-9_QnpoeEAIQAw", "CgkI-9_QnpoeEAIQBA", "CgkI-9_QnpoeEAIQBQ", "CgkI-9_QnpoeEAIQBg", "CgkI-9_QnpoeEAIQBw", "CgkI-9_QnpoeEAIQCA", "CgkI-9_QnpoeEAIQCQ", "CgkI-9_QnpoeEAIQCg",
		"CgkI-9_QnpoeEAIQCw", "CgkI-9_QnpoeEAIQDA", "CgkI-9_QnpoeEAIQDQ", "CgkI-9_QnpoeEAIQDg", "CgkI-9_QnpoeEAIQDw"
	};

	private static Achievements s_singleton;

	private IAchievement[] m_achievements;

	private State m_state;

	public Achievements()
	{
		CreateAchievementInstances();
		RegisterForEvents();
		s_singleton = this;
	}

	public void LoadAchievements()
	{
		if ((m_state & State.LoadingAchievements) == State.LoadingAchievements)
		{
			m_state |= State.PendingRequest;
			return;
		}
		m_state |= State.LoadingAchievements;
		Social.LoadAchievements(delegate(IAchievement[] result)
		{
			OnAchievementsLoaded(result);
		});
	}

	public static void AwardAchievement(Types achievement, float progress, ref AchievementTracker.Achievement currentAchievement)
	{
		if (!Social.localUser.authenticated)
		{
			return;
		}
		IAchievement achievement2 = s_singleton.FindAchievement(achievement, s_singleton.m_achievements);
		if (achievement2.percentCompleted == 100.0 || achievement2.completed || achievement2.percentCompleted >= (double)progress)
		{
			return;
		}
		achievement2.percentCompleted = progress;
		bool completed = false;
		achievement2.ReportProgress(delegate(bool result)
		{
			if (result && progress >= 100f)
			{
				completed = true;
			}
		});
		if (completed)
		{
			currentAchievement.m_state |= AchievementTracker.Achievement.State.Completed;
		}
	}

	private void CreateAchievementInstances()
	{
		int enumCount = Utils.GetEnumCount<Types>();
		m_achievements = new IAchievement[enumCount];
		for (int i = 0; i < enumCount; i++)
		{
			string id = Names[i];
			m_achievements[i] = Social.CreateAchievement();
			m_achievements[i].id = id;
			m_achievements[i].percentCompleted = 0.0;
		}
	}

	private void OnAchievementsLoaded(IAchievement[] currentAchievements)
	{
		for (int i = 0; i < m_achievements.Length; i++)
		{
			IAchievement achievement = FindAchievement((Types)i, currentAchievements);
			if (achievement != null)
			{
				m_achievements[i] = achievement;
			}
		}
		m_state &= ~State.LoadingAchievements;
		if ((m_state & State.PendingRequest) == State.PendingRequest)
		{
			m_state &= ~State.PendingRequest;
			LoadAchievements();
		}
	}

	public void Event_OnAndroidAchievementsFetched()
	{
		int enumCount = Utils.GetEnumCount<Types>();
		for (int i = 0; i < enumCount; i++)
		{
			m_achievements[i].percentCompleted = HLSocialPluginAndroid.GetAchievementProgress(m_achievements[i].id);
		}
		m_state &= ~State.LoadingAchievements;
		if ((m_state & State.PendingRequest) == State.PendingRequest)
		{
			m_state &= ~State.PendingRequest;
			LoadAchievements();
		}
	}

	private void RegisterForEvents()
	{
		EventDispatch.RegisterInterest("RequestAchievementDisplay", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnAndroidAchievementsFetched", this);
	}

	private IAchievement FindAchievement(Types achievement, IAchievement[] achievementList)
	{
		if (achievementList == null)
		{
			return null;
		}
		string text = Names[(int)achievement];
		foreach (IAchievement achievement2 in achievementList)
		{
			if (achievement2.id == text)
			{
				return achievement2;
			}
		}
		return null;
	}

	private void Event_RequestAchievementDisplay()
	{
		Social.ShowAchievementsUI();
	}
}
