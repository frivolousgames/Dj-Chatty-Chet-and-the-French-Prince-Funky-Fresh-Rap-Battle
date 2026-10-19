using Godot;
using System;

public partial class DestroyParticleSystem : GpuParticles3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OneShot = true;
		Emitting = true;
		Finished += QueueFree;
	}
}
