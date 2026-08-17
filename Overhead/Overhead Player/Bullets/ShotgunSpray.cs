using Godot;
using System;

public partial class ShotgunSpray : Area3D
{
    [Export]
    public float waitTime { get; set; }

    [Export]
    public GpuParticles3D particles3D { get; set; }
    public override void _Ready()
	{
        particles3D.Emitting = true;

		Timer timer = new Timer();
        AddChild(timer);
        timer.WaitTime = waitTime;
        timer.Timeout += DestroyBullet;
        timer.Start();
    }

    void DestroyBullet()
    {
        QueueFree();
    }	
}
