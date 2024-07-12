using UnityEngine;

public class GCCollectable : SpawnableObject
{
	[SerializeField]
	public int m_collectableNumber;

	private ParticleSystem m_particleSystem;

	[SerializeField]
	private AudioClip m_audio;

	public void Awake()
	{
		m_particleSystem = GetComponentInChildren<ParticleSystem>();
	}

	public override void Place(OnEvent onDestroy, Track track, Spline spline)
	{
		base.Place(onDestroy, track, spline);
		base.transform.position += 1f * base.transform.up;
		Activate();
	}

	public void notifyCollection()
	{
		m_particleSystem.Stop();
		m_particleSystem.Clear();
		GC3Progress.GCCollectableCollected();
		EventDispatch.GenerateEvent("GCCollectableCollected", m_collectableNumber);
		Sonic.RenderManager.playGCCCollectedParticles();
		Debug.Log("TODO : CHANGE AUDIO");
		Audio.PlayClip(m_audio, loop: false);
		Deactivate();
	}

	private void Activate()
	{
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer), includeInactive: false);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = true;
		}
		Component[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			((Collider)componentsInChildren2[j]).enabled = true;
		}
	}

	private void Deactivate()
	{
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer), includeInactive: false);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = false;
		}
		Component[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			((Collider)componentsInChildren2[j]).enabled = false;
		}
	}
}
