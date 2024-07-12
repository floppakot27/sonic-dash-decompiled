using UnityEngine;

public abstract class RingBounce
{
	protected string m_source = string.Empty;

	public string Source
	{
		set
		{
			m_source = value;
		}
	}

	public RingBounce()
	{
		InitialiseBounce();
	}

	public void Reset(float currentSpeed, Vector3 forward)
	{
		ResetBounce(currentSpeed, forward);
	}

	public void Update()
	{
		UpdateBounce();
	}

	public void Enable(bool enable)
	{
		EnableBounce(enable);
	}

	public Vector3 Position()
	{
		return GetBouncePosition();
	}

	protected abstract void InitialiseBounce();

	protected abstract void ResetBounce(float currentSpeed, Vector3 forward);

	protected abstract void UpdateBounce();

	protected abstract void EnableBounce(bool enable);

	protected abstract Vector3 GetBouncePosition();
}
