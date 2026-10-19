using Godot;
using System;

public partial class RagdollTrigger : Area3D
{
	public override void _Ready()
	{
		BodyEntered += VehicleHit;
	}

	void VehicleHit(Node3D body)
	{
		if(body is CarMover car)
		{
            if (car.LinearVelocity.Length() > 1f)
            {
                GetParent<Pedestrian>().isLimb = true;
            }
            if (car.LinearVelocity.Length() > .1f)
			{
                GetParent<Pedestrian>().CarHit();
            }
		}
	}
}
