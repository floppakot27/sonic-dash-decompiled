using System.Collections;
using UnityEngine;

[AddComponentMenu("Dash/Track/Distance TV")]
public class DistanceTV : SpawnableObject
{
	[SerializeField]
	private AnimationCurve m_verticalBob;

	[SerializeField]
	private TextMesh m_distanceText;

	public override void Place(OnEvent onDestroy, Track onTrack, Spline onSpline)
	{
		base.Place(onDestroy, onTrack, onSpline);
		base.transform.position += 3f * base.transform.forward;
		StartCoroutine(HoverBob());
	}

	public void SetDistanceValue(int distance)
	{
		m_distanceText.text = distance + " M";
	}

	private IEnumerator HoverBob()
	{
		float bobTimer = Random.value * m_verticalBob.keys[m_verticalBob.length - 1].time;
		float bobSpeed = 1.5f;
		Vector3 origin = base.transform.localPosition;
		while (true)
		{
			bobTimer += Time.deltaTime * bobSpeed;
			base.transform.localPosition = origin + Vector3.up * m_verticalBob.Evaluate(bobTimer);
			yield return null;
		}
	}
}
