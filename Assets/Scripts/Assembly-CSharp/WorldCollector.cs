using System.Collections.Generic;
using UnityEngine;

public class WorldCollector
{
	private static ICollection<GameObject> s_whitelist = new HashSet<GameObject>();

	private static bool s_flagForRescan = false;

	public static bool IsRescanRequired => s_flagForRescan;

	public static void MarkAsMovable(GameObject obj)
	{
		s_whitelist.Add(obj);
		s_flagForRescan = true;
	}

	public static IEnumerable<GameObject> FindAllMovableGameObjects()
	{
		Object[] array = Object.FindObjectsOfType(typeof(GameObject));
		for (int i = 0; i < array.Length; i++)
		{
			GameObject sceneObject = (GameObject)array[i];
			if (s_whitelist.Contains(sceneObject) || !(sceneObject.tag != "Movable"))
			{
				yield return sceneObject;
			}
		}
		s_flagForRescan = false;
	}

	public static IEnumerable<GameObject> FindAllMovableOnlyInYGameObjects()
	{
		return GameObject.FindGameObjectsWithTag("MovableOnlyInY");
	}

	public static IEnumerable<GameObject> FindAllWhitelistedGameObjects()
	{
		s_flagForRescan = false;
		return s_whitelist;
	}
}
