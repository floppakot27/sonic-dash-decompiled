using UnityEngine.SocialPlatforms.Impl;

public class HLLocalUser : LocalUser
{
	public bool isFacebookAuthenticated { get; private set; }

	public string FacebookUserName { get; private set; }

	public string FacebookUserID { get; private set; }

	public bool isGameCenterAuthenticated { get; private set; }

	public string GameCenterUserName { get; private set; }

	public string GameCenterUserID { get; private set; }

	public void setFacebookAuthenticated(bool authenticated)
	{
		if (authenticated != isFacebookAuthenticated)
		{
			isFacebookAuthenticated = authenticated;
			if (isFacebookAuthenticated)
			{
				SetAuthenticated(value: true);
				EventDispatch.GenerateEvent("OnFacebookLogin");
			}
			else
			{
				SetAuthenticated(isGameCenterAuthenticated);
			}
		}
	}

	public void setFacebookUserInfo(string userId, string userName)
	{
		FacebookUserID = userId;
		FacebookUserName = userName;
	}

	public void setGameCenterAuthenticated(bool authenticated)
	{
		if (authenticated != isGameCenterAuthenticated)
		{
			isGameCenterAuthenticated = authenticated;
			if (isGameCenterAuthenticated)
			{
				SetAuthenticated(value: true);
				EventDispatch.GenerateEvent("OnGameCenterLogin");
			}
			else
			{
				SetAuthenticated(isFacebookAuthenticated);
			}
		}
	}

	public void setGameCenterUserInfo(string userId, string userName)
	{
		GameCenterUserID = userId;
		GameCenterUserName = userName;
	}

	public bool matchesUserID(string userID)
	{
		if (isFacebookAuthenticated && userID.Equals(FacebookUserID))
		{
			return true;
		}
		if (isGameCenterAuthenticated && userID.Equals(GameCenterUserID))
		{
			return true;
		}
		return false;
	}
}
