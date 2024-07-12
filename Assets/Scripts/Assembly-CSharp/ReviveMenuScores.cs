using System.Collections;
using UnityEngine;

public class ReviveMenuScores : MonoBehaviour
{
	[SerializeField]
	private UILabel m_friendScore;

	[SerializeField]
	private UILabel m_friendRank;

	[SerializeField]
	private UILabel m_friendName;

	[SerializeField]
	private UITexture m_friendImage;

	[SerializeField]
	private Texture2D m_defaultFriendImage;

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void DisplayFriendProperties()
	{
		Leaderboards.Entry entry = HudContent_FriendDisplay.CurrentFriend();
		if (entry == null)
		{
			return;
		}
		if ((bool)m_friendName)
		{
			m_friendName.text = entry.m_user;
		}
		if ((bool)m_friendScore)
		{
			m_friendScore.text = LanguageUtils.FormatNumber(entry.m_score);
		}
		if ((bool)m_friendRank)
		{
			m_friendRank.text = entry.m_rank.ToString();
		}
		if ((bool)m_friendImage)
		{
			if ((bool)entry.m_avatar)
			{
				m_friendImage.mainTexture = entry.m_avatar;
			}
			else
			{
				m_friendImage.mainTexture = m_defaultFriendImage;
			}
		}
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		DisplayFriendProperties();
	}
}
