using Godot;
using System;
using System.ComponentModel;

public partial class OverheadPlayer : CharacterBody3D
{
	[Export]
	public float Speed;
	[Export]
	public float JumpVelocity;

	[Export]
	public AnimationTree animTree;

	public bool isIdle;
	public bool isMoving;

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		float inputRotDir = Input.GetAxis("ui_left", "ui_right");
		Vector3 rotDir = Transform.Basis.X * inputRotDir;
		Transform.LookingAt(rotDir);

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float inputDir = Input.GetAxis("ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(0, 0, inputDir)).Normalized();
		if (direction != Vector3.Zero)
		{
			//velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			//velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		///Animation Conditions
		animTree.Set("parameters/conditions/is_idle", velocity == Vector3.Zero && Input.GetAxis("ui_left", "ui_right") == 0);
		animTree.Set("parameters/conditions/is_moving", velocity != Vector3.Zero);
        animTree.Set("parameters/conditions/is_turning", velocity == Vector3.Zero && Input.GetAxis("ui_left", "ui_right") > 0);
    }

//    instead of:
//if velocity == Vector2.ZERO:
//	animation_tree["parameters/conditions/idle"] = true
//	animation_tree["parameters/conditions/is_moving"] = false
//else:
//	animation_tree["parameters/conditions/idle"] = false
//	animation_tree["parameters/conditions/is_moving"] = true
//if Input.is_action_just_pressed("use"):
//	animation_tree["parameters/conditions/swing"] = true
//else:
//	animation_tree["parameters/conditions/swing"] = false

//do this:
//animation_tree.set("parameters/conditions/idle", velocity == Vector2.ZERO)
//animation_tree.set("parameters/conditions/is_moving", velocity != Vector2.ZERO)
//animation_tree.set("parameters/conditions/swing", Input.is_action_just_pressed("use") )
}
