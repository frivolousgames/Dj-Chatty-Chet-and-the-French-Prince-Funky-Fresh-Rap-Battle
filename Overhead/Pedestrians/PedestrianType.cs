using Godot;
using System;

[GlobalClass]
public partial class PedestrianType : Resource
{
    [Export]
    public bool IsMale { get; set; }
    [Export]
    public bool HasStroller { get; set; }
    [Export]
    public float RunSpeed { get; set; }
    [Export]
    public float WalkSpeed { get; set; }
    [Export]
    public float InjuredRunSpeed { get; set; }
    [Export]
    public int Health { get; set; }
}
