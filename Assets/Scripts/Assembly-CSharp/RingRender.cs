using UnityEngine;

public class RingRender : MonoBehaviour
{
	private const int FrameCount = 16;

	private const int FramesPerRow = 4;

	private const float OneOverRows = 0.25f;

	private static Vector3 s_ringScale = new Vector3(1f, 1f, 1f);

	private static Quaternion s_billboardOrientation = Quaternion.Euler(90f, 0f, 0f);

	private bool m_magnetPermitted;

	[SerializeField]
	private Mesh m_panelRingMesh;

	[SerializeField]
	private Material m_animatedRingMaterial;

	[SerializeField]
	private float m_ringBillBoardSize = 2f;

	[SerializeField]
	private int m_billboardFps = 10;

	[SerializeField]
	private ParticleSystem m_pickUpEffect;

	[SerializeField]
	private int m_pickUpEffectPoolSize = 10;

	[SerializeField]
	private AudioClip m_pickUpAudioClip;

	[SerializeField]
	private AudioClip m_pickUpAudioClipAlt;

	private bool m_playAltAudioclip = true;

	[SerializeField]
	private float m_maxSqRenderDistance = 40000f;

	private ParticleSystem[] m_pickUpEffectPool;

	private int m_currentPickUpEffect;

	private Transform m_worldContainerTransform;

	private RingGenerator m_ringGenerator;

	private CameraTypeMain m_mainCamera;

	private Vector2[] m_ringUVs = new Vector2[4];

	private float m_oneOverFps;

	private float m_fpsTimer;

	private int m_billboardFrame;

	private float m_lastPickupTime;

	private float m_pickupSfxPitch = 1f;

	[SerializeField]
	private float m_pickupSfxPitchMin = 1f;

	[SerializeField]
	private float m_pickupSfxPitchMax = 1.25f;

	[SerializeField]
	private float m_pickupSfxPitchChangeDuration = 10f;

	[SerializeField]
	private float m_pickupSfxPitchChangeGapSeconds = 0.5f;

	private float m_pitchChangePerSec;

	private void Start()
	{
		m_ringGenerator = GetComponent<RingGenerator>();
		FindMainCamera();
		CreatePools(m_pickUpEffect, ref m_pickUpEffectPool, m_pickUpEffectPoolSize, ref m_currentPickUpEffect);
		CreateMeshCombiners();
		s_ringScale.Set(m_ringBillBoardSize, m_ringBillBoardSize, m_ringBillBoardSize);
		m_oneOverFps = 1f / (float)m_billboardFps;
		m_fpsTimer = 0f;
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("DisableGameState", this);
		EventDispatch.RegisterInterest("ResetGameState", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("OnDashMeterFilled", this);
		EventDispatch.RegisterInterest("OnDashMeterEmpty", this);
		EventDispatch.RegisterInterest("CharacterLoaded", this);
		m_pickupSfxPitch = m_pickupSfxPitchMin;
		m_lastPickupTime = 0f;
		m_pitchChangePerSec = (m_pickupSfxPitchMax - m_pickupSfxPitchMin) / m_pickupSfxPitchChangeDuration;
	}

	public void Event_StartGameState(GameState.Mode state)
	{
		if (state == GameState.Mode.Game)
		{
			m_magnetPermitted = true;
		}
	}

	private void Event_DisableGameState(GameState.Mode nextState)
	{
		m_magnetPermitted = false;
	}

	private void Event_CharacterLoaded()
	{
		m_worldContainerTransform = SonicSplineTracker.FindRootTransform();
	}

	private void Event_ResetGameState(GameState.Mode nextState)
	{
		m_magnetPermitted = false;
		m_pickupSfxPitch = m_pickupSfxPitchMin;
		m_lastPickupTime = 0f;
	}

	private void Event_OnDashMeterFilled()
	{
		m_playAltAudioclip = false;
	}

	private void Event_OnDashMeterEmpty()
	{
		m_playAltAudioclip = true;
	}

	private void Update()
	{
		if (!(m_mainCamera == null))
		{
			if (m_magnetPermitted)
			{
				UpdateMagnetism();
			}
			RenderRingSequences2D();
		}
	}

	private void UpdateMagnetism()
	{
		bool flag = MagnetMonitor.instance().isMagnetised();
		Vector3 position = Sonic.Transform.position;
		position.y += 0.5f;
		float num = 20f;
		float num2 = num * num;
		float num3 = 0.6f;
		RingSequence[] sequences = m_ringGenerator.GetSequences();
		foreach (RingSequence ringSequence in sequences)
		{
			if ((!ringSequence.RingsAvailable && !ringSequence.RingsCollected) || !ringSequence.Collectable)
			{
				continue;
			}
			for (int j = 0; j < ringSequence.Length; j++)
			{
				RingSequence.Ring ring = ringSequence.GetRing(j);
				if (flag && !ring.isMagnetised)
				{
					float sqrMagnitude = (position - ring.m_position).sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						ring.Magnetise();
					}
				}
				if (ring.isMagnetised)
				{
					float num4 = ring.m_magnetismTime / num3;
					if (num4 > 1f)
					{
						num4 = 1f;
						ring.m_forceCollecion = true;
					}
					Transform transform = ((!(ring.m_owningObject != null)) ? null : ring.m_owningObject.transform);
					if (!(transform == null))
					{
						Vector3 from = transform.TransformPoint(ring.m_preMagnetisedLocalPosition);
						ring.m_position = Vector3.Lerp(from, position, num4);
						ring.m_magnetismTime += Time.deltaTime;
					}
				}
			}
		}
	}

	private void RenderRingSequences2D()
	{
		if (null == m_worldContainerTransform || null == Sonic.Transform)
		{
			return;
		}
		bool flag = false;
		UpdateBillboardFrame();
		SetMeshTextureFrame(m_panelRingMesh, m_billboardFrame, m_ringUVs);
		Vector3 position = m_mainCamera.transform.position;
		RingSequence[] sequences = m_ringGenerator.GetSequences();
		for (int num = sequences.Length - 1; num >= 0; num--)
		{
			RingSequence ringSequence = sequences[num];
			if (!ringSequence.RingsAvailable && !ringSequence.RingsCollected)
			{
				continue;
			}
			if (ringSequence.IsSequencePositionSupported && !ringSequence.RingsCollected)
			{
				Vector3 vector = m_worldContainerTransform.TransformPoint(ringSequence.WorldLocalSequencePosition);
				float sqrMagnitude = (vector - Sonic.Transform.position).sqrMagnitude;
				if (sqrMagnitude - ringSequence.SqSequenceRadius > m_maxSqRenderDistance)
				{
					ringSequence.ClearCollection();
					continue;
				}
			}
			for (int num2 = ringSequence.Length - 1; num2 >= 0; num2--)
			{
				RingSequence.Ring ring = ringSequence.GetRing(num2);
				if (ring.m_occupied)
				{
					Vector3 forward = position - ring.m_position;
					Quaternion q = Quaternion.LookRotation(forward, Vector3.up);
					q *= s_billboardOrientation;
					Graphics.DrawMesh(m_panelRingMesh, Matrix4x4.TRS(ring.m_position, q, s_ringScale), m_animatedRingMaterial, 0);
				}
				else if (ring.m_collected)
				{
					PlayPickUpEffect(ring);
					if (!flag)
					{
						PlayPickUpAudio(ring);
						flag = true;
					}
					ringSequence.ClearCollection(ring);
				}
			}
			ringSequence.ClearCollection();
		}
	}

	private void SetMeshTextureFrame(Mesh thisMesh, int textureFrame, Vector2[] uvs)
	{
		float num = textureFrame % 4;
		float num2 = Mathf.Floor((float)textureFrame / 4f);
		uvs[0].x = 0.25f * num;
		uvs[0].y = 0.25f * num2;
		uvs[1].x = 0.25f * (num + 1f);
		uvs[1].y = 0.25f * num2;
		uvs[2].x = 0.25f * num;
		uvs[2].y = 0.25f * (num2 + 1f);
		uvs[3].x = 0.25f * (num + 1f);
		uvs[3].y = 0.25f * (num2 + 1f);
		thisMesh.uv = uvs;
	}

	private void UpdateBillboardFrame()
	{
		m_fpsTimer += Time.deltaTime;
		if (m_fpsTimer >= m_oneOverFps)
		{
			m_billboardFrame++;
			if (m_billboardFrame >= 16)
			{
				m_billboardFrame = 0;
			}
			m_fpsTimer -= m_oneOverFps;
		}
	}

	private void CreatePools<T>(T sourceObject, ref T[] objectPool, int poolSize, ref int currentCount) where T : Component
	{
		objectPool = new T[poolSize];
		objectPool[0] = sourceObject;
		for (int i = 1; i < poolSize; i++)
		{
			objectPool[i] = Object.Instantiate(sourceObject) as T;
			objectPool[i].gameObject.transform.parent = sourceObject.transform.parent;
		}
		currentCount = 0;
	}

	private void CreateMeshCombiners()
	{
	}

	private void FindMainCamera()
	{
		if (!(m_mainCamera != null))
		{
			m_mainCamera = Object.FindObjectOfType(typeof(CameraTypeMain)) as CameraTypeMain;
		}
	}

	private void PlayPickUpEffect(RingSequence.Ring thisRing)
	{
		ParticleSystem particleSystem = m_pickUpEffectPool[m_currentPickUpEffect];
		particleSystem.transform.position = thisRing.m_position;
		ParticlePlayer.Play(particleSystem, ParticlePlayer.Important.Yes);
		m_currentPickUpEffect++;
		if (m_currentPickUpEffect >= m_pickUpEffectPool.Length)
		{
			m_currentPickUpEffect = 0;
		}
	}

	private void PlayPickUpAudio(RingSequence.Ring thisRing)
	{
		float num = Time.time - m_lastPickupTime;
		m_lastPickupTime = Time.time;
		if (num > m_pickupSfxPitchChangeGapSeconds)
		{
			m_pickupSfxPitch = m_pickupSfxPitchMin;
		}
		else
		{
			m_pickupSfxPitch += num * m_pitchChangePerSec;
			if (m_pickupSfxPitch >= m_pickupSfxPitchMax)
			{
				m_pickupSfxPitch = m_pickupSfxPitchMin;
			}
		}
		if (m_playAltAudioclip)
		{
			Audio.PlayClip(m_pickUpAudioClipAlt, loop: false, m_pickupSfxPitch, 0.4f);
		}
		Audio.PlayClip(m_pickUpAudioClip, loop: false, 1f, 1f);
	}
}
