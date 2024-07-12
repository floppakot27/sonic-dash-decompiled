using System.Collections.Generic;
using UnityEngine;

public class ShaderWarmup : MonoBehaviour
{
	public List<Shader> m_shaders = new List<Shader>();

	private void Start()
	{
		Shader.WarmupAllShaders();
	}

	private void Update()
	{
	}
}
