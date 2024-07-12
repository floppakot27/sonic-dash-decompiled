using UnityEngine;

public class LoadingScreenTypeFade : LoadingScreenType
{
	[SerializeField]
	private UISprite m_rootSprite;

	[SerializeField]
	private UILabel m_rootLabel;

	[SerializeField]
	private UITexture m_rootTexture;

	[SerializeField]
	private Transform m_rootObject;

	public override void SetTransitionState(float transitionValue)
	{
		TransitionObject(m_rootSprite, transitionValue);
		TransitionObject(m_rootLabel, transitionValue);
		TransitionObject(m_rootTexture, transitionValue);
		if (!(m_rootObject != null))
		{
			return;
		}
		UISprite[] componentsInChildren = m_rootObject.GetComponentsInChildren<UISprite>();
		UILabel[] componentsInChildren2 = m_rootObject.GetComponentsInChildren<UILabel>();
		UITexture[] componentsInChildren3 = m_rootObject.GetComponentsInChildren<UITexture>();
		if (componentsInChildren != null)
		{
			UISprite[] array = componentsInChildren;
			foreach (UISprite thisSprite in array)
			{
				TransitionObject(thisSprite, transitionValue);
			}
		}
		if (componentsInChildren2 != null)
		{
			UILabel[] array2 = componentsInChildren2;
			foreach (UILabel thisLabel in array2)
			{
				TransitionObject(thisLabel, transitionValue);
			}
		}
		if (componentsInChildren3 != null)
		{
			UITexture[] array3 = componentsInChildren3;
			foreach (UITexture thisTexture in array3)
			{
				TransitionObject(thisTexture, transitionValue);
			}
		}
	}

	private void Start()
	{
		if ((bool)m_rootObject)
		{
			m_rootObject.gameObject.SetActive(value: true);
		}
	}

	private void TransitionObject(UISprite thisSprite, float transitionValue)
	{
		if (!(thisSprite == null))
		{
			Color color = thisSprite.color;
			color.a = transitionValue;
			thisSprite.color = color;
		}
	}

	private void TransitionObject(UILabel thisLabel, float transitionValue)
	{
		if (!(thisLabel == null))
		{
			Color color = thisLabel.color;
			color.a = transitionValue;
			thisLabel.color = color;
		}
	}

	private void TransitionObject(UITexture thisTexture, float transitionValue)
	{
		if (!(thisTexture == null))
		{
			Color color = thisTexture.color;
			color.a = transitionValue;
			thisTexture.color = color;
		}
	}
}
