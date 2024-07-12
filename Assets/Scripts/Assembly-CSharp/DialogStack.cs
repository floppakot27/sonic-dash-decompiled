using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogStack : MonoBehaviour
{
	[Flags]
	private enum State
	{
		PresentNewDialog = 1,
		BackgroundNeeded = 2,
		BackgroundInTransition = 4,
		BackgroundActive = 8
	}

	private static DialogStack s_dialogStack;

	private State m_state;

	private Stack<GuiTrigger> m_dialogStack;

	private GuiTrigger m_activeDialog;

	private GuiTrigger m_transitioningDialog;

	[SerializeField]
	private AudioClip m_clipDialogTransition;

	[SerializeField]
	private GameObject m_backgroundTrigger;

	public static GuiTrigger ShowDialog(GuiTrigger dialog)
	{
		s_dialogStack.NewDialog(dialog);
		return dialog;
	}

	public static GuiTrigger ShowDialog(string dialogName)
	{
		Transform transform = s_dialogStack.transform.Find(dialogName);
		GuiTrigger component = transform.GetComponent<GuiTrigger>();
		s_dialogStack.NewDialog(component);
		return component;
	}

	public static void HideDialog()
	{
		s_dialogStack.CloseDialog();
	}

	private void Start()
	{
		s_dialogStack = this;
		m_dialogStack = new Stack<GuiTrigger>(5);
	}

	private void Update()
	{
		UpdateBackgroundFade();
	}

	private void NewDialog(GuiTrigger dialog)
	{
		GuiTrigger guiTrigger = null;
		if (m_dialogStack.Count > 0)
		{
			guiTrigger = m_dialogStack.Peek();
		}
		m_dialogStack.Push(dialog);
		InformDialog(dialog, pushed: true);
		if (guiTrigger == null)
		{
			OpenDialog();
			EventDispatch.GenerateEvent("OnDialogShown");
		}
		else if (m_transitioningDialog == null)
		{
			ResignDialog(guiTrigger);
		}
		else
		{
			m_state |= State.PresentNewDialog;
		}
	}

	private void OpenDialog()
	{
		if (m_dialogStack.Count != 0 && !(m_transitioningDialog != null))
		{
			m_transitioningDialog = m_dialogStack.Peek();
			m_transitioningDialog.Show();
			m_activeDialog = m_transitioningDialog;
			StartCoroutine(UpdateDialogTransition());
			m_state |= State.BackgroundNeeded;
		}
	}

	private void CloseDialog()
	{
		if (m_dialogStack.Count != 0 && !(m_transitioningDialog != null))
		{
			EventDispatch.GenerateEvent("CloseDialog");
			m_activeDialog = null;
			m_transitioningDialog = m_dialogStack.Pop();
			InformDialog(m_transitioningDialog, pushed: false);
			StartCoroutine(UpdateDialogTransition());
			if (m_dialogStack.Count == 0)
			{
				m_state &= ~State.BackgroundNeeded;
			}
		}
	}

	private void UpdateBackgroundFade()
	{
		if ((m_state & State.BackgroundInTransition) == State.BackgroundInTransition)
		{
			return;
		}
		bool flag = (m_state & State.BackgroundNeeded) == State.BackgroundNeeded;
		bool flag2 = (m_state & State.BackgroundActive) == State.BackgroundActive;
		if (flag != flag2)
		{
			m_backgroundTrigger.SendMessage("OnClick");
			m_state |= State.BackgroundInTransition;
			if (flag)
			{
				m_state |= State.BackgroundActive;
			}
			else
			{
				m_state &= ~State.BackgroundActive;
			}
		}
	}

	private void ResignDialog(GuiTrigger dialogToResign)
	{
		if (!(dialogToResign == null) && !(m_transitioningDialog != null))
		{
			m_activeDialog = null;
			m_transitioningDialog = dialogToResign;
			StartCoroutine(UpdateDialogTransition());
		}
	}

	private IEnumerator UpdateDialogTransition()
	{
		GuiTrigger dialogInTransition = m_transitioningDialog;
		Audio.PlayClip(m_clipDialogTransition, loop: false);
		dialogInTransition.Hide();
		do
		{
			yield return null;
		}
		while (dialogInTransition.Transition);
		m_transitioningDialog = null;
		if (m_dialogStack.Count == 0)
		{
			EventDispatch.GenerateEvent("OnDialogHidden");
		}
		else if (m_activeDialog == null)
		{
			OpenDialog();
		}
		else if ((m_state & State.PresentNewDialog) == State.PresentNewDialog)
		{
			ResignDialog(m_activeDialog);
			m_state &= ~State.PresentNewDialog;
		}
	}

	private void InformDialog(GuiTrigger guiTrigger, bool pushed)
	{
		GameObject trigger = guiTrigger.Trigger;
		if (pushed)
		{
			StartCoroutine(PendingPushedMessage(trigger));
		}
		else
		{
			trigger.BroadcastMessage("DialogPopped", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	private IEnumerator PendingPushedMessage(GameObject dialogObject)
	{
		yield return null;
		dialogObject.BroadcastMessage("DialogPushed", null, SendMessageOptions.DontRequireReceiver);
	}

	private IEnumerator DelayedBackgroundFinished()
	{
		yield return null;
		m_state &= ~State.BackgroundInTransition;
	}

	private void Trigger_BackgroundFadeFinished()
	{
		StartCoroutine(DelayedBackgroundFinished());
	}
}
