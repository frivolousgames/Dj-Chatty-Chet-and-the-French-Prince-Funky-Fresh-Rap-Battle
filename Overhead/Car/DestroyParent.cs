using Godot;
using System;

public partial class DestroyParent : GpuParticles3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Finished += Destroy;
	}

	void Destroy()
	{
		GetParent<Node3D>().QueueFree();
	}
}
