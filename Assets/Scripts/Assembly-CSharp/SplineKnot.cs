using UnityEngine;

[AddComponentMenu("")]
[ExecuteInEditMode]
public class SplineKnot : MonoBehaviour
{
	public enum EndBehaviour
	{
		Splat,
		Fall
	}

	[SerializeField]
	private EndBehaviour m_behaviourIfEnd;

	private Vector3 m_lastCleanPosition;

	private bool m_isDirty;

	public float Length { get; set; }

	public float WorldDistanceToNextKnot { get; set; }

	public Vector3 TowardsNextKnot { get; set; }

	public Vector3 Tangent { get; set; }

	public bool IsDirty { get; private set; }

	public Spline Spline => (!(base.transform.parent != null)) ? null : base.transform.parent.GetComponent<Spline>();

	public EndBehaviour BehaviourIfEnd
	{
		get
		{
			return m_behaviourIfEnd;
		}
		set
		{
			m_behaviourIfEnd = value;
		}
	}

	private void Awake()
	{
		IsDirty = true;
	}

	private void Update()
	{
		IsDirty = IsDirty || base.transform.localPosition != m_lastCleanPosition;
	}

	public void ClearDirtyFlag()
	{
		IsDirty = false;
		m_lastCleanPosition = base.transform.localPosition;
		base.enabled = false;
	}

	private void OnDrawGizmos()
	{
		Spline component = base.transform.parent.GetComponent<Spline>();
		Gizmos.color = component.KnotColour;
		Vector3 position = base.transform.position;
		Gizmos.DrawIcon(position, "splineknot.gif", allowScaling: false);
		Gizmos.DrawRay(position + Vector3.up * 0.05f, Tangent);
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(position + Vector3.up * 0.05f, TowardsNextKnot);
	}

	private void OnDestroy()
	{
		Spline spline = Spline;
		if (spline != null)
		{
			Spline.OnKnotDestruction(this);
		}
	}
}
