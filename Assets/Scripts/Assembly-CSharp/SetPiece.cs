using UnityEngine;

[AddComponentMenu("Dash/Track/Set Piece")]
internal class SetPiece : MonoBehaviour
{
	private void OnSpawned()
	{
		base.collider.enabled = false;
	}
}
