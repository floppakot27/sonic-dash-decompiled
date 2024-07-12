using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngineExtensions;

public static class CoroutineUtils
{
	public class CoroutineRefCounter
	{
		private int m_counter;

		public bool IsAnyRoutineRunning => m_counter > 0;

		public IEnumerator RefCountRoutine(MonoBehaviour owner, IEnumerator routine)
		{
			m_counter++;
			yield return owner.StartCoroutine(routine);
			m_counter--;
		}
	}

	public static Coroutine JoinCoroutines(this MonoBehaviour owner, IEnumerable<IEnumerator> coroutines)
	{
		return owner.StartCoroutine(CoroutineJoiner(owner, coroutines));
	}

	private static IEnumerator CoroutineJoiner(MonoBehaviour owner, IEnumerable<IEnumerator> coroutines)
	{
		CoroutineRefCounter refCounter = new CoroutineRefCounter();
		foreach (IEnumerator coroutine in coroutines)
		{
			if (coroutine != null)
			{
				owner.StartCoroutine(refCounter.RefCountRoutine(owner, coroutine));
			}
		}
		while (refCounter.IsAnyRoutineRunning)
		{
			yield return null;
		}
	}
}
