using System.Collections;
using UnityEngine;

[AddComponentMenu("Dash/Track/Spring")]
public class Spring : SpawnableObject
{
	private Collider m_collider;

	private SpringTV m_tv;

	[SerializeField]
	private ParticleSystem m_boosterParticles;

	public void Awake()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		m_collider = GetComponentInChildren<Collider>();
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterInterest("OnNewGameStarted", this);
	}

	public void OnCollision()
	{
		if (m_collider != null)
		{
			m_collider.enabled = false;
		}
	}

	public void SetTV(SpringTV tv)
	{
		m_tv = tv;
		tv.transform.parent = base.gameObject.transform;
	}

	public SpringTV.Type GetSpringType()
	{
		return m_tv.SpringType;
	}

	public SpringTV.Destination GetDestination()
	{
		return m_tv.SpringDestination;
	}

	public SpringTV.CreateFlags GetCreateFlags()
	{
		return m_tv.SpringCreateFlags;
	}

	private void OnEnable()
	{
		m_collider.enabled = true;
		StartCoroutine(SetBoosterAura());
	}

	private void OnSpawned()
	{
		m_collider.enabled = true;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(SetBoosterAura());
		}
	}

	private void Event_OnNewGameStarted()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(SetBoosterAura());
		}
	}

	private IEnumerator SetBoosterAura()
	{
		yield return null;
		if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_SpringBonus))
		{
			m_boosterParticles.gameObject.SetActive(value: true);
			m_boosterParticles.Play();
		}
		else
		{
			m_boosterParticles.Stop();
			m_boosterParticles.Clear();
			m_boosterParticles.gameObject.SetActive(value: false);
		}
	}
}
