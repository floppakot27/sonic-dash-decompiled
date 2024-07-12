using System.Collections.Generic;
using UnityEngine;

public class BossVisualController : MonoBehaviour
{
	public enum Part
	{
		Boss,
		Vehicle,
		All
	}

	private Renderer[] m_renderers;

	private Renderer m_actualBossRenderer;

	private Renderer m_bossVehicleRenderer;

	private bool m_currentlyVisible = true;

	private float m_flashTimer;

	private Part m_flashPart = Part.All;

	[SerializeField]
	private ParticleSystem m_SpawnMineEffect;

	[SerializeField]
	private ParticleSystem m_SpawnMissleEffect;

	[SerializeField]
	private List<ParticleSystem> m_AttackImpactEffects;

	[SerializeField]
	private float m_flashFrequency = 8f;

	[SerializeField]
	private float m_flashTimePerHit = 0.5f;

	public bool Visible { get; set; }

	public void PlaySpawnMineEffect()
	{
		if (m_SpawnMineEffect != null)
		{
			m_SpawnMineEffect.Play();
		}
	}

	public void PlaySpawnMissileEffect()
	{
		if (m_SpawnMissleEffect != null)
		{
			m_SpawnMissleEffect.Play();
		}
	}

	public void PlayAttackImpactEffect(int attackIndex)
	{
		m_AttackImpactEffects[attackIndex].Play();
	}

	public void Flash(Part part)
	{
		m_flashTimer = m_flashTimePerHit;
		m_flashPart = part;
	}

	private void Awake()
	{
		m_renderers = GetComponentsInChildren<Renderer>();
		GameObject gameObject = Utils.FindTagInChildren(base.gameObject, "BossRenderer");
		m_actualBossRenderer = gameObject.GetComponent<Renderer>();
		GameObject gameObject2 = Utils.FindTagInChildren(base.gameObject, "BossVehicleRenderer");
		m_bossVehicleRenderer = gameObject2.GetComponent<Renderer>();
		SetVisibility(visible: false);
		Visible = false;
	}

	private void LateUpdate()
	{
		if (Visible != m_currentlyVisible)
		{
			SetVisibility(Visible);
		}
		UpdateFlash();
	}

	private void UpdateFlash()
	{
		m_flashTimer -= IndependantTimeDelta.Delta;
		if (m_flashTimer > 0f)
		{
			int num = Mathf.FloorToInt(m_flashTimer * m_flashFrequency * 2f);
			if (num % 2 == 0)
			{
				SetPartVisibility(m_flashPart, visible: true);
			}
			else
			{
				SetPartVisibility(m_flashPart, visible: false);
			}
		}
		else
		{
			SetPartVisibility(Part.All, m_currentlyVisible);
		}
	}

	private void SetPartVisibility(Part part, bool visible)
	{
		switch (part)
		{
		case Part.Boss:
			m_actualBossRenderer.enabled = visible;
			break;
		case Part.Vehicle:
			m_bossVehicleRenderer.enabled = visible;
			break;
		case Part.All:
			m_actualBossRenderer.enabled = visible;
			m_bossVehicleRenderer.enabled = visible;
			break;
		}
	}

	private void SetVisibility(bool visible)
	{
		for (int i = 0; i < m_renderers.Length; i++)
		{
			m_renderers[i].enabled = visible;
		}
		m_currentlyVisible = visible;
	}
}
