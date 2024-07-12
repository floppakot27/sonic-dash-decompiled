using System.Collections;
using UnityEngine;

[AddComponentMenu("Dash/Cameras/Spring Boss Camera")]
public class CameraTypeBossSpring : CameraType
{
	private float m_currentOrbitTime;

	private Vector3 m_velocity;

	[SerializeField]
	private string m_sonicHeadBoneName;

	private bool m_isSpringDescentStarted;

	private bool m_gestureFailureEntryLock;

	private float m_headshakeOffset;

	[SerializeField]
	private AnimationCurve m_climbYOffset;

	[SerializeField]
	private float m_orbitFwdOffset;

	[SerializeField]
	private float m_orbitUpOffset = -3f;

	[SerializeField]
	private float m_orbitAttackRadius = 5f;

	[SerializeField]
	private float m_orbitAttackAngularVelocity = 40f;

	[SerializeField]
	private float m_orbitRadius = 1.5f;

	[SerializeField]
	private float m_orbitAngularVelocity = 90f;

	[SerializeField]
	private float m_orbitTime = 1f;

	[SerializeField]
	private float m_climbSmoothing = 20f;

	[SerializeField]
	private float m_decendSmoothing = 20f;

	[SerializeField]
	private AnimationCurve m_descentYOffset;

	[SerializeField]
	private AnimationCurve m_orbitAngularVelocityMultiplier;

	[SerializeField]
	private AnimationCurve m_descentAngularVelocityMultiplierToEnd;

	[SerializeField]
	private float m_descentStartDeadZoneDuration = 0.5f;

	[SerializeField]
	private float m_descentLookAhead = 15f;

	[SerializeField]
	private float m_descentLookAheadSmoothSpeed = 1f;

	[SerializeField]
	private AnimationCurve m_failureHeadshakeLateralOffsetOverTime;

	[SerializeField]
	private float m_failureHeadshakeLateralOffsetMagnitude = 0.2f;

	public override bool EnableSmoothing => false;

	private void Awake()
	{
		EventDispatch.RegisterInterest("OnSpringDescent", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("BossSpringGestureSuccess", this);
		EventDispatch.RegisterInterest("BossSpringGestureFailure", this);
		EventDispatch.RegisterInterest("CharacterUnloadStart", this);
		EventDispatch.RegisterInterest("CharacterLoaded", this);
		StartCoroutine(IdleLoop());
	}

	private IEnumerator IdleLoop()
	{
		while (true)
		{
			if (Sonic.Bones != null)
			{
				Transform sonicHeadTransform = Sonic.Bones[m_sonicHeadBoneName];
				base.CachedLookAt = sonicHeadTransform.position;
				if (BehindCamera.Instance.Camera != null)
				{
					base.transform.position = BehindCamera.Instance.Camera.transform.position;
					base.transform.rotation = BehindCamera.Instance.Camera.transform.rotation;
				}
			}
			yield return null;
		}
	}

	public override void onActive()
	{
		m_isSpringDescentStarted = false;
		StopAllCoroutines();
		StartCoroutine(MoveToAttack());
	}

	public override void onInactive()
	{
		StopAllCoroutines();
	}

	public void Event_CharacterUnloadStart()
	{
		StopAllCoroutines();
	}

	public void Event_CharacterLoaded()
	{
		StopAllCoroutines();
		StartCoroutine(IdleLoop());
	}

	private IEnumerator FailureCameraFeedback()
	{
		m_gestureFailureEntryLock = true;
		float headshakeDuration = m_failureHeadshakeLateralOffsetOverTime.keys[m_failureHeadshakeLateralOffsetOverTime.length - 1].time;
		float headshakeTimer = 0f;
		while (headshakeTimer < headshakeDuration)
		{
			m_headshakeOffset = m_failureHeadshakeLateralOffsetOverTime.Evaluate(headshakeTimer) * m_failureHeadshakeLateralOffsetMagnitude;
			headshakeTimer += Time.deltaTime;
			yield return null;
		}
		m_headshakeOffset = 0f;
		m_gestureFailureEntryLock = false;
	}

	private IEnumerator MoveToAttack()
	{
		m_currentOrbitTime = 0f;
		m_velocity = Vector3.zero;
		Vector3 cameraPosition = base.transform.position;
		Transform sonicHeadTransform = Sonic.Bones[m_sonicHeadBoneName];
		while (BossBattleSystem.Instance().CurrentPhase.CurrentGesture == null)
		{
			float climbYOffset = m_climbYOffset.Evaluate(m_currentOrbitTime);
			cameraPosition = Vector3.SmoothDamp(target: CalculateOrbitPosAtTime(m_orbitAttackRadius, m_orbitAttackAngularVelocity, m_currentOrbitTime), current: cameraPosition, currentVelocity: ref m_velocity, smoothTime: m_climbSmoothing);
			if (m_currentOrbitTime < m_orbitTime)
			{
				m_currentOrbitTime += Time.deltaTime;
			}
			Vector3 position = cameraPosition;
			position.y = Mathf.Max(sonicHeadTransform.position.y, cameraPosition.y);
			base.transform.position = position;
			UpdateLookAt(sonicHeadTransform.position + Sonic.MeshTransform.forward * m_orbitFwdOffset);
			yield return new WaitForEndOfFrame();
		}
	}

	private void Event_BossSpringGestureSuccess()
	{
		StartCoroutine(BossSpringGestureSuccess());
	}

	private IEnumerator BossSpringGestureSuccess()
	{
		yield return StartCoroutine(MoveToOrbit());
		if (m_isSpringDescentStarted && !m_gestureFailureEntryLock)
		{
			yield return StartCoroutine(OrbitDescent(m_currentOrbitTime, 360f / m_orbitAngularVelocity));
		}
		else
		{
			yield return StartCoroutine(OrbitSonic(m_currentOrbitTime, smoothToTarget: false));
		}
	}

	private void Event_BossSpringGestureFailure()
	{
		if (!m_gestureFailureEntryLock)
		{
			StartCoroutine(FailureCameraFeedback());
			if (m_isSpringDescentStarted && !m_gestureFailureEntryLock)
			{
				StartCoroutine(OrbitDescent(m_currentOrbitTime, 360f / m_orbitAngularVelocity));
			}
			else
			{
				StartCoroutine(OrbitSonic(m_currentOrbitTime, smoothToTarget: true));
			}
		}
	}

	private IEnumerator MoveToOrbit()
	{
		float orbitPeriod = 360f / m_orbitAngularVelocity;
		Transform sonicHeadTransform = Sonic.Bones[m_sonicHeadBoneName];
		m_velocity = Vector3.zero;
		Vector3 lookatVelocity = Vector3.zero;
		Vector3 orbitPos = CalculateOrbitPosAtTime(m_orbitRadius, m_orbitAngularVelocity, m_currentOrbitTime);
		float smoothTime = 0.15f;
		float orbitTime = 1f;
		while (orbitTime > 0f)
		{
			Vector3 lookatOffset = base.transform.right * m_headshakeOffset;
			Vector3 lookat = sonicHeadTransform.position + lookatOffset;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, orbitPos, ref m_velocity, smoothTime);
			lookat = Vector3.SmoothDamp(base.transform.position + base.transform.forward, lookat, ref lookatVelocity, smoothTime);
			UpdateLookAt(lookat);
			orbitTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator OrbitSonic(float orbitStartTime, bool smoothToTarget)
	{
		float orbitTime = orbitStartTime;
		float orbitPeriod = 360f / m_orbitAngularVelocity;
		Transform sonicHeadTransform = Sonic.Bones[m_sonicHeadBoneName];
		m_velocity = Vector3.zero;
		Vector3 lookatVelocity = Vector3.zero;
		while (true)
		{
			Vector3 orbitPos = CalculateOrbitPosAtTime(m_orbitRadius, m_orbitAngularVelocity, orbitTime);
			Vector3 lookatOffset = base.transform.right * m_headshakeOffset;
			Vector3 lookat = sonicHeadTransform.position + lookatOffset;
			if (smoothToTarget)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, orbitPos, ref m_velocity, m_climbSmoothing);
				smoothToTarget = orbitTime < orbitStartTime + m_climbSmoothing * 2f;
				lookat = Vector3.SmoothDamp(base.transform.position + base.transform.forward, lookat, ref lookatVelocity, m_climbSmoothing);
			}
			else
			{
				base.transform.position = orbitPos;
			}
			UpdateLookAt(lookat);
			float orbitT = Mathf.InverseLerp(0f, orbitPeriod, orbitTime);
			float orbitMul = m_orbitAngularVelocityMultiplier.Evaluate(orbitT);
			orbitTime += Time.deltaTime * orbitMul;
			if (orbitTime > orbitPeriod)
			{
				orbitTime -= orbitPeriod;
			}
			if (m_isSpringDescentStarted && !m_gestureFailureEntryLock)
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return StartCoroutine(OrbitDescent(orbitTime, orbitPeriod));
	}

	private IEnumerator OrbitDescent(float descentStartTime, float orbitPeriod)
	{
		if (orbitPeriod - descentStartTime < m_descentStartDeadZoneDuration)
		{
			descentStartTime -= orbitPeriod;
		}
		float orbitTime = descentStartTime;
		m_velocity = Vector3.zero;
		float lookAhead = 0f;
		float lookAheadVelocity = 0f;
		Transform sonicHeadTransform = Sonic.Bones[m_sonicHeadBoneName];
		while (true)
		{
			float descentT = Utils.MapValue(orbitTime, descentStartTime, orbitPeriod, 0f, 1f);
			float descentYOffset = m_descentYOffset.Evaluate(descentT);
			Vector3 oldPosition = base.transform.position;
			Vector3 newPosition = CalculateOrbitPosAtTime(m_orbitRadius, m_orbitAngularVelocity, orbitTime) + descentYOffset * Sonic.MeshTransform.up;
			base.transform.position = newPosition;
			Vector3 idealLookAt = sonicHeadTransform.position + Sonic.MeshTransform.forward * lookAhead * descentT;
			Vector3 previousLookVector = base.CachedLookAt - oldPosition;
			Vector3 thisLookVector = idealLookAt - newPosition;
			thisLookVector = Vector3.SmoothDamp(previousLookVector, thisLookVector, ref m_velocity, m_decendSmoothing);
			base.CachedLookAt = newPosition + thisLookVector;
			base.transform.LookAt(base.CachedLookAt, Vector3.up);
			Debug.DrawLine(base.transform.position, idealLookAt, Color.red);
			Debug.DrawRay(base.CachedLookAt, Vector3.up, Color.red);
			Debug.DrawRay(base.transform.position, base.CachedLookAt - base.transform.position, Color.yellow);
			float descentRotationMultiplier = m_descentAngularVelocityMultiplierToEnd.Evaluate(descentT);
			float orbitT = Mathf.InverseLerp(0f, orbitPeriod, orbitTime);
			float orbitMul = m_orbitAngularVelocityMultiplier.Evaluate(orbitT);
			orbitTime += Time.deltaTime * orbitMul * descentRotationMultiplier;
			if (orbitTime > orbitPeriod)
			{
				orbitTime = orbitPeriod;
				lookAhead = Mathf.SmoothDamp(lookAhead, m_descentLookAhead, ref lookAheadVelocity, m_descentLookAheadSmoothSpeed);
			}
			yield return null;
		}
	}

	private Vector3 CalculateOrbitPosAtTime(float orbitRadius, float orbitAngularVelocity, float time)
	{
		Vector3 vector = -Sonic.MeshTransform.forward;
		float angle = time * orbitAngularVelocity;
		Quaternion quaternion = Quaternion.AngleAxis(angle, Sonic.MeshTransform.up);
		Vector3 vector2 = quaternion * vector;
		return Sonic.MeshTransform.position + vector2 * orbitRadius + Sonic.MeshTransform.up * m_orbitUpOffset;
	}

	private void Event_OnSpringDescent(float springDescentTime)
	{
		m_isSpringDescentStarted = true;
	}

	private void Event_OnSpringEnd()
	{
		StopAllCoroutines();
		BehindCamera.Instance.ResetToGameCamera(0.5f);
		StartCoroutine(IdleLoop());
	}

	private void UpdateLookAt(Vector3 lookAt)
	{
		base.CachedLookAt = lookAt;
		base.transform.LookAt(base.CachedLookAt, Vector3.up);
	}

	private void LateUpdate()
	{
	}
}
