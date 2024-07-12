using UnityEngine;

[RequireComponent(typeof(ParticleControllerScript))]
[AddComponentMenu("Dash/Sonic/Airborne Particles")]
public class AirborneParticles : MonoBehaviour
{
	private ParticleSystem m_airstreamParticles;

	private float m_angleFromHorizontal;

	private float m_targetAngleFromHorizontal;

	private float m_angleFromHorizontalVelocity;

	[SerializeField]
	private float m_ascentAngleFromHorizontal = 75f;

	[SerializeField]
	private float m_angleSmoothing = 0.5f;

	private void Start()
	{
		ParticleControllerScript component = GetComponent<ParticleControllerScript>();
		m_airstreamParticles = component.m_AirborneParticles;
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringDescent", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		ParticlePlayer.Play(m_airstreamParticles, ParticlePlayer.Important.Yes);
		m_angleFromHorizontalVelocity = 0f;
		m_targetAngleFromHorizontal = m_ascentAngleFromHorizontal;
	}

	private void Event_OnSpringDescent(float springDescentTime)
	{
		m_targetAngleFromHorizontal = 0f;
	}

	private void Event_OnSpringEnd()
	{
		m_airstreamParticles.Stop();
	}

	private void Update()
	{
		m_angleFromHorizontal = Utils.SmoothDamp(m_angleFromHorizontal, m_targetAngleFromHorizontal, ref m_angleFromHorizontalVelocity, m_angleSmoothing);
		Vector3 localEulerAngles = m_airstreamParticles.transform.localEulerAngles;
		m_airstreamParticles.transform.localEulerAngles = new Vector3(m_angleFromHorizontal, localEulerAngles.y, localEulerAngles.z);
	}
}
