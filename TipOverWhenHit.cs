using Godot;
using System;

public partial class TipOverWhenHit : RigidBody3D
{
	[Export]
	public SpotLight3D Spot {  get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Freeze = true;
		ContactMonitor = true;
		MaxContactsReported = 10;
		BodyEntered += Hit;

	}

	void Hit(Node body)
	{
		//GD.Print("Touched");
		if (body.IsInGroup("Player Vehicle"))
		{
			//GD.Print("Hit");
			if(Spot != null)
			{
				Spot.Visible = false;
			}
			Freeze = false;
		}
	}
}
