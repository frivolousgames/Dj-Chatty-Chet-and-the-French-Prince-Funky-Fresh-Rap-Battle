using Godot;
using System;

public partial class SetOverheadWeapon : Button
{
	[Export]
	public int Weapon {  get; set; }

    public override void _Ready()
    {
        this.Pressed += ButtonPress;
    }

    void ButtonPress()
    {
        OverheadPlayer.selectedWeapon = Weapon;
    }
}
