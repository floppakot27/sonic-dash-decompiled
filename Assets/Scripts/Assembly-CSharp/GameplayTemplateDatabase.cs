using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Gameplay Templates/Template Database")]
public class GameplayTemplateDatabase : MonoBehaviour, IEnumerable, IEnumerable<GameplayTemplate>
{
	[SerializeField]
	private string m_templatesPath = "Dash Assets/Prefabs/Gameplay/gameplay_templates.csv";

	[SerializeField]
	private List<GameplayTemplate> m_templates = new List<GameplayTemplate>();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_templates.GetEnumerator();
	}

	public void Awake()
	{
	}

	public void Start()
	{
		using IEnumerator<GameplayTemplate> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameplayTemplate current = enumerator.Current;
			current.OnEnable();
		}
	}

	public int GetIndexOf(GameplayTemplate templateToFind)
	{
		return m_templates.FindIndex((GameplayTemplate template) => template == templateToFind);
	}

	public IEnumerator<GameplayTemplate> GetEnumerator()
	{
		return m_templates.GetEnumerator();
	}

	public GameplayTemplate GetRandomTemplate(System.Random rng)
	{
		return m_templates[rng.Next(m_templates.Count)];
	}

	public int GetCountOfTemplatesForGroupID(GameplayTemplate.Group id)
	{
		int num = 0;
		foreach (GameplayTemplate template in m_templates)
		{
			if (template.ContainsGroup(id))
			{
				num++;
			}
		}
		return num;
	}

	public GameplayTemplate GetIndexedTemplateForGroupID(GameplayTemplate.Group id, int index)
	{
		int num = 0;
		foreach (GameplayTemplate template in m_templates)
		{
			if (template.ContainsGroup(id))
			{
				if (index == num)
				{
					return template;
				}
				num++;
			}
		}
		return m_templates[0];
	}

	public GameplayTemplate PickRandomTemplate(IEnumerable<GameplayTemplate> templates, System.Random rng, GameplayTemplate ignoreTemplate)
	{
		int num = templates.Count();
		if (num == 0)
		{
			return null;
		}
		int num2 = rng.Next(num);
		GameplayTemplate gameplayTemplate = templates.ElementAt(num2);
		if (gameplayTemplate == ignoreTemplate)
		{
			num2 = (num2 + 1) % num;
			return templates.ElementAt(num2);
		}
		return gameplayTemplate;
	}
}
