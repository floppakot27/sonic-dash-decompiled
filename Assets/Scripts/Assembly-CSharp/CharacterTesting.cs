using UnityEngine;

public class CharacterTesting : MonoBehaviour
{
	private void Start()
	{
		GameObject gameObject = GameObject.FindWithTag("TopLevelSonic");
		if ((bool)gameObject)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		GameObject[] array = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		GameObject[] array2 = array;
		foreach (GameObject gameObject2 in array2)
		{
			GameObject gameObject3 = gameObject2.transform.root.gameObject;
			if (gameObject3 != base.gameObject)
			{
				Object.Destroy(gameObject3);
			}
		}
	}

	private void Update()
	{
		Application.LoadLevelAdditive("s_game");
		Object.Destroy(base.gameObject);
	}
}
