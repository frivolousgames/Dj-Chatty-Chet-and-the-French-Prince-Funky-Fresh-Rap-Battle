using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class OverheadPlayer : CharacterBody3D
{

    [Export]
	public float WalkSpeed { get; set; }
	[Export]
	public float RunSpeed { get; set; }
    [Export]
	public float WalkBackSpeed { get; set; }
    float speed;
	float tempSpeed;
	bool walkBackReset;
	[Export]
	public float JumpVelocity { get; set; }
    [Export]
	public float RotSpeed { get; set; }

    [Export]
	public AnimationPlayer AnimPlayer { get; set; }
    [Export]
	public AnimationTree AnimTree { get; set; }


    //Animation
    bool isMoving;
	bool isRunning;
	bool isRaising;
	bool raiseFinished;
	bool isWeaponRaised;
	bool shotStarted;
	bool isShooting;
	bool shotOver;
	bool isTurning;
	bool isStrafing;
	bool isWalkBackward;
	

	//Run Blends
	float currentRunBlend;
	float targetRunBlend;
	float blendSpeed = 10f;
	float newRunBlend;

	string[] runBlends = new string[] 
	{
        "parameters/One Handed Weapon/Locomotion/Move Forward/Run Blend/blend_amount",
        "parameters/Two Handed Weapon/Locomotion/Move Forward/Run Blend/blend_amount"
    };

    //Strafe Blends
    float currentStrafeBlend;
    float targetStrafeBlend;
    float newStrafeBlend;
    string[] strafeBlends = new string[]
    {
        "parameters/One Handed Weapon/Locomotion/Strafe/Strafe Dir Blend/blend_amount",
        "parameters/Two Handed Weapon/Locomotion/Strafe/Strafe Dir Blend/blend_amount"
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
	public WeaponItem[] WeaponResources { get; set; }
	[Export]
	public MeshInstance3D[] Weapons = [];
    int[] availableWeapons;
	int currentWeapon;
    public static int selectedWeapon { get; set; } //set this when equipping new weapon

    bool isOneHanded;

	//Bullets

	[Export]
	public Node3D[] BulletSpawns { get; set; }
	PackedScene currentBullet;
	public static Vector3 bulletDirection { get; set; }
	[Export]
	public PackedScene muzzleflash;

    ////All Weapon Blends

    string[] raisedBlends = new string[] //Add all raised weapon blends
	{
		//One Handed
        "parameters/One Handed Weapon/Locomotion/Idle/Raised Blend/blend_amount",
        "parameters/One Handed Weapon/Locomotion/Move Forward/Raised Blend/blend_amount",
        "parameters/One Handed Weapon/Locomotion/Move Backward/Raised Blend/blend_amount",
        "parameters/One Handed Weapon/Locomotion/Turn/Raised Blend/blend_amount",
        "parameters/One Handed Weapon/Locomotion/Strafe/Raised Blend/blend_amount",
		//Two Handed
		"parameters/Two Handed Weapon/Locomotion/Idle/Raised Blend/blend_amount",
        "parameters/Two Handed Weapon/Locomotion/Move Forward/Raised Blend/blend_amount",
        "parameters/Two Handed Weapon/Locomotion/Move Backward/Raised Blend/blend_amount",
        "parameters/Two Handed Weapon/Locomotion/Turn/Raised Blend/blend_amount",
        "parameters/Two Handed Weapon/Locomotion/Strafe/Raised Blend/blend_amount"
    };

	public override void _Ready()
	{
        //Weapons
        currentWeapon = SaveManager.LoadValue(GameManager.slotNumber, "Weapons", "Current", 2);
		isOneHanded = WeaponResources[currentWeapon].IsOneHanded;
        Weapons[currentWeapon].Visible = true;
		currentBullet = WeaponResources[currentWeapon].WeaponBullet;
		GD.Print("currentWeapon: " + currentWeapon);
        GD.Print("isOneHanded: " + isOneHanded);

        AnimTree.Active = true;
		speed = WalkSpeed;
        walkBackReset = true;
		AnimTree.AnimationFinished += ShotOver;
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
		if (!isStrafing)
		{
            float inputRotDir = Input.GetAxis("ui_right", "ui_left");
            Vector3 rotDir = new Vector3(0f, inputRotDir, 0f);
            Rotation += rotDir * RotSpeed;
        }
		


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

		//Strafe
		Vector3 strafeDir = GlobalTransform.Basis.X;
        if (Mathf.Abs(Input.GetAxis("ui_left", "ui_right")) > 0 && Input.IsActionPressed("Strafe"))
        {
            isStrafing = true;
            speed = WalkSpeed;
            velocity = strafeDir * Input.GetAxis("ui_left", "ui_right") * speed;
        }
        else
        {
            isStrafing = false;
        }
        Velocity = velocity;
        Transform.LookingAt(Position + direction);
        MoveAndSlide();

        isTurning = velocity == Vector3.Zero && Mathf.Abs(Input.GetAxis("ui_left", "ui_right")) > 0 ? true : false;
		IsRunning(delta);
		IsShooting(delta);
		RaiseWeapon(delta);
        Strafe(delta);

        //////Locomotion
        //One Handed
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_moving", isMoving);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_not_moving", !isMoving);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_turning", isTurning);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_not_turning", !isTurning);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_walk_backward", isWalkBackward);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_not_walk_backward", !isWalkBackward);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_strafing", isStrafing);
        AnimTree.Set("parameters/One Handed Weapon/Locomotion/conditions/is_not_strafing", !isStrafing);

        //Two Handed
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_moving", isMoving);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_not_moving", !isMoving);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_turning", isTurning);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_not_turning", !isTurning);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_walk_backward", isWalkBackward);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_not_walk_backward", !isWalkBackward);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_strafing", isStrafing);
        AnimTree.Set("parameters/Two Handed Weapon/Locomotion/conditions/is_not_strafing", !isStrafing);

        /////Weapons
        AnimTree.Set("parameters/conditions/is_one_handed", isOneHanded);
        AnimTree.Set("parameters/conditions/is_two_handed", !isOneHanded);
    }

    public override void _Process(double delta)
    {
        if(currentWeapon != selectedWeapon)
		{
			SwitchWeapon();
			GD.Print("Switched: " + currentWeapon);
		}
    }

	void IsRunning(double delta)
	{
		if (Input.IsActionJustPressed("Run") && isMoving)
		{
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
		currentRunBlend = (float)AnimTree.Get("parameters/One Handed Weapon/Locomotion/Move Forward/Run Blend/blend_amount");
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

	void Strafe(double delta)
	{

        if (isStrafing)
		{
            if (Input.GetAxis("ui_left", "ui_right") < 0)
            {
                targetStrafeBlend = 0f;
            }
			else
			{
                targetStrafeBlend = 1f;
            }
            currentStrafeBlend = (float)AnimTree.Get("parameters/One Handed Weapon/Locomotion/Strafe/Strafe Dir Blend/blend_amount");
            if (newStrafeBlend != targetStrafeBlend)
            {
                newStrafeBlend = Mathf.Lerp(currentStrafeBlend, targetStrafeBlend, blendSpeed * (float)delta);
                foreach (var blend in strafeBlends)
                {
                    AnimTree.Set(blend, (Variant)newStrafeBlend);
                }
                if (Mathf.Abs(newStrafeBlend - targetStrafeBlend) < .001f)
                {
                    newStrafeBlend = targetStrafeBlend;
                }
            }
        }
    }

	void RaiseWeapon(double delta)
	{
        if (Input.IsActionJustPressed("Shoot"))
		{
            if (!isRaising && !isWeaponRaised)
            {
                RaiseWait(delta);
            }
        }
		if (Input.IsActionJustReleased("Shoot"))
		{
			if (!isWeaponRaised)
			{
				GD.Print("Weapon Raised");
                isWeaponRaised = true;
            }
        }
    }

	async void RaiseWait(double delta)
	{
        isRaising = true;
        if (!isWeaponRaised)
        {
            targetRaisedBlend = 1f;
        }
        else
        {
            targetRaisedBlend = 0f;
        }
        currentRaisedBlend = (float)AnimTree.Get("parameters/One Handed Weapon/Locomotion/Idle/Raised Blend/blend_amount");
        await BlendLoop(delta, newRaisedBlend, currentRaisedBlend, targetRaisedBlend, raisedBlends);
		isRaising = false;
		GD.Print("Done Raising");
    }

	void IsShooting(double delta)
	{
		if (Input.IsActionPressed("Shoot") || (Input.IsActionJustPressed("Shoot")))
		{
			//GD.Print("Shoot Pressed");
			if (isWeaponRaised && !isRaising)
			{
				if (!isShooting)
				{
					isShooting = true;
					if(isOneHanded)
					{
                        AnimTree.Set("parameters/One Handed Weapon/One Handed Shoot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
                        AnimTree.Set("parameters/One Handed Weapon/One Handed Shoot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);	
                    }
					else
					{
                        AnimTree.Set("parameters/Two Handed Weapon/Two Handed Shoot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
                        AnimTree.Set("parameters/Two Handed Weapon/Two Handed Shoot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
                    }
					bulletDirection = -this.GlobalTransform.Basis.Z.Normalized();
                    SpawnBullet();
                    GD.Print("Shoot Started");
                }
			}
		}
	}
    void ShotOver(StringName animName)
    {//Add each non-interruptible shoot animation name to if statement
        if (animName == "Glock Shoot" || animName == "Shotgun Shoot") 
        {
            AnimTree.Set("parameters/One Handed Weapon/One Handed Shoot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            AnimTree.Set("parameters/Two Handed Weapon/Two Handed Shoot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);

            isShooting = false;
            GD.Print("Shoot Over");
        }
    }

	public void ResetIsShooting() //put this as Call Method track on any fast interruptable shoot animations (uzi, ak47 etc.)
	{
		isShooting = false;
        GD.Print("Shoot Over");
    }

	void SpawnBullet()
	{
        var bulletPrefab = currentBullet.Instantiate<Node3D>();
        GetTree().Root.AddChild(bulletPrefab);
        bulletPrefab.GlobalPosition = BulletSpawns[currentWeapon].GlobalPosition;
		bulletPrefab.GlobalRotation = this.GlobalRotation;

		//Muzzle Flashes
		var flash = muzzleflash.Instantiate<Node3D>();
        AddChild(flash);
        flash.GlobalPosition = BulletSpawns[currentWeapon].GlobalPosition;
        flash.GlobalRotation = this.GlobalRotation;
    }

	public void SwitchWeapon()
	{
		currentWeapon = selectedWeapon;
		SaveManager.SaveValue(GameManager.slotNumber, "Weapons", "Current Weapon", currentWeapon);
        isOneHanded = WeaponResources[currentWeapon].IsOneHanded;
        currentBullet = WeaponResources[currentWeapon].WeaponBullet;
        Weapons[currentWeapon].Visible = true;
		for (int i = 0; i < Weapons.Length; i++)
		{
			if(i != currentWeapon)
			{
				Weapons[i].Visible = false;
			}
		}

	}

    private async Task BlendLoop(double delta, float newBlend, float currentBlend, float targetBlend, string[] blends)
	{
        while (newBlend != targetBlend)
        {
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
