using Godot;
using System;

public partial class MuzzleFlash : Node3D
{
    BaseMaterial3D material;
    Sprite3D sprite;
    [Export]
    public Texture2D[] flashTexts {  get; set; }
    [Export]
    public float waitTime { get; set; }
    public override void _Ready()
    {
        sprite = GetNode<Sprite3D>("Sprite3D");

        material = sprite.MaterialOverride as BaseMaterial3D;
        int rand = GD.RandRange(0, flashTexts.Length - 1);
        material.AlbedoTexture = flashTexts[rand];
        int rand2 = GD.RandRange(0, 1);
        sprite.FlipH = rand2 == 0 ? true : false;
        GD.Print(material.AlbedoTexture.ResourceName);

        Timer timer = new Timer();
        AddChild(timer);
        timer.WaitTime = waitTime;
        timer.Timeout += DestroyFlash;
        timer.Start();
    }

    void DestroyFlash()
    {
        QueueFree();
    }
}
