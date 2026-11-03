using Godot;
using System;

public partial class RagdollSimulate : PhysicalBoneSimulator3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PhysicalBonesStartSimulation();
	}

	
}
