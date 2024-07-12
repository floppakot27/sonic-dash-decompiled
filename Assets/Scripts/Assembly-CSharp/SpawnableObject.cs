using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnableObject : MonoBehaviour
{
	public delegate void OnEvent(SpawnableObject trackObject);

	[method: MethodImpl(32)]
	private event OnEvent m_onDeathDelegates;

	public void RequestDestruction()
	{
		DestroySelf();
	}

	public virtual void Place(OnEvent onDestroy, Track onTrack, Spline onSpline)
	{
		Place(onDestroy);
	}

	protected virtual void DestroySelf()
	{
		if (this.m_onDeathDelegates != null)
		{
			this.m_onDeathDelegates(this);
		}
	}

	protected void Place(OnEvent onDestroy)
	{
		this.m_onDeathDelegates = (OnEvent)Delegate.Combine(this.m_onDeathDelegates, onDestroy);
	}
}
