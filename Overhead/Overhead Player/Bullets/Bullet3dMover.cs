using Godot;
using System;

public partial class Bullet3dMover : RigidBody3D
{
    [Export]
    public float velocity;
    Vector3 dir;
    [Export]
    public float waitTime;

    public override void _Ready()
    {
        dir = OverheadPlayer.bulletDirection;
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
