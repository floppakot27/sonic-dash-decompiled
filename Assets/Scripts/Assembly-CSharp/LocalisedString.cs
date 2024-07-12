using UnityEngine;

[RequireComponent(typeof(LocalisedStringProperties))]
public abstract class LocalisedString : MonoBehaviour
{
	protected enum UpdateState
	{
		Start,
		Update
	}

	protected LocalisedStringProperties Properties { get; private set; }

	public void ForceStringUpdate()
	{
		if (!(Properties == null))
		{
			UpdateGuiText(UpdateState.Start);
		}
	}

	protected abstract void UpdateGuiText(UpdateState updateState);

	private void Start()
	{
		GetPropertiesComponent();
		UpdateGuiText(UpdateState.Start);
	}

	private void Update()
	{
		UpdateGuiText(UpdateState.Update);
	}

	private void GetPropertiesComponent()
	{
		Properties = GetComponent<LocalisedStringProperties>();
		if (Properties == null)
		{
			throw new UnityException($"LocalisedString::GetPropertiesComponent - Unable to find the properties node for the localised string '{base.name}'");
		}
	}
}
