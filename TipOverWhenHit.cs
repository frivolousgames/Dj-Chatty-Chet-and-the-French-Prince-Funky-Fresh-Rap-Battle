using Godot;
using System;

public partial class TipOverWhenHit : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Freeze = true;
        ContactMonitor = true;
        MaxContactsReported = 5;
        BodyEntered += Hit;

    }

    void Hit(Node body)
    {
        if (body.IsInGroup("Player Vehicle"))
        {
            GD.Print("Hit");
            Freeze = false;
        }
    }
}
