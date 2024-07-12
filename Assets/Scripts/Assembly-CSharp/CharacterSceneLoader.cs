using System.Collections;
using UnityEngine;

public class CharacterSceneLoader : MonoBehaviour
{
	public enum Cycle
	{
		Next,
		Previous
	}

	private GameObject m_sonicRootGameObject;

	private bool m_sceneLoaded;

	public bool ScenesLoaded => m_sceneLoaded;

	public static GameObject GetRootObject()
	{
		return Sonic.MeshTransform.gameObject;
	}

	private void Start()
	{
		m_sceneLoaded = false;
		EventDispatch.RegisterInterest("OnCharacterLoad", this);
		m_sonicRootGameObject = GameObject.FindWithTag("TopLevelSonic");
	}

	private void Event_OnCharacterLoad(SceneIdentifiers.ID nSceneId)
	{
		StartCoroutine(Load(nSceneId));
	}

	private IEnumerator Load(SceneIdentifiers.ID nSceneId)
	{
		EventDispatch.GenerateEvent("CharacterUnloadStart");
		if (!(Sonic.Transform == null))
		{
			if ((bool)m_sonicRootGameObject)
			{
				for (int ii = 0; ii < m_sonicRootGameObject.transform.childCount; ii++)
				{
					Transform child = m_sonicRootGameObject.transform.GetChild(ii);
					Object.Destroy(child.gameObject);
				}
			}
			Sonic.Clear();
			yield return null;
		}
		EventDispatch.GenerateEvent("CharacterUnloaded");
		yield return Application.LoadLevelAdditiveAsync(SceneIdentifiers.Names[(int)nSceneId]);
		Resources.UnloadUnusedAssets();
		yield return null;
		ApplyReferences();
		m_sceneLoaded = true;
		EventDispatch.GenerateEvent("CharacterLoaded");
	}

	private void ApplyReferences()
	{
		GameObject gameObject = GameObject.FindWithTag("CharacterImportRoot");
		if (!(null == gameObject) && !(null == m_sonicRootGameObject))
		{
			gameObject.transform.parent = m_sonicRootGameObject.transform;
			gameObject.transform.localPosition = Vector3.zero;
			Sonic.FixupSonic();
			gameObject = null;
		}
	}

	private void CopyAnimationStates(GameObject sourceObject, GameObject targetObj)
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			foreach (AnimationState item in targetObj.animation)
			{
				if ((bool)item)
				{
					targetObj.animation.RemoveClip(item.clip);
					flag = true;
					break;
				}
			}
		}
		foreach (AnimationState item2 in sourceObject.animation)
		{
			targetObj.animation.AddClip(sourceObject.animation[item2.name].clip, item2.name);
		}
	}

	private Transform[] CreateBoneArray(SkinnedMeshRenderer renderer, Transform target)
	{
		Transform[] array = new Transform[renderer.bones.Length];
		for (int i = 0; i < renderer.bones.Length; i++)
		{
			Transform transform = null;
			Component[] componentsInChildren = target.GetComponentsInChildren(typeof(Transform));
			foreach (Component component in componentsInChildren)
			{
				if (component.name == renderer.bones[i].name)
				{
					transform = component.transform;
					break;
				}
			}
			array[i] = transform;
		}
		return array;
	}
}
