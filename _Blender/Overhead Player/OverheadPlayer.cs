using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class OverheadPlayer : CharacterBody3D
{
	[Export]
	public float WalkSpeed;
	[Export]
	public float RunSpeed;
	[Export]
	public float WalkBackSpeed;
	float speed;
	float tempSpeed;
	bool walkBackReset;
	[Export]
	public float JumpVelocity;
	[Export]
	public float RotSpeed;

	[Export]
	public AnimationPlayer AnimPlayer;
	[Export]
	public AnimationTree AnimTree;


	//Animation
	bool isMoving;
	bool isRunning;
	bool isRaising;
	bool isWeaponRaised;
	bool shotStarted;
	bool isShooting;
	bool isTurning;
	bool isWalkBackward;

	//Run Blends
	float currentRunBlend;
	float targetRunBlend;
	float blendSpeed = 10f;
	float newRunBlend;

	string[] runBlends = new string[] //Add all run blends for every weapon
	{
		"parameters/Glock/Move Forward/Run Blend/blend_amount"
	};

    //Raised Blends
    float currentRaisedBlend;
    float targetRaisedBlend;
    float newRaisedBlend;

    //Shoot Blends
    float currentShootBlend;
	float targetShootBlend;
	float newShootBlend;

	//Weapons
	[Export]
	public MeshInstance3D[] Weapons;
	MeshInstance3D[] availableWeapons;
	string currentWeapon;
	string selectedWeapon; //set this when equipping new weapon



	//All Shoot Weapon Blends
	string[] stanceBlends = new string[] //Add all raised weapon blends from every weapon
	{
		//Glock
        "parameters/Glock/Idle/Idle Stance Blend/blend_amount",
        "parameters/Glock/Turn/Turn Stance Blend/blend_amount",
        "parameters/Glock/Move Forward/Move Forward Stance Blend/blend_amount",
        "parameters/Glock/Move Backward/Walk Back Stance Blend/blend_amount"
    };
	string[] shootBlends = new string[] //Add all shoot blends from every weapon
	{
		//Glock
        "parameters/Glock/Idle/Idle Shoot Blend/blend_amount",
		"parameters/Glock/Turn/Turn Shoot Blend/blend_amount",
		"parameters/Glock/Move Forward/Move Forward Shoot Blend/blend_amount",
		"parameters/Glock/Move Backward/Walk Back Shoot Blend/blend_amount"
	};



	public override void _Ready()
	{
		AnimTree.Active = true;
		speed = WalkSpeed;
        walkBackReset = true;
        //Weapons
        //currentWeapon = availableWeapons[0].Name;
	}

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

		///Rotation
		float inputRotDir = Input.GetAxis("ui_right", "ui_left");
		Vector3 rotDir = new Vector3(0f, inputRotDir, 0f);
		Rotation += rotDir * RotSpeed;


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float inputDir = Input.GetAxis("ui_up", "ui_down");
		Vector3 direction = GlobalTransform.Basis.Z;
		if (direction != Vector3.Zero)
		{
			//velocity.X = direction.X * speed;
			velocity = direction * inputDir * speed;
		}
		else
		{
			//velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}

		Velocity = velocity;
		Transform.LookingAt(Position + direction);
		MoveAndSlide();

		///Animation Conditions
		if(inputDir < 0f)
		{
			isMoving = true;
			if (isRunning)
			{
				speed = RunSpeed;
			}
			else
			{
				speed = WalkSpeed;
			}
			//if(!isShooting && isWeaponRaised)
			//{
			//	isWeaponRaised = false;
			//}
		}
		else
		{
			isMoving = false;
		}

		if (inputDir > 0f)
		{
			isWalkBackward = true;
			speed = WalkBackSpeed;
		}
		else
		{
			isWalkBackward = false;
		}

		isTurning = velocity == Vector3.Zero && Mathf.Abs(Input.GetAxis("ui_left", "ui_right")) > 0 ? true : false;
		IsRunning(delta);
		IsShooting(delta);
		//Glock
		AnimTree.Set("parameters/Glock/conditions/is_moving", isMoving);
		AnimTree.Set("parameters/Glock/conditions/is_not_moving", !isMoving);
		AnimTree.Set("parameters/Glock/conditions/is_turning", isTurning);
		AnimTree.Set("parameters/Glock/conditions/is_not_turning", !isTurning);
		AnimTree.Set("parameters/Glock/conditions/is_walk_backward", isWalkBackward);
		AnimTree.Set("parameters/Glock/conditions/is_not_walk_backward", !isWalkBackward);

		//Blends


	}

	void IsRunning(double delta)
	{
		if (Input.IsActionJustPressed("Run") && isMoving)
		{
			GD.Print("Pressed");
			if (isRunning)
			{
				isRunning = false;
				speed = WalkSpeed;
				targetRunBlend = 0f;
			}
			else
			{
				isRunning = true;
				speed = RunSpeed;
				targetRunBlend = 1f;
			}
		}
		currentRunBlend = (float)AnimTree.Get("parameters/Glock/Move Forward/Run Blend/blend_amount");
		if (newRunBlend != targetRunBlend)
		{
			newRunBlend = Mathf.Lerp(currentRunBlend, targetRunBlend, blendSpeed * (float)delta);
			foreach (var blend in runBlends)
			{
				AnimTree.Set(blend, (Variant)newRunBlend);
			}
			if(Mathf.Abs(newRunBlend - targetRunBlend) < .001f)
			{
				newRunBlend = targetRunBlend;
			}
		}
	}

	void IsShooting(double delta)
	{
		if (Input.IsActionPressed("Shoot") || (Input.IsActionJustPressed("Shoot")))
		{
			//GD.Print("Shoot Pressed");
			if (isWeaponRaised)
			{
				if(!shotStarted)
				{
					shotStarted = true;
                    targetShootBlend = 1f;
                    currentShootBlend = (float)AnimTree.Get("parameters/Glock/Idle/Idle Shoot Blend/blend_amount");
                    ShootWeapon(delta);
				}
                
            }
			else
			{
				if (!isRaising)
				{
					isRaising = true;
                    targetRaisedBlend = 1f;
                    currentRaisedBlend = (float)AnimTree.Get("parameters/Glock/Idle/Idle Stance Blend/blend_amount");
					RaiseWeapon(delta);
                    
                }
                
            }
		}
		//else
		//{
		//	isShooting = false;
		//	targetShootBlend = 0f;
		//}
		//currentShootBlend = (float)AnimTree.Get("parameters/Glock/Idle/Idle Shoot Blend/blend_amount");
		//if (newShootBlend != targetShootBlend)
		//{
		//	GD.Print("Current: " + currentShootBlend);
		//	newShootBlend = Mathf.Lerp(currentShootBlend, targetShootBlend, blendSpeed * (float)delta);
		//	foreach (var blend in shootBlends)
		//	{
		//		AnimTree.Set(blend, (Variant)newShootBlend);
		//	}
		//	if (Mathf.Abs(newShootBlend - targetShootBlend) < .001f)
		//	{
		//		newShootBlend = targetShootBlend;
		//	}
		//}
	}
	//private async Task RaiseWeapon(double delta)
	//{
	//	targetRaisedBlend = 1f;
 //       currentRaisedBlend = (float)AnimTree.Get("parameters/Glock/Idle/Idle Stance Blend/blend_amount");
 //       while (newRaisedBlend != targetRaisedBlend)
 //       {
 //           GD.Print("Current: " + newRaisedBlend);
 //           newRaisedBlend = Mathf.Lerp(currentRaisedBlend, targetRaisedBlend, blendSpeed * (float)delta);
 //           foreach (var blend in stanceBlends)
 //           {
 //               AnimTree.Set(blend, (Variant)newRaisedBlend);
 //           }
 //           if (Mathf.Abs(newRaisedBlend - targetRaisedBlend) < .001f)
 //           {
 //               newRaisedBlend = targetRaisedBlend;
 //           }
	//		currentRaisedBlend = newRaisedBlend;
	//		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
 //       }
	//	isWeaponRaised = true;
 //   }

	void RaiseWeapon(double delta)
	{
        _ = BlendLoop(delta, newRaisedBlend, currentRaisedBlend, targetRaisedBlend, stanceBlends);
		if (!isWeaponRaised)
		{
            isWeaponRaised = true;
        }
		else
		{
            isWeaponRaised = false;
        }
			
    }
  //  private async Task ShootWeapon(double delta)
  //  {
  //      currentShootBlend = (float)AnimTree.Get("parameters/Glock/Idle/Idle Shoot Blend/blend_amount");
  //      while (newShootBlend != targetShootBlend)
  //      {
  //          GD.Print("Current: " + currentShootBlend);
  //          newShootBlend = Mathf.Lerp(currentShootBlend, targetShootBlend, blendSpeed * (float)delta);
  //          foreach (var blend in shootBlends)
  //          {
  //              AnimTree.Set(blend, (Variant)newShootBlend);
  //          }
  //          if (Mathf.Abs(newShootBlend - targetShootBlend) < .001f)
  //          {
  //              newShootBlend = targetShootBlend;
  //          }
		//	currentShootBlend = newShootBlend;
  //          await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
  //      }
		//while (isShooting)
		//{
  //          GD.Print("isShooting: " + isShooting);
  //          await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
  //      }
  //  }
	void ShootWeapon(double delta)
	{
        _ = BlendLoop(delta, newShootBlend, currentShootBlend, targetShootBlend, shootBlends);
		_ = ResetIsShooting();
    }

	private async Task ResetIsShooting()
	{
        await ToSignal(AnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
		if(AnimPlayer.AnimationFinished == )
        isShooting = false;
		GD.Print("Done Shooting");
	}
    private async Task BlendLoop(double delta, float newBlend, float currentBlend, float targetBlend, string[] blends)
	{
        while (newBlend != targetBlend)
        {
            GD.Print("Current: " + currentBlend);
            newBlend = Mathf.Lerp(currentBlend, targetBlend, blendSpeed * (float)delta);
            foreach (var blend in blends)
            {
                AnimTree.Set(blend, (Variant)newBlend);
            }
            if (Mathf.Abs(newBlend - targetBlend) < .001f)
            {
                newBlend = targetBlend;
            }
            currentBlend = newBlend;
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
    }
       
    
    //Weapons
    //void CheckWeaponAvailability()
    //{
    //	when picking up weapon run a loop to compare and add to availableWeapons
    //}
    void SetShootBlend() //Run this when new weapon is selected
    {

    }
}
