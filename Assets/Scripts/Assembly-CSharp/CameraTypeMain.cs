using UnityEngine;

[AddComponentMenu("Dash/Cameras/Main")]
public class CameraTypeMain : CameraType
{
	private struct OriginalProperties
	{
		public Vector3 m_position;

		public Vector3 m_up;

		public Vector3 m_lookAt;

		public float m_fov;
	}

	private CameraType m_cameraToTrack;

	private CameraType m_previousCameraToTrack;

	private Vector3 m_previousCameraOffset = Vector3.zero;

	private Vector3 m_previousLookAtOffset = Vector3.zero;

	private float m_transitionTimer;

	private float m_transitionDuration;

	private OriginalProperties m_velocities = default(OriginalProperties);

	private Vector3 m_cameraWorldPosition;

	private Vector3 m_lookAtWorldPosition;

	private Vector3 m_cameraUpDirection;

	private Vector3 m_worldVelocity;

	private Camera m_thisCamera;

	public CameraTypeAnimation m_animationCamera;

	[SerializeField]
	private float m_standardSmoothingRate = 0.05f;

	[SerializeField]
	private float m_fovSpeedMultiplier = 0.4f;

	[SerializeField]
	private Material m_simpleSkybox;

	public void SetCamera(CameraType cameraToTrack, float transitionDuration)
	{
		if (!(cameraToTrack == m_cameraToTrack))
		{
			m_previousCameraToTrack = m_cameraToTrack;
			if (m_previousCameraToTrack != null)
			{
				m_previousCameraOffset = m_cameraWorldPosition - m_previousCameraToTrack.gameObject.transform.position;
				m_previousLookAtOffset = m_lookAtWorldPosition - m_previousCameraToTrack.CachedLookAt;
			}
			else
			{
				m_previousCameraOffset = Vector3.zero;
				m_previousLookAtOffset = Vector3.zero;
			}
			m_cameraToTrack = cameraToTrack;
			m_cameraToTrack.onActive();
			m_transitionTimer = 0f;
			m_transitionDuration = transitionDuration;
			SaveOriginalProperties();
		}
	}

	public void SetAnimationCamera(CameraTypeAnimation animationCamera)
	{
		m_animationCamera = animationCamera;
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
		m_previousCameraToTrack = null;
		m_cameraToTrack = null;
		m_transitionTimer = 0f;
		m_transitionDuration = 0f;
		base.CachedLookAt = base.transform.forward * 100f;
		m_cameraUpDirection = base.transform.up;
		m_thisCamera = GetComponentInChildren<Camera>();
		if (!FeatureSupport.IsSupported("Skydome Scene"))
		{
			RenderSettings.skybox = m_simpleSkybox;
		}
	}

	private void LateUpdate()
	{
		if (!(m_cameraToTrack == null))
		{
			TrackTargetCamera();
			ApplyTransform();
		}
	}

	private void ApplyTransform()
	{
		if ((bool)m_animationCamera)
		{
			base.transform.position = Vector3.Lerp(m_cameraWorldPosition, m_animationCamera.transform.position, m_animationCamera.Weight);
			Vector3 vector = Vector3.Lerp(m_lookAtWorldPosition, m_animationCamera.CachedLookAt, m_animationCamera.Weight);
			Vector3 worldUp = Vector3.Lerp(m_cameraUpDirection, m_animationCamera.transform.up, m_animationCamera.Weight);
			base.transform.LookAt(vector, worldUp);
			base.CachedLookAt = vector;
		}
		else
		{
			base.transform.position = m_cameraWorldPosition;
			base.transform.LookAt(m_lookAtWorldPosition, m_cameraUpDirection);
			base.CachedLookAt = m_lookAtWorldPosition;
		}
	}

	private void TrackTargetCamera()
	{
		CameraProperties component = m_cameraToTrack.GetComponent<CameraProperties>();
		Vector3 vector = m_cameraToTrack.transform.position;
		Vector3 vector2 = m_cameraToTrack.CachedLookAt;
		float num = component.FOV;
		Vector3 vector3 = m_cameraToTrack.transform.up;
		if ((bool)m_previousCameraToTrack)
		{
			m_transitionTimer += Time.deltaTime;
			CameraProperties component2 = m_previousCameraToTrack.GetComponent<CameraProperties>();
			Vector3 to = m_previousCameraToTrack.transform.position + m_previousCameraOffset;
			Vector3 to2 = m_previousCameraToTrack.CachedLookAt + m_previousLookAtOffset;
			float num2 = Mathf.Clamp01(m_transitionTimer / m_transitionDuration);
			float t = 1f - num2;
			vector = Vector3.Lerp(vector, to, t);
			vector2 = Vector3.Lerp(vector2, to2, t);
			num = Mathf.Lerp(num, component2.FOV, t);
			vector3 = Vector3.Lerp(vector3, m_previousCameraToTrack.transform.up, t);
			if (m_transitionTimer > m_transitionDuration)
			{
				m_previousCameraToTrack.onInactive();
				m_previousCameraToTrack = null;
			}
		}
		if (m_cameraToTrack.EnableSmoothing)
		{
			if (Time.deltaTime > 0f)
			{
				Vector3 vector4 = Vector3.SmoothDamp(m_cameraWorldPosition, vector, ref m_velocities.m_position, m_standardSmoothingRate);
				m_worldVelocity = ((Time.deltaTime != 0f) ? ((vector4 - m_cameraWorldPosition) / Time.deltaTime) : m_worldVelocity);
				m_cameraWorldPosition = vector4;
				m_lookAtWorldPosition = Vector3.SmoothDamp(m_lookAtWorldPosition, vector2, ref m_velocities.m_lookAt, m_standardSmoothingRate);
				m_cameraUpDirection = Vector3.SmoothDamp(m_cameraUpDirection, vector3, ref m_velocities.m_up, m_standardSmoothingRate);
			}
		}
		else
		{
			m_cameraWorldPosition = vector;
			m_cameraUpDirection = vector3;
			m_lookAtWorldPosition = vector2;
		}
		if ((bool)Sonic.Tracker)
		{
			float num3 = Sonic.Tracker.Speed / Sonic.Handling.StartSpeed;
			num3 = (num3 - 1f) * m_fovSpeedMultiplier + 1f;
			num3 = Mathf.Clamp(num3, 1f, 1.1f);
			num = Mathf.Max(num * num3, num);
		}
		if (Time.deltaTime > 0f)
		{
			float fieldOfView = Utils.SmoothDamp(m_thisCamera.fieldOfView, num, ref m_velocities.m_fov, m_standardSmoothingRate);
			m_thisCamera.fieldOfView = fieldOfView;
		}
	}

	private void SaveOriginalProperties()
	{
		m_velocities.m_position = m_worldVelocity;
	}

	public CameraType GetCurrentCameraType()
	{
		return m_cameraToTrack;
	}
}
