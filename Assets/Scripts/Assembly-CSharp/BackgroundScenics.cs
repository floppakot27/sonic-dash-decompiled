using System;
using UnityEngine;

public class BackgroundScenics : MonoBehaviour
{
	private const float m_kSpeedMultiplier = 0.001f;

	private void Update()
	{
		Vector3 vector = default(Vector3);
		vector = base.transform.position;
		Vector3 vector2 = default(Vector3);
		vector2 = Sonic.Transform.forward;
		vector.y = 0f;
		vector2.y = 0f;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		vector2.Normalize();
		float f = Vector3.Dot(vector, vector2);
		Vector3 vector3 = Vector3.Cross(vector, vector2);
		float num = Mathf.Acos(f);
		if (num > (float)Math.PI / 2f)
		{
			num = (float)Math.PI - num;
		}
		if (vector3.y < 0f)
		{
			num *= -1f;
		}
		float num2 = 1000f / magnitude;
		base.transform.Rotate(Vector3.up, num * Time.deltaTime * 0.001f * Sonic.Tracker.Speed * num2);
	}
}
