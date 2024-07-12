using System.Collections;
using UnityEngine;

[AddComponentMenu("Dash/Sonic/Audio Control")]
public class SonicAudioControl : MonoBehaviour
{
	public enum MaterialType
	{
		Invalid = -1,
		Grass,
		Stone
	}

	[SerializeField]
	private AudioClip m_rollSpinUpAudioClip;

	[SerializeField]
	private AudioClip m_rollGoAudioClip;

	[SerializeField]
	private AudioClip m_enemyCollisionClip;

	[SerializeField]
	private AudioClip m_jumpAudioClip;

	[SerializeField]
	private AudioClip m_diveSlamAudioClipGrass;

	[SerializeField]
	private AudioClip m_diveSlamAudioClipStone;

	[SerializeField]
	private AudioClip m_hopAudioClip;

	[SerializeField]
	private AudioClip m_strafeAudioClip;

	private Transform m_dashActive;

	[SerializeField]
	private AudioClip m_dashOnceAudioClip;

	[SerializeField]
	private AudioClip m_dashLoopedAudioClip;

	private MaterialType m_nMtrlType = MaterialType.Invalid;

	private AudioClipManager m_currentFootstepsSfx;

	[SerializeField]
	private AudioClipManager m_RunningOnGrass;

	[SerializeField]
	private AudioClipManager m_RunningOnStone;

	[SerializeField]
	private AudioClip m_HomingAttackAudioClip;

	[SerializeField]
	private AudioClip m_DamageAudioClip;

	[SerializeField]
	private AudioClip m_respawnAudioClip;

	[SerializeField]
	private AudioClip m_powerUpPickUpMagnetAudioClip;

	[SerializeField]
	private AudioClip m_powerUpPickUpShieldAudioClip;

	[SerializeField]
	private AudioClip m_powerUpPickUpRingsAudioClip;

	[SerializeField]
	private AudioClip m_springAudioClip;

	[SerializeField]
	private AudioClip m_boosterSpringAudioClip;

	private bool m_onSetPiece;

	[SerializeField]
	private AudioClip m_setPieceDashStart;

	[SerializeField]
	private AudioClip m_setPieceDashLoop;

	[SerializeField]
	private AudioClip m_setPieceDashPad;

	private int m_nextSFXToPlay;

	[SerializeField]
	private AudioClip[] m_springGestureSfx;

	[SerializeField]
	private AudioClip m_springGestureSuccess;

	private void Start()
	{
		EventDispatch.RegisterInterest("OnSonicFall", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSingleSpringGestureSuccess", this);
		EventDispatch.RegisterInterest("SpringGestureSuccess", this);
		m_nextSFXToPlay = 0;
		m_currentFootstepsSfx = null;
		m_onSetPiece = false;
	}

	private void Update()
	{
		if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
		{
			if (m_dashActive == null)
			{
				Audio.PlayClip(m_dashOnceAudioClip, loop: false);
				m_dashActive = Audio.PlayClip(m_dashLoopedAudioClip, loop: true);
			}
		}
		else
		{
			if ((bool)m_dashActive)
			{
				Audio.Stop(m_dashLoopedAudioClip);
			}
			m_dashActive = null;
		}
		if (m_currentFootstepsSfx != null)
		{
			m_currentFootstepsSfx.Update();
		}
	}

	public void SetMaterialType(MaterialType mtrlType)
	{
		if (m_nMtrlType != mtrlType && m_currentFootstepsSfx != null)
		{
			StartSprintAudio(mtrlType);
		}
		m_nMtrlType = mtrlType;
	}

	public void StartSprintAudio(MaterialType mtrlType)
	{
		if (m_currentFootstepsSfx != null)
		{
			m_currentFootstepsSfx.EndPlaying();
		}
		switch (mtrlType)
		{
		case MaterialType.Grass:
			m_currentFootstepsSfx = m_RunningOnGrass;
			break;
		case MaterialType.Stone:
			m_currentFootstepsSfx = m_RunningOnStone;
			break;
		}
		m_currentFootstepsSfx.BeginPlaying();
	}

	public void SetSprintAudioFrequencyMultiplier(float fMultiplier)
	{
		if (m_currentFootstepsSfx != null)
		{
			m_currentFootstepsSfx.SetFrequencyMultiplier(fMultiplier);
		}
	}

	public void StopSprintAudio()
	{
		if (m_currentFootstepsSfx != null)
		{
			m_currentFootstepsSfx.EndPlaying();
			m_currentFootstepsSfx = null;
		}
	}

	public void PlayRollSpinUpSFX()
	{
		Audio.PlayClip(m_rollSpinUpAudioClip, loop: false);
	}

	public void PlayRollGoSFX()
	{
		Audio.PlayClip(m_rollGoAudioClip, loop: false);
	}

	public void StopRollSpinUpSFX()
	{
		Audio.Stop(m_rollSpinUpAudioClip);
	}

	private void PlayJumpSFX()
	{
		Audio.PlayClip(m_jumpAudioClip, loop: false);
	}

	public void PlayDeathSFX()
	{
		Audio.PlayClip(m_enemyCollisionClip, loop: false);
	}

	public void PlayRespawnSFX()
	{
		Audio.PlayClip(m_respawnAudioClip, loop: false);
	}

	public void PlayPowerUpMagnetPickupSFX()
	{
		Audio.PlayClip(m_powerUpPickUpMagnetAudioClip, loop: false);
	}

	public void PlayPowerUpShieldPickupSFX()
	{
		Audio.PlayClip(m_powerUpPickUpShieldAudioClip, loop: false);
	}

	public void PlayPowerUpRingsPickupSFX()
	{
		Audio.PlayClip(m_powerUpPickUpRingsAudioClip, loop: false);
	}

	public void PlaySpringSFX()
	{
		Audio.StopAll();
		if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_SpringBonus))
		{
			Audio.PlayClip(m_boosterSpringAudioClip, loop: false);
		}
		else
		{
			Audio.PlayClip(m_springAudioClip, loop: false);
		}
	}

	private void OnEnterSetPiece()
	{
		if (!m_onSetPiece)
		{
			Audio.PlayClip(m_dashOnceAudioClip, loop: false);
			Audio.PlayClip(m_setPieceDashPad, loop: false);
			StartCoroutine(SetPieceDashLoop());
			m_onSetPiece = true;
		}
	}

	private IEnumerator SetPieceDashLoop()
	{
		Transform dashStartSource = Audio.PlayClip(m_setPieceDashStart, loop: false);
		while (Audio.IsPlaying(dashStartSource) || Audio.Paused)
		{
			yield return null;
		}
		Audio.PlayClip(m_setPieceDashLoop, loop: true);
	}

	private void OnLeftSetPiece()
	{
		if (m_onSetPiece)
		{
			StopAllCoroutines();
			Audio.Stop(m_setPieceDashStart);
			Audio.Stop(m_setPieceDashLoop);
			m_onSetPiece = false;
		}
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		m_nextSFXToPlay = 0;
	}

	private void Event_OnSingleSpringGestureSuccess(CartesianDir dir)
	{
		if (m_nextSFXToPlay < m_springGestureSfx.Length)
		{
			Audio.PlayClip(m_springGestureSfx[m_nextSFXToPlay], loop: false);
		}
		m_nextSFXToPlay++;
		if (m_nextSFXToPlay == m_springGestureSfx.Length)
		{
			m_nextSFXToPlay = 0;
		}
	}

	private void Event_SpringGestureSuccess()
	{
		Audio.PlayClip(m_springGestureSuccess, loop: false);
	}

	private void Event_OnSpringEnd()
	{
		OnSlam();
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		StopSprintAudio();
		m_onSetPiece = false;
	}

	public void Event_OnSonicFall()
	{
	}

	public void StartJumpAnim(Pair<float, float> SpinUpTimeTargetVelocity)
	{
		PlayJumpSFX();
	}

	public void OnSlam()
	{
		switch (m_nMtrlType)
		{
		case MaterialType.Grass:
			Audio.PlayClip(m_diveSlamAudioClipGrass, loop: false);
			break;
		case MaterialType.Stone:
			Audio.PlayClip(m_diveSlamAudioClipStone, loop: false);
			break;
		}
	}

	public void PlayHopSFX()
	{
		Audio.PlayClip(m_hopAudioClip, loop: false);
	}

	public void PlayStrafeSFX()
	{
		Audio.PlayClip(m_strafeAudioClip, loop: false);
	}

	public void StartAttackAnim(Pair<float, float> SpinUpTimeTargetVelocity)
	{
		Audio.PlayClip(m_HomingAttackAudioClip, loop: false);
	}

	public void StartStumbleAnim()
	{
		Audio.PlayClip(m_DamageAudioClip, loop: false);
	}
}
