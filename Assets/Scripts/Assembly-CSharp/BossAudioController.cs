using System.Reflection;
using UnityEngine;

public class BossAudioController : MonoBehaviour
{
	[SerializeField]
	private LocalisedAudioClip m_introTauntSFX;

	[SerializeField]
	private LocalisedAudioClip m_mineTransitionSFX;

	[SerializeField]
	private LocalisedAudioClip m_flyOffSFX;

	[SerializeField]
	private LocalisedAudioClip m_hitCharacterSFX;

	[SerializeField]
	private AudioClip m_starFiringSFX;

	[SerializeField]
	private AudioClip m_qteHitDamage;

	[SerializeField]
	private LocalisedAudioClip m_qteStartSFX;

	[SerializeField]
	private AudioClip m_qteBossAttackSFX;

	[SerializeField]
	private AudioClip m_qteGestureFailSFX;

	[SerializeField]
	private LocalisedAudioClip m_qteHit1SFX;

	[SerializeField]
	private LocalisedAudioClip m_qteHit2SFX;

	[SerializeField]
	private LocalisedAudioClip m_qteHit3SFX;

	[SerializeField]
	private AudioClip m_qteBossDefeatSFX;

	[SerializeField]
	private LocalisedAudioClip m_qteBossDefeatedVO;

	[SerializeField]
	private LocalisedAudioClip m_qteBossEscapeVO;

	[SerializeField]
	private float m_voiceOverVolumeMutliplier;

	private void OnEnable()
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		int num = fields.Length;
		for (int i = 0; i < num; i++)
		{
			FieldInfo fieldInfo = fields[i];
			if (fieldInfo.FieldType == typeof(LocalisedAudioClip))
			{
				WarmupAudioClip(((LocalisedAudioClip)fieldInfo.GetValue(this)).GetAudioClip());
			}
		}
	}

	private void WarmupAudioClip(AudioClip clip)
	{
		if (clip != null)
		{
			Audio.PlayClipOverrideVolumeModifier(clip, loop: false, 0f);
			Audio.Stop(clip);
		}
	}

	public void PlayIntroTauntSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_introTauntSFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayMineTransitionSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_mineTransitionSFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayFlyOffSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_flyOffSFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayHitCharacterSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_hitCharacterSFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayStarFiringSFX()
	{
		Audio.PlayClip(m_starFiringSFX, loop: false);
	}

	public void PlayQTEStartSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteStartSFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayQTEHit1SFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteHitDamage, loop: false, m_voiceOverVolumeMutliplier);
		Audio.PlayClipOverrideVolumeModifier(m_qteHit1SFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayQTEHit2SFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteHitDamage, loop: false, m_voiceOverVolumeMutliplier);
		Audio.PlayClipOverrideVolumeModifier(m_qteHit2SFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayQTEHit3SFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteHitDamage, loop: false, m_voiceOverVolumeMutliplier);
		Audio.PlayClipOverrideVolumeModifier(m_qteHit3SFX.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayQTEAttackSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteBossAttackSFX, loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayQTEGestureFailSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteGestureFailSFX, loop: false, m_voiceOverVolumeMutliplier);
	}

	public void PlayQTEDefeatedSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteBossDefeatedVO.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
		if (m_qteBossDefeatSFX != null)
		{
			Audio.PlayClip(m_qteBossDefeatSFX, loop: false);
		}
	}

	public void PlayQTEEscapeSFX()
	{
		Audio.PlayClipOverrideVolumeModifier(m_qteBossEscapeVO.GetAudioClip(), loop: false, m_voiceOverVolumeMutliplier);
	}
}
