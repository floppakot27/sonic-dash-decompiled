using UnityEngine;

[AddComponentMenu("Dash/Powerups/Powerup Generator")]
public class PowerupGenerator : HazardGenerator
{
	protected override SpawnPool Pool { get; set; }

	public override void Start()
	{
		base.Start();
		EventDispatch.RegisterInterest("SpringGestureSuccess", this);
		Pool = GetComponent<SpawnPool>();
	}

	private void Event_SpringGestureSuccess()
	{
		int ringsForRingsPickup = PowerUps.GetRingsForRingsPickup();
		PowerUps.DoRingPowerupAction(ringsForRingsPickup);
	}
}
