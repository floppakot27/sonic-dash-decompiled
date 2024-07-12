using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
	public static Trail m_instance;

	public Material m_trailMaterial;

	public int m_numberOfTubeSections = 16;

	public float m_zeroAlphaDistance = 5f;

	private bool m_requestActivation;

	private bool m_requestDeactivation;

	private bool m_activated;

	public float m_verticalOffset = 0.5f;

	public float m_tubeRadius = 0.5f;

	private List<Vector3> m_trailPoints;

	private void Awake()
	{
		m_requestActivation = false;
		m_requestDeactivation = false;
		m_activated = false;
		m_instance = this;
	}

	private void Start()
	{
		if (null == m_trailMaterial)
		{
			base.enabled = false;
		}
		EventDispatch.RegisterInterest("OnSonicDeath", this);
	}

	public void Event_OnSonicDeath()
	{
		deactivate();
	}

	private void Update()
	{
		if (m_activated)
		{
			if (m_requestDeactivation)
			{
				m_activated = false;
				m_requestDeactivation = false;
			}
		}
		else if (m_requestActivation)
		{
			m_activated = true;
			m_requestActivation = false;
			m_trailPoints = new List<Vector3>();
			Vector3 item = Sonic.Tracker.transform.position + Vector3.up * m_verticalOffset;
			m_trailPoints.Insert(0, item);
		}
		if (!m_activated)
		{
			return;
		}
		Vector3 frameMovement = MotionAttackState.getFrameMovement();
		List<Vector3> list = new List<Vector3>();
		foreach (Vector3 trailPoint in m_trailPoints)
		{
			Vector3 item2 = trailPoint - frameMovement;
			list.Insert(0, item2);
		}
		Vector3 item3 = Sonic.Tracker.transform.position + Vector3.up * m_verticalOffset;
		list.Insert(0, item3);
		m_trailPoints = list;
	}

	private void OnPostRender()
	{
		if (!m_activated || m_trailPoints.Count <= 1)
		{
			return;
		}
		m_trailMaterial.SetPass(0);
		if (Sonic.RenderManager == null)
		{
			return;
		}
		Color trailColor = Sonic.RenderManager.m_trailColor;
		GL.Begin(7);
		float num = 0f;
		for (int i = 0; i < m_trailPoints.Count - 1; i++)
		{
			int index = i + 1;
			Vector3 vector = m_trailPoints[i];
			Vector3 vector2 = m_trailPoints[index];
			Vector3 axis = vector2 - vector;
			float magnitude = axis.magnitude;
			axis.Normalize();
			float num2 = num;
			num += magnitude;
			float num3 = num;
			Vector3 up = Vector3.up;
			int numberOfTubeSections = m_numberOfTubeSections;
			if (i == 0)
			{
				for (int j = 0; j < numberOfTubeSections; j++)
				{
					int num4 = j;
					int num5 = num4 + 1;
					if (num5 >= numberOfTubeSections)
					{
						num5 = 0;
					}
					float angle = 360f / (float)numberOfTubeSections * (float)num4;
					Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
					Vector3 vector3 = quaternion * (up * m_tubeRadius);
					float angle2 = 360f / (float)numberOfTubeSections * (float)num5;
					Quaternion quaternion2 = Quaternion.AngleAxis(angle2, axis);
					Vector3 vector4 = quaternion2 * (up * m_tubeRadius);
					Vector3 vector5 = vector + vector3;
					Vector3 vector6 = vector + vector4;
					Color c = trailColor;
					Color c2 = trailColor;
					GL.Color(c);
					GL.Vertex3(vector5.x, vector5.y, vector5.z);
					GL.Color(c2);
					GL.Vertex3(vector.x, vector.y, vector.z);
					GL.Vertex3(vector.x, vector.y, vector.z);
					GL.Color(c);
					GL.Vertex3(vector6.x, vector6.y, vector6.z);
				}
			}
			for (int k = 0; k < numberOfTubeSections; k++)
			{
				int num6 = k;
				int num7 = num6 + 1;
				if (num7 >= numberOfTubeSections)
				{
					num7 = 0;
				}
				float angle3 = 360f / (float)numberOfTubeSections * (float)num6;
				Quaternion quaternion3 = Quaternion.AngleAxis(angle3, axis);
				Vector3 vector7 = quaternion3 * (up * m_tubeRadius);
				float angle4 = 360f / (float)numberOfTubeSections * (float)num7;
				Quaternion quaternion4 = Quaternion.AngleAxis(angle4, axis);
				Vector3 vector8 = quaternion4 * (up * m_tubeRadius);
				Vector3 vector9 = vector + vector7;
				Vector3 vector10 = vector + vector8;
				Vector3 vector11 = vector2 + vector7;
				Vector3 vector12 = vector2 + vector8;
				float zeroAlphaDistance = m_zeroAlphaDistance;
				Color c3 = trailColor;
				Color c4 = trailColor;
				c3.a = Mathf.Clamp(1f - num2 / zeroAlphaDistance, 0f, 1f);
				c4.a = Mathf.Clamp(1f - num3 / zeroAlphaDistance, 0f, 1f);
				GL.Color(c3);
				GL.Vertex3(vector9.x, vector9.y, vector9.z);
				GL.Color(c4);
				GL.Vertex3(vector11.x, vector11.y, vector11.z);
				GL.Vertex3(vector12.x, vector12.y, vector12.z);
				GL.Color(c3);
				GL.Vertex3(vector10.x, vector10.y, vector10.z);
			}
		}
		GL.End();
	}

	public void activate()
	{
		m_requestActivation = true;
		m_requestDeactivation = false;
	}

	public void deactivate()
	{
		m_requestActivation = false;
		m_requestDeactivation = true;
	}
}
