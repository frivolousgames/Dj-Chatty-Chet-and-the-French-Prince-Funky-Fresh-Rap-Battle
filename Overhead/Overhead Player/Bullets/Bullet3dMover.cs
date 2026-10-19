using Godot;
using System;

public partial class Bullet3dMover : RigidBody3D
{
    [Export]
    public float Velocity;
    Vector3 dir;
    [Export]
    public float WaitTime;
    [Export]
    public RayCast3D Ray;

    [Export]
    public PackedScene BloodSpatter;
    [Export]
    public PackedScene Sparks;


    public override void _Ready()
    {
        dir = OverheadPlayer.bulletDirection;
        ApplyCentralImpulse(dir * Velocity);

        Timer timer = new Timer();
        AddChild(timer);
        timer.WaitTime = WaitTime;
        timer.Timeout += DestroyBullet;
        timer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        CheckCollision();
    }

    void CheckCollision()
    {
        if (Ray.IsColliding())
        {
            Object col = Ray.GetCollider();
            if(col is Node3D collider)
            {
                if (collider.IsInGroup("Bleedable"))
                {
                    //GD.Print("Hit Bleedable");
                    var blood = BloodSpatter.Instantiate<GpuParticles3D>();
                    GetTree().Root.AddChild(blood);
                    blood.GlobalPosition = Ray.GetCollisionPoint();
                    blood.GlobalRotation = Ray.GetCollisionNormal();
                    //blood.Scale = new Vector3(.1f, .1f, .1f);
                    if (collider is Pedestrian ped)
                    {
                        ped.bulletVelocity = LinearVelocity;
                        ped.BulletHit();
                        ped.bulletPosition = Ray.GetCollisionPoint();
                    }
                    DestroyBullet();
                }
                if (collider.IsInGroup("Vehicle Static Hit"))
                {
                    //GD.Print("Hit Sparkable");
                    var sparks = Sparks.Instantiate<Node3D>();
                    GetTree().Root.AddChild(sparks);
                    sparks.GlobalPosition = Ray.GetCollisionPoint();
                    sparks.GlobalRotation = Ray.GetCollisionNormal();
                    sparks.Scale = new Vector3(.2f, .2f, .2f);
                    DestroyBullet();
                }
            }
        }
    }

    void DestroyBullet()
    {
        //GD.Print("Dead");
        QueueFree();
    }
}
