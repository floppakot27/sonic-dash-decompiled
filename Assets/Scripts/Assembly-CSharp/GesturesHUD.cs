using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dash/UI/GesturesHUD")]
public class GesturesHUD : MonoBehaviour
{
	[SerializeField]
	private GameObject m_leftGesture;

	[SerializeField]
	private GameObject m_rightGesture;

	[SerializeField]
	private GameObject m_upGesture;

	[SerializeField]
	private GameObject m_downGesture;

	private IList<GameObject> m_gestures;

	[SerializeField]
	private AudioClip m_sfx;

	private static GesturesHUD s_singleton;

	private GameObject this[CartesianDir dir]
	{
		get
		{
			return m_gestures[(int)dir];
		}
		set
		{
			m_gestures[(int)dir] = value;
		}
	}

	public static void ShowGesture(CartesianDir dir)
	{
		s_singleton.ShowGestureInternal(dir);
	}

	private void Awake()
	{
		s_singleton = this;
		m_gestures = new GameObject[Utils.GetEnumCount<CartesianDir>()];
		this[CartesianDir.Left] = m_leftGesture;
		this[CartesianDir.Right] = m_rightGesture;
		this[CartesianDir.Up] = m_upGesture;
		this[CartesianDir.Down] = m_downGesture;
	}

	private void ShowGestureInternal(CartesianDir dir)
	{
		DisableAllGestures();
		GameObject gameObject = this[dir];
		gameObject.SetActive(value: true);
		Animation componentInChildren = gameObject.GetComponentInChildren<Animation>();
		if (componentInChildren != null)
		{
			componentInChildren.Rewind();
			componentInChildren.Play();
			Audio.PlayClip(m_sfx, loop: false);
		}
	}

	private void DisableAllGestures()
	{
		foreach (GameObject gesture in m_gestures)
		{
			gesture.SetActive(value: false);
		}
	}
}
