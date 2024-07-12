using System.Linq;
using UnityEngine;

public class TrackSeedDisplay : MonoBehaviour
{
	private int m_seed;

	private string m_trackPieceName;

	private string m_templateName;

	private UILabel m_uiLabel;

	private void Start()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		m_uiLabel = GetComponent<UILabel>();
	}

	private void Update()
	{
		if (!(m_uiLabel == null))
		{
			m_uiLabel.text = string.Empty;
		}
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		TrackGenerator trackGenerator = Object.FindObjectOfType(typeof(TrackGenerator)) as TrackGenerator;
		m_seed = ((!(trackGenerator == null)) ? trackGenerator.Seed : (-1));
		m_trackPieceName = "(no track)";
		m_templateName = "(no tmplt)";
	}

	private void UpdateTrackInfo()
	{
		TrackSegment trackSegment = ((!(Sonic.Tracker.CurrentSpline == null)) ? TrackSegment.GetSegmentOfSpline(Sonic.Tracker.CurrentSpline) : null);
		if (!(trackSegment == null))
		{
			m_trackPieceName = trackSegment.Template.PieceType.Name;
			string text = string.Join(",", trackSegment.TemplateContainers.Select((Transform container) => container.gameObject.name).ToArray());
			m_templateName = ((!trackSegment.TemplateContainers.Any()) ? "(none)" : text);
			m_templateName = m_templateName.ToLower().Replace("template", string.Empty);
		}
	}
}
