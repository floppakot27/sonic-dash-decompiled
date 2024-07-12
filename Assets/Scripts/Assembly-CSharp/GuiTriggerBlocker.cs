using UnityEngine;

public class GuiTriggerBlocker : MonoBehaviour
{
	private enum Reasons
	{
		Dialogs = 1,
		CharacterSelection
	}

	private Reasons m_blockedReason;

	private void Start()
	{
		m_blockedReason = (Reasons)0;
		EventDispatch.RegisterInterest("OnDialogShown", this);
		EventDispatch.RegisterInterest("OnDialogHidden", this);
		EventDispatch.RegisterInterest("OnCharacterSelection", this);
	}

	private void UpdateGuiState()
	{
		GuiTrigger[] componentsInChildren = GetComponentsInChildren<GuiTrigger>();
		GuiTrigger[] array = componentsInChildren;
		foreach (GuiTrigger guiTrigger in array)
		{
			guiTrigger.ButtonsBlocked(m_blockedReason != (Reasons)0);
		}
	}

	private void Event_OnCharacterSelection(bool active)
	{
		if (active)
		{
			m_blockedReason |= Reasons.CharacterSelection;
		}
		else
		{
			m_blockedReason &= (Reasons)(-3);
		}
		UpdateGuiState();
	}

	private void Event_OnDialogShown()
	{
		Reasons blockedReason = m_blockedReason;
		m_blockedReason |= Reasons.Dialogs;
		if (blockedReason == (Reasons)0)
		{
			UpdateGuiState();
		}
	}

	private void Event_OnDialogHidden()
	{
		m_blockedReason &= (Reasons)(-2);
		if (m_blockedReason == (Reasons)0)
		{
			UpdateGuiState();
		}
	}
}
