using UnityEngine;

public class BuildReef : MonoBehaviour
{
	private struct RandomValue
	{
		private readonly float x;

		private readonly float z;

		private readonly int meshIndex;

		private readonly int angleDeg;

		public Vector3 LocalOffset => new Vector3(x, 0f, z);

		public int MeshIndex => meshIndex;

		public int AngleDeg => angleDeg;

		public RandomValue(float x, float z, int meshIndex, int angleDeg)
		{
			this.x = x;
			this.z = z;
			this.meshIndex = meshIndex;
			this.angleDeg = angleDeg;
		}
	}

	private const float kfFogScalar = 1f;

	public Mesh[] m_reefMeshes;

	public Material m_reefMtrl;

	private Material[] m_reefMtrlArray;

	public int m_NumObjectsSide = 9;

	private int knNumReefObjectsSide;

	private int knNumReefObjects;

	public float m_DistanceApart = 31f;

	public float m_ScaleLocalOffset = 0.06f;

	public Vector3 m_objectScale = new Vector3(1.5f, 1.5f, 1f);

	public bool m_doRandomRotation = true;

	private RandomValue[] m_randomValues = new RandomValue[12]
	{
		new RandomValue(-1.1f, -1.1f, 0, 0),
		new RandomValue(4.4f, 1.4f, 2, 90),
		new RandomValue(-1.3f, 3.5f, 1, 180),
		new RandomValue(2.4f, -0.4f, 0, 270),
		new RandomValue(1f, 1.9f, 2, 180),
		new RandomValue(4.78f, 3.2f, 0, 90),
		new RandomValue(-1.2f, -0.9f, 2, 0),
		new RandomValue(-0.7f, 5f, 1, 90),
		new RandomValue(-3.9f, 1.9f, 2, 180),
		new RandomValue(2.2f, -2.29f, 1, 270),
		new RandomValue(-3.1f, 1.59f, 0, 180),
		new RandomValue(1.2f, -2.39f, 1, 90)
	};

	private void Start()
	{
		knNumReefObjectsSide = m_NumObjectsSide;
		knNumReefObjects = knNumReefObjectsSide * knNumReefObjectsSide;
		WorldCollector.MarkAsMovable(base.gameObject);
		m_reefMtrlArray = new Material[knNumReefObjects];
		for (int i = 0; i < knNumReefObjects; i++)
		{
			Material material = new Material(Shader.Find("Custom/Coral"));
			material.CopyPropertiesFromMaterial(m_reefMtrl);
			m_reefMtrlArray[i] = material;
		}
	}

	private void Update()
	{
		if (m_reefMeshes.Length == 0 || Sonic.MeshTransform == null)
		{
			return;
		}
		float num = (float)knNumReefObjectsSide * m_DistanceApart * 0.5f * 1f;
		float num2 = (float)(knNumReefObjectsSide - 1) * 0.5f * m_DistanceApart;
		float num3 = (float)(knNumReefObjectsSide - 1) * 0.5f * m_DistanceApart;
		Vector3 position = base.transform.position;
		position.x = 0f - Sonic.MeshTransform.position.x;
		position.z = 0f - Sonic.MeshTransform.position.z;
		Vector3 vector = position;
		if (vector.x < 0f)
		{
			vector.x -= m_DistanceApart * 0.5f;
		}
		else
		{
			vector.x += m_DistanceApart * 0.5f;
		}
		if (vector.z < 0f)
		{
			vector.z -= m_DistanceApart * 0.5f;
		}
		else
		{
			vector.z += m_DistanceApart * 0.5f;
		}
		int num4 = (int)(vector.x / m_DistanceApart);
		int num5 = (int)(vector.z / m_DistanceApart);
		int layer = LayerMask.NameToLayer("Underwater");
		for (int i = 0; i < knNumReefObjects; i++)
		{
			int num6 = i % knNumReefObjectsSide;
			int num7 = i / knNumReefObjectsSide;
			float num8 = num2 - (float)(-num4 + num6) * m_DistanceApart;
			float num9 = num3 - (float)(-num5 + num7) * m_DistanceApart;
			int num10 = Mathf.Abs(-num4 + num6) * 3 + Mathf.Abs(-num5 + num7);
			int num11 = num10 % m_randomValues.Length;
			RandomValue randomValue = m_randomValues[num11];
			Vector3 localOffset = randomValue.LocalOffset;
			localOffset = localOffset * m_ScaleLocalOffset * m_DistanceApart;
			Vector3 vector2 = new Vector3(0f - num8, 0f, 0f - num9) + localOffset;
			Vector3 vector3 = base.transform.position + vector2 - Sonic.MeshTransform.position;
			vector3.y = 0f;
			float value = 1f - vector3.sqrMagnitude / (num * num);
			value = Mathf.Clamp01(value);
			if (value > 0f)
			{
				Material material = m_reefMtrlArray[i];
				if ((bool)material)
				{
					Color color = material.color;
					color.a = value;
					material.SetColor("_Color", color);
					Quaternion q = ((!m_doRandomRotation) ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.Euler(-90f, randomValue.AngleDeg, 0f));
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(base.transform.position + vector2, q, m_objectScale);
					Mesh mesh = m_reefMeshes[randomValue.MeshIndex % m_reefMeshes.Length];
					Graphics.DrawMesh(mesh, matrix, material, layer);
				}
			}
		}
	}

	private void OnDestroy()
	{
	}
}
