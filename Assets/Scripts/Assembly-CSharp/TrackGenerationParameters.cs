using System;
using UnityEngine;

[AddComponentMenu("Dash/Track/Track Generation Parameters")]
public class TrackGenerationParameters : MonoBehaviour
{
	public const int MaxBossBattleSpringChances = 10;

	public const int MaxBossBattleNotEncounteredChances = 5;

	[SerializeField]
	private float m_minTrackDistanceToGenerate = 200f;

	[SerializeField]
	private float m_minStraightLength = 150f;

	[SerializeField]
	private float m_initialTemplateFreeDistance = 16f;

	[SerializeField]
	private float m_springLandingTemplateFreeDistance = 16f;

	[SerializeField]
	private float m_afterSetPieceTemplateFreeDistance = 16f;

	[SerializeField]
	private float m_chanceSBend = 0.1f;

	[SerializeField]
	private int m_firstSubzoneIndex;

	[SerializeField]
	private float m_grassHeightOffset;

	[SerializeField]
	private float m_templeHeightOffset;

	[SerializeField]
	private float m_beachHeightOffset;

	[SerializeField]
	private int m_lowElevationMinSegmentCount = 8;

	[SerializeField]
	private int m_lowElevationMaxSegmentCount = 12;

	[SerializeField]
	private int m_highElevationMinSegmentCount = 7;

	[SerializeField]
	private int m_highElevationMaxSegmentCount = 10;

	[SerializeField]
	private int m_hillsMinSegmentCount = 7;

	[SerializeField]
	private int m_hillsMaxSegmentCount = 10;

	[SerializeField]
	private float m_chanceStayOnLowElevation = 0.5f;

	[SerializeField]
	private float m_chanceSwitchToHighElevation = 0.5f;

	[SerializeField]
	private float m_chanceHillyElevationChange = 0.33f;

	[SerializeField]
	private int m_setPiecesPerSetPieceTrack = 2;

	[SerializeField]
	private float m_setPieceHeightToggleTrigger;

	[SerializeField]
	private string m_setPieceHeightToggleOnAbove;

	[SerializeField]
	private string m_setPieceHeightToggleOnBelow;

	[SerializeField]
	private float m_bossBattleNeverSeenChance = 0.5f;

	[SerializeField]
	private int m_bossBattleIncreaseDifficulty = 10;

	[SerializeField]
	private float[] m_bossBattleSpringFirstChance = new float[10];

	[SerializeField]
	private float[] m_bossBattleSpringSecondChance = new float[10];

	[SerializeField]
	private float[] m_bossBattleNotEncounteredRunsModifier = new float[5];

	[SerializeField]
	private AnimationCurve m_bankSpringsPerTrackSection;

	[SerializeField]
	private int m_bankSpringsOmitFrequency;

	public float GameplayMinTrackDistanceToGenerate => m_minTrackDistanceToGenerate;

	public float MinStraightLength => m_minStraightLength;

	public float InitialTemplateFreeDistance => m_initialTemplateFreeDistance;

	public float SpringLandingTemplateFreeDistance => m_springLandingTemplateFreeDistance;

	public float AfterSetPieceTemplateFreeDistance => m_afterSetPieceTemplateFreeDistance;

	public float ChanceSBend => m_chanceSBend;

	public int FirstSubzoneIndex => m_firstSubzoneIndex;

	public int LowElevationMinSegmentCount => m_lowElevationMinSegmentCount;

	public int LowElevationMaxSegmentCount => m_lowElevationMaxSegmentCount;

	public int HighElevationMinSegmentCount => m_highElevationMinSegmentCount;

	public int HighElevationMaxSegmentCount => m_highElevationMaxSegmentCount;

	public int HillsMinSegmentCount => m_hillsMinSegmentCount;

	public int HillsMaxSegmentCount => m_hillsMaxSegmentCount;

	public float ChanceStayOnLowElevation => m_chanceStayOnLowElevation;

	public float ChanceSwitchToHighElevation => m_chanceSwitchToHighElevation;

	public float ChanceHillyElevationChange => m_chanceHillyElevationChange;

	public int SetPiecePerSetPieceTrack => m_setPiecesPerSetPieceTrack;

	public float SetPieceHeightToggleTrigger => m_setPieceHeightToggleTrigger;

	public string SetPieceHeightToggleOnAbove => m_setPieceHeightToggleOnAbove;

	public string SetPieceHeightToggleOnBelow => m_setPieceHeightToggleOnBelow;

	public float BossBattleNeverSeenChance => m_bossBattleNeverSeenChance;

	public int BossBattleIncreaseDifficulty => m_bossBattleIncreaseDifficulty;

	public int BankSpringsOmitFrequency => m_bankSpringsOmitFrequency;

	public float SubzoneWorldHeightOffset(int subzoneIndex)
	{
		return subzoneIndex switch
		{
			0 => m_grassHeightOffset, 
			1 => m_templeHeightOffset, 
			2 => m_beachHeightOffset, 
			_ => 0f, 
		};
	}

	public float GetBossBattleSpringFirstChance(int springIndex)
	{
		springIndex = Math.Min(springIndex, 9);
		return m_bossBattleSpringFirstChance[springIndex];
	}

	public float GetBossBattleSpringSecondChance(int springIndex)
	{
		springIndex = Math.Min(springIndex, 9);
		return m_bossBattleSpringSecondChance[springIndex];
	}

	public float GetBossBattleNotEncounteredRunsModifier(int numRuns)
	{
		numRuns = Math.Min(numRuns, 4);
		return m_bossBattleNotEncounteredRunsModifier[numRuns];
	}

	public int GetBankSpringCount(int sectionsCompleted)
	{
		return Mathf.RoundToInt(m_bankSpringsPerTrackSection.Evaluate(sectionsCompleted));
	}
}
