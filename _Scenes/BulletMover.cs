using Godot;
using System;

public partial class BulletMover : RigidBody2D
{
	[Export]
	public float velocity;
	Vector2 dir;
	[Export]
	public float waitTime;

	public override void _Ready()
	{
		dir = Vector2.Up;
		ApplyCentralImpulse(dir * velocity);

		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = waitTime;
		timer.Timeout += DestroyBullet;
		timer.Start();
	}

	void DestroyBullet()
	{
		GD.Print("Dead");
		QueueFree();
	}
}
