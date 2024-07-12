using UnityEngine.SocialPlatforms.Impl;

public class HLUserProfile : UserProfile
{
	public enum ProfileSource
	{
		Facebook,
		GameCenter,
		GooglePlay,
		Multiple,
		Max
	}

	public ProfileSource Source { get; private set; }

	public HLUserProfile()
	{
		Source = ProfileSource.Max;
	}

	public void SetSource(ProfileSource src)
	{
		Source = src;
	}
}
