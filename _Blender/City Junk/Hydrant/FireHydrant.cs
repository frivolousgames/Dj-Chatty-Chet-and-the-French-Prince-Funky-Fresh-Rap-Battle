using Godot;
using System;

public partial class FireHydrant : GpuParticles3D
{
	[Export]
	public RigidBody3D rb;
	public override void _Ready()
	{
		rb.ContactMonitor = true;
		rb.MaxContactsReported = 5;
		rb.BodyEntered += OnHit;
	}

    private void OnHit(Node body)
    {
		if (body.IsInGroup("Player Vehicle"))
		{
            this.Emitting = true;

        }
    }
}
