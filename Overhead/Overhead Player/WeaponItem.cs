using Godot;
using System;

[GlobalClass]
public partial class WeaponItem : Resource
{
    [Export]
    public string WeaponName { get; set; }
    [Export]
    public bool IsOneHanded { get; set; }
    [Export]
    public PackedScene WeaponBullet { get; set; }
    [Export]
    public int WeaponDamage { get; set; }
    [Export]
    public float WeaponForce { get; set; }
}
