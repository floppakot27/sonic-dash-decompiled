using System;
using System.Collections.Generic;
using UnityEngine;

public class BGScenicsGenerator : MonoBehaviour
{
	private const float m_minAngle = 60f;

	private const float m_maxAngle = 120f;

	private float[] m_nearRange = new float[2] { 700f, 1100f };

	private float[] m_farRange = new float[2] { 1000f, 1400f };

	public List<Transform> m_database = new List<Transform>();

	private List<GameObject> m_active = new List<GameObject>();

	private void Awake()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
	}

	private void Start()
	{
		Event_ResetGameState(GameState.Mode.Menu);
	}

	private void Event_ResetGameState(GameState.Mode state)
	{
		if (state != 0)
		{
			return;
		}
		foreach (GameObject item in m_active)
		{
			UnityEngine.Object.Destroy(item);
		}
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < m_database.Count; i++)
		{
			list.Add(m_database[i]);
		}
		float num = 0f;
		float num2 = (float)Math.PI * 2f;
		float num3 = (float)Math.PI / 3f;
		float max = (float)Math.PI * 2f / 3f;
		while (num < num2 - num3 && list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			GameObject gameObject = UnityEngine.Object.Instantiate(list[index].gameObject) as GameObject;
			num += UnityEngine.Random.Range(num3, max);
			float num4 = ((list.Count % 2 != 0) ? UnityEngine.Random.Range(m_farRange[0], m_farRange[1]) : UnityEngine.Random.Range(m_nearRange[0], m_nearRange[1]));
			Vector3 position = default(Vector3);
			position.x = Mathf.Cos(num) * num4;
			position.z = Mathf.Sin(num) * num4;
			position.y = -9f;
			gameObject.transform.position = position;
			gameObject.transform.rotation = Quaternion.Euler(-90f, UnityEngine.Random.Range(-180, 180), 0f);
			list.RemoveAt(index);
			m_active.Add(gameObject);
		}
	}

	private void Update()
	{
	}
}
