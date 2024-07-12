using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Sonic
{
	public delegate void OnMovementDelegate(SonicSplineTracker.MovementInfo info);

	public delegate void OnStrafeDelegate(SplineTracker newSplineTracker);

	private static GameObject TopLevelSonic { get; set; }

	public static GameObject ScoreAnchor { get; private set; }

	public static Transform Transform { get; private set; }

	public static Transform MeshTransform { get; private set; }

	public static SonicSplineTracker Tracker { get; private set; }

	public static SonicMenuAnimationsControl MenuAnimationControl { get; private set; }

	public static SonicAnimationControl AnimationControl { get; private set; }

	public static SonicHandling Handling { get; private set; }

	public static MotionMonitor MotionMonitor { get; private set; }

	public static SonicAudioControl AudioControl { get; private set; }

	public static SonicController Controller { get; private set; }

	public static SonicRenderManager RenderManager { get; private set; }

	public static ParticleControllerScript ParticleController { get; private set; }

	public static Dictionary<string, Transform> Bones { get; private set; }

	[method: MethodImpl(32)]
	public static event OnMovementDelegate OnMovementCallback;

	[method: MethodImpl(32)]
	public static event OnStrafeDelegate OnStrafeCallback;

	public static void Initialise()
	{
		if (null == TopLevelSonic)
		{
			TopLevelSonic = GameObject.FindWithTag("TopLevelSonic");
		}
		Handling = TopLevelSonic.GetComponentInChildren(typeof(SonicHandling)) as SonicHandling;
	}

	public static void FixupSonic()
	{
		Transform = Utils.FindTagInChildren(TopLevelSonic, "CharacterImportRoot").transform;
		ScoreAnchor = Utils.FindTagInChildren(TopLevelSonic, "SonicScoreAnchor");
		ScoreAnchor.SetActive(value: false);
		Tracker = TopLevelSonic.GetComponentInChildren(typeof(SonicSplineTracker)) as SonicSplineTracker;
		MenuAnimationControl = TopLevelSonic.GetComponentInChildren(typeof(SonicMenuAnimationsControl)) as SonicMenuAnimationsControl;
		AnimationControl = TopLevelSonic.GetComponentInChildren(typeof(SonicAnimationControl)) as SonicAnimationControl;
		GameObject gameObject = GameObject.FindWithTag("CharacterMesh");
		MeshTransform = gameObject.transform;
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		GameObject gameObject2 = Utils.FindTagInChildren(gameObject, "BoneRoot");
		Transform[] componentsInChildren = gameObject2.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (!dictionary.ContainsKey(transform.name))
			{
				dictionary.Add(transform.name, transform);
			}
		}
		Bones = dictionary;
		MotionMonitor = TopLevelSonic.GetComponentInChildren(typeof(MotionMonitor)) as MotionMonitor;
		AudioControl = TopLevelSonic.GetComponentInChildren(typeof(SonicAudioControl)) as SonicAudioControl;
		Controller = TopLevelSonic.GetComponentInChildren(typeof(SonicController)) as SonicController;
		RenderManager = TopLevelSonic.GetComponentInChildren(typeof(SonicRenderManager)) as SonicRenderManager;
		ParticleController = TopLevelSonic.GetComponentInChildren(typeof(ParticleControllerScript)) as ParticleControllerScript;
	}

	public static void Clear()
	{
		Transform = null;
		Tracker = null;
		MenuAnimationControl = null;
		AnimationControl = null;
		MotionMonitor = null;
		AudioControl = null;
		Controller = null;
		Bones = null;
		RenderManager = null;
		ParticleController = null;
	}

	public static void fireMovementEvent(SonicSplineTracker.MovementInfo info)
	{
		if (Sonic.OnMovementCallback != null)
		{
			Sonic.OnMovementCallback(info);
		}
	}

	public static void fireStrafeEvent(SplineTracker newSplineTracker)
	{
		if (Sonic.OnStrafeCallback != null)
		{
			Sonic.OnStrafeCallback(newSplineTracker);
		}
	}
}
