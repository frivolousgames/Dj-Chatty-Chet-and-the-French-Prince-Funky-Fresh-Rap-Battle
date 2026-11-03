using Godot;
using System;

public partial class CarDoor : MeshInstance3D
{
	public bool isOpen;

	AnimationTree animTree;
	public override void _Ready()
	{
		animTree = GetNode<AnimationTree>("AnimationTree");
	}

	public override void _Process(double delta)
	{
		animTree.Set("parameters/StateMachine/conditions/is_closed", !isOpen);
		animTree.Set("parameters/StateMachine/conditions/is_open", isOpen);
	}
}
