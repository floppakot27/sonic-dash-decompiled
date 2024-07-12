using UnityEngine;

public class MoveToCharacter : MonoBehaviour
{
	private void Start()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("CharacterImportRoot");
		if ((bool)gameObject)
		{
			base.gameObject.transform.parent = gameObject.transform;
		}
		Object.Destroy(this);
	}
}
