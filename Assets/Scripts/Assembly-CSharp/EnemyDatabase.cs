using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dash/Enemies/Enemy Database")]
internal class EnemyDatabase : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_enemies;

	public IEnumerable<GameObject> EnemyPrefabs => m_enemies;
}
