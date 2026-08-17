using Godot;
using System;

public partial class TiretrackDecal : Decal
{
    [Export]
    public float WaitTime { get; set; }

    public override void _Ready()
	{
        Timer timer = new Timer();
        this.AddChild(timer);
        timer.WaitTime = WaitTime;
        timer.OneShot = true;
        timer.Start();
        timer.Timeout += () =>
        {
            FadeOut();
            timer.QueueFree();
        };
    }
    
    void FadeOut()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 1.5f);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
