using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class Pedestrian : CharacterBody3D
{
    List<Vector3> pathWaypoints;
    RayCast3D[] FrontCasts;
    [Export]
    RayCast3D LeftCast { get; set; }
    [Export]
    RayCast3D RightCast { get; set; }
    Vector3 openDirection = Vector3.Zero;
    Vector3 avoidancePosition;
    float rayCheckOffset;
    int facingDirection;
    bool hitObstacle;
    bool evadingObstacle;
    bool rejoiningPath;
    RayCast3D obstacleEvadeRay;
    Vector3 evadeDirection;

    CharacterBody3D player;

    float speed;
    float walkSpeed;
    float runSpeed;
    float injuredRunSpeed;
    float currentRunBlend;
    float targetRunBlend;
    float newRunBlend;

    string runBlend = "parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount";
    string injuredRunBlend = "parameters/Alive/Locomotion/Move Forward/Run/Injured Run/Injured Run Blend/blend_amount";
    string injuredIdleBlend = "parameters/Alive/Locomotion/Idle/Injured Idle Blend/blend_amount";
    string hitDirBlend = "parameters/Alive/Hit/Hit Dir Blend/blend_amount";
    float blendSpeed = 10f;

    public int idle = GD.RandRange(1, 3);


    [Export]
    float RotSpeed {  get; set; }

	[Export]
	public PedestrianType type {  get; set; }

	bool isMoving;
	bool isRunning;
	bool isIdle;
	bool isCowering;
	bool isPanicked;
    bool isPanickSelected;
	bool isHit;
	bool hitBack;
    bool hitLeft;
    bool isInjured;
	bool isDead;

	

	[Export]
	public Node3D FrontTrans;
    [Export]
    public Node3D BackTrans;
    [Export]
    public Node3D LeftTrans;
    [Export]
    public Node3D RightTrans;

    //Animation
    [Export]
    public AnimationPlayer AnimPlayer { get; set; }
    [Export]
    public AnimationTree AnimTree { get; set; }

    ////NAVIGATION//
    //private NavigationAgent3D NavAgent;
    private Marker3D currentMarker;
    private Marker3D nextMarker;
    private Marker3D previousMarker;
    private NavigationMarkers currentMarkerScript;
    private Vector3 nextPosition;
    private Vector3 destination;
    private float destinationOffset = .4f;
    private bool destinationReached;

    //Ragdoll//
    private PhysicalBoneSimulator3D ragdoll;
    private Skeleton3D skeleton;
    private PhysicalBone3D spineBone;
    public bool isLimb;

    //Hit
    private CollisionShape3D bodyCol;
    private Area3D hitArea;
    public Vector3 bulletVelocity;
    public Vector3 bulletPosition;
    public Vector3 bulletNormal;
    private float bulletVelocityMulti;
    private bool isShot;
    [Export]
    public PackedScene BloodSpatterLarge;
    [Export]
    public Node3D[] Limbs;
    [Export]
    public string[] boneNames;
    private int[] boneIndexes;
    [Export]
    public PhysicalBone3D[] physBones;
    public List<int> rejectedBoneIndexes;
    [Export]
    public Node3D[] LimbBloods;
    [Export]
    public Node3D IntestineNode;
    PhysicalBoneSimulator3D intestine;
    PhysicalBone3D intestineRoot;
    [Export]
    public Generic6DofJoint3D intestineJoint;
    string spinePath;
    string intestinePath;
    //List<CollisionShape3D> intestineCols;

    //Blood Decals
    [Export]
    public Godot.Collections.Array<Node3D> BloodDecals;
    [Export]
    public PackedScene GroundBlood;
    public bool groundBloodSpawned;
    int groundBloodDelay;
    [Export]
    public PackedScene[] BloodStreak;
    [Export]
    public float BloodStreakThreshold;
    [Export]
    public float BloodStreakDist;
    float streakOffset = .02f;
    float dynamicSortingOrder;
    float maxSortingLimit = 200f;

    ///Person Type
    bool hasStroller;
    bool isMale;
    int maxHealth;
    int health;
    int damage;
    [Export]
    public MeshInstance3D Body;
    [Export]
    public MeshInstance3D Pants;
    [Export]
    public MeshInstance3D Shorts;
    [Export]
    public MeshInstance3D Hoodie;
    [Export]
    public MeshInstance3D Tshirt;
    [Export]
    public MeshInstance3D Tanktop;
    [Export]
    public MeshInstance3D Socks;
    [Export]
    public MeshInstance3D Shoes;
    [Export]
    public Godot.Collections.Array<Node3D> WomanHair;
    [Export]
    public Godot.Collections.Array<Node3D> ManHair;
    [Export]
    public Godot.Collections.Array<Node3D> SeveredWomanHair;
    [Export]
    public Godot.Collections.Array<Node3D> SeveredManHair;

    [Export]
    public MeshInstance3D Stroller { get; set; }
    [Export]
    public CollisionShape3D StrollerCol { get; set; }

    public override void _Ready()
    {
        player = GetNode<CharacterBody3D>("/root/World 1/Overhead player");
        pathWaypoints = new List<Vector3>();
        FrontCasts = new RayCast3D[]
        {
            GetNode<RayCast3D>("Front Cast"),
            GetNode<RayCast3D>("Front Cast2")
        };
        rayCheckOffset = GD.Randi() % 5;
		
        hasStroller = type.HasStroller;
        if(hasStroller)
        {
            Stroller.Visible = true;
            StrollerCol.Disabled = false;
        }

        //walkSpeed = type.WalkSpeed;
        //runSpeed = type.RunSpeed;
        //injuredRunSpeed = type.InjuredRunSpeed;
        maxHealth = 4;
        SetType();
        health = maxHealth;

        speed = walkSpeed;
        CallDeferred(nameof(_Start));

        bodyCol = GetNode<CollisionShape3D>("Body Col");

        ragdoll = GetNode<PhysicalBoneSimulator3D>("Pedestrian/Player/Skeleton3D/Ragdoll");
        skeleton = GetNode<Skeleton3D>("Pedestrian/Player/Skeleton3D");
        spineBone = GetNode<PhysicalBone3D>("Pedestrian/Player/Skeleton3D/Ragdoll/Physical Bone spine_001");
        hitArea = GetNode<Area3D>("HitCol");

        dynamicSortingOrder = (float)GD.RandRange(0, maxSortingLimit - 1);
        _ = SpawnBloodStreaks();

        boneIndexes = new int[boneNames.Length];
        for(int i = 0; i < boneNames.Length; i++)
        {
            boneIndexes[i] = skeleton.FindBone(boneNames[i]);
            //GD.Print("Bone: " + skeleton.FindBone(boneNames[i]));
        }
        rejectedBoneIndexes = new List<int>();

        intestine = IntestineNode.GetNode<PhysicalBoneSimulator3D>("Guts Full/Guts Rig/Skeleton3D/PhysicalBoneSimulator3D");
        intestineRoot = intestine.GetNode<PhysicalBone3D>("Physical Bone Bone");
        spinePath = spineBone.GetPath();
        intestinePath = intestine.GetPath();
        //intestineCols = new List<CollisionShape3D>();
        //foreach()
    }
    private void _Start()
    {
        GetCurrentMarker();
    }

    public override void _PhysicsProcess(double delta)
	{
		if (!isDead)
		{
            //Raycasts
            CheckFrontRaycast();

            if (isRunning)
            {
                speed = runSpeed;
            }
            else if (isIdle || isCowering)
            {
                speed = 0;
            }
            else if(isInjured && !isRunning)
            {
                speed = injuredRunSpeed;
            }
            else
            {
                speed = walkSpeed;
            }

            //Set Direction
            Vector3 direction = GlobalPosition.DirectionTo(nextPosition);
            direction.Y = 0;
            Vector3 velocity = direction * speed;
            Vector3 lookDir = new Vector3(nextPosition.X, GlobalPosition.Y, nextPosition.Z);
            if(velocity.Length() > .1f)
            {
                Transform3D targetTrans = Transform.LookingAt(lookDir, Vector3.Up);
                Transform = Transform.InterpolateWith(targetTrans, RotSpeed * (float)delta);
            }

            // Add the gravity.
            if (!IsOnFloor())
            {
                velocity += GetGravity() * (float)delta;
            }

            Velocity = velocity;
            MoveAndSlide();

            isIdle = velocity.Length() < .01f && !isCowering? true: false;
            isMoving = velocity.Length() > .01f? true: false;
            hasStroller = type.HasStroller;

            //Animation Refs
            AnimTree.Set("parameters/Alive/Locomotion/conditions/is_idle", isIdle);
            AnimTree.Set("parameters/Alive/Locomotion/conditions/is_moving_forward", isMoving);
            AnimTree.Set("parameters/Alive/Locomotion/conditions/is_cowering", isCowering);
            AnimTree.Set("parameters/Alive/Locomotion/Idle/Idle/conditions/has_stroller", hasStroller);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Walk/conditions/has_stroller", hasStroller);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Walk/conditions/is_healthy", !isInjured);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Walk/conditions/is_injured", isInjured);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Walk/conditions/no_stroller", !hasStroller);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run/conditions/has_stroller", hasStroller);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run/conditions/is_healthy", !isInjured);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run/conditions/is_injured", isInjured);
            AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run/conditions/no_stroller", !hasStroller);
            AnimTree.Set("parameters/Alive/Hit/Hit Front/conditions/hit_left", hitLeft);
            AnimTree.Set("parameters/Alive/Hit/Hit Front/conditions/hit_right", !hitLeft);
            AnimTree.Set("parameters/Alive/Hit/Hit Back/conditions/hit_left", hitLeft);
            AnimTree.Set("parameters/Alive/Hit/Hit Back/conditions/hit_right", !hitLeft);

            //Blends
            //"parameters/Alive/Locomotion/Idle/Idle Blend/blend_amount"
            //"parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount"
            //"parameters/Alive/Locomotion/Move Forward/Run/Injured Run/Injured Run Blend/blend_amount"
            //"parameters/Alive/Hit/Hit Dir Blend/blend_amount"

            EvadeObstacle();
            ReachDestination();
            RejoinedPath();
            Panic();
            
        }
        else
		{
            ActivateRagdoll();
            
        }
        //if(IsVisibleInTree())
        //{
        //    GD.Print("Velocity: " + spineBone.LinearVelocity.Length());
        //}
    }
    

	void Panic()
	{
        if (isPanicked)
        {
            if (!isPanickSelected)
            {
                int i = GD.RandRange(0, 3);
                if(i == 0)
                {
                    isCowering = true;
                    isInjured = false;
                    isRunning = false;
                    AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount", 0);
                    destination = GlobalPosition;
                }
                else
                {
                    Vector3 dir = (GlobalPosition - player.GlobalPosition).Normalized();
                    destination = dir * 1000;
                    nextPosition = destination;
                }
                isPanickSelected = true;
            }
            
        }
    }
    public void CarHit()
    {
        if (!isDead)
        {
            health = 0;
            isDead = true;
            foreach(Decal d in BloodDecals)
            {
                d.Visible = true;
            }
            groundBloodDelay = 500;
            
        }
    }
    public void BulletHit()
    {
        if (damage >= health)
        {
            health = 0;
            isDead = true;
            isShot = true;
            bulletVelocityMulti = OverheadPlayer.weaponForce;
            bulletVelocity *= bulletVelocityMulti;
            groundBloodDelay = 500;
            AddBloodDecal();
            //GD.Print("Hit Bullet");
        }
        else
        {
            damage = OverheadPlayer.weaponDamage;
            health -= damage;
            //GD.Print("Damage: " + damage);
            //GD.Print("Health: " + health);
            isPanicked = true;
            if (!isCowering)
            {
                if (health == 2)
                {
                    isInjured = true;
                    isRunning = true;
                    AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount", 1);
                }
                else if(health == 1)
                {
                    isInjured = true;
                    isRunning = false;
                    AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount", 0);
                    speed = injuredRunSpeed;
                }
                else
                {
                    isRunning = true;
                    AnimTree.Set("parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount", 1);
                }
            }
            
            SetHitPos();
            if (hitBack)
            {
                AnimTree.Set(hitDirBlend, (Variant)1f);
            }
            else
            {
                AnimTree.Set(hitDirBlend, (Variant)0f);
            }
            AddBloodDecal();
            AnimTree.Set("parameters/Alive/Hit Oneshot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            AnimTree.Set("parameters/Alive/Hit Oneshot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }

    //void Hit(Node3D body)
    //{
    //    if (!isDead)
    //    {
    //        if (body is Bullet3dMover bullet)//change to seperate function without bullet statement
    //        {
                
    //            if (damage >= health)
    //            {
    //                health = 0;
    //                isDead = true;
    //                isShot = true;
    //                bulletVelocity = bullet.LinearVelocity * bulletVelocityMulti;
    //                GD.Print("Hit Bullet");
    //            }
    //            else
    //            {
    //                damage = OverheadPlayer.weaponDamage;
    //                health -= damage;
    //                isInjured = true;
    //                isRunning = true;
    //                SetHitSide();
    //                if (hitBack)
    //                {
    //                    AnimTree.Set(hitDirBlend, (Variant)1f);
    //                }
    //                else
    //                {
    //                    AnimTree.Set(hitDirBlend, (Variant)0f);
    //                }
    //                AnimTree.Set("parameters/Alive/Hit Oneshot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
    //                AnimTree.Set("parameters/Alive/Hit Oneshot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    //                nextPosition = GlobalPosition;
    //                HitRoutine();
    //            }
    //        }
    //    }
    //}
    public void SetHitPos()
    {
        if(bulletPosition.DistanceTo(FrontTrans.GlobalPosition) < bulletPosition.DistanceTo(BackTrans.GlobalPosition))
        {
            hitBack = false;
        }
        else
        {
            hitBack = true;
        }

        if (bulletPosition.DistanceTo(LeftTrans.GlobalPosition) < bulletPosition.DistanceTo(RightTrans.GlobalPosition))
        {
            hitLeft = true;
        }
        else
        {
            hitLeft = false;
        }

    }

    async void HitRoutine()
    {
        //while (isHit)
        //{
            await ToSignal(AnimTree, AnimationTree.SignalName.AnimationFinished);
            isHit = false;
            AnimationNodeStateMachinePlayback animNode = (AnimationNodeStateMachinePlayback)AnimTree.Get("parameters/playback");
            string currentAnim = animNode.GetCurrentNode();
            //GD.Print("Anim: " + currentAnim);
        //}
        nextPosition = destination; //TEMP will be retreat position
    }

    //Blood Decals
    void AddBloodDecal()
    {
        int i = GD.RandRange(0, BloodDecals.Count - 1);
        if (!BloodDecals[i].IsVisibleInTree())
        {
            BloodDecals[i].Visible = true;
        }
        else
        {
            AddBloodDecal();
            return;
        }
    }

    void Run(double delta)
    {
        if (isRunning)
        {
            speed = runSpeed;
            targetRunBlend = 1f;
            newRunBlend = 0f;
            currentRunBlend = (float)AnimTree.Get("parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount");
            if (newRunBlend != targetRunBlend)
            {
                newRunBlend = Mathf.Lerp(currentRunBlend, targetRunBlend, blendSpeed * (float)delta);
                if (Mathf.Abs(newRunBlend - targetRunBlend) < .001f)
                {
                    newRunBlend = targetRunBlend;
                }
                AnimTree.Set(runBlend, (Variant)newRunBlend);
            }
        }
        else
        {
            targetRunBlend = 0f;
            newRunBlend = 1f;
            currentRunBlend = (float)AnimTree.Get("parameters/Alive/Locomotion/Move Forward/Run Blend/blend_amount");
            if (newRunBlend != targetRunBlend)
            {
                newRunBlend = Mathf.Lerp(currentRunBlend, targetRunBlend, blendSpeed * (float)delta);
                if (Mathf.Abs(newRunBlend - targetRunBlend) < .001f)
                {
                    newRunBlend = targetRunBlend;
                    //GD.Print("RunBlend: " + runBlend);
                }
                AnimTree.Set(runBlend, (Variant)newRunBlend);
            }
        }
    }

    void CheckFrontRaycast()
    {
        if (!hitObstacle)
        {
            if(Engine.GetPhysicsFrames() % 5 == rayCheckOffset)
            {
                //GD.Print($"Rays is null: {FrontCasts == null}, length: {FrontCasts?.Length}");
                foreach (RayCast3D ray in FrontCasts)
                {
                    //GD.Print("Searching...");
                    if (ray.IsColliding())
                    {
                        hitObstacle = true;
                        CheckSideRaycasts();
                        //GD.Print("Obstacle Detected");
                        return;
                    }
                }
            } 
        }
    }
    void CheckSideRaycasts()
    {
        rejoiningPath = false;
        if(hitObstacle)
        {
            if(LeftCast.IsColliding() && RightCast.IsColliding())
            {
                avoidancePosition = Basis * Vector3.Back * 100f;
                if(facingDirection == -2)
                {
                    facingDirection = -facingDirection;
                }
                facingDirection -= 2;
            }
            else if (LeftCast.IsColliding() && !RightCast.IsColliding())
            {
                avoidancePosition = Basis * Vector3.Right * 100f;
                if(facingDirection == 2)
                {
                    facingDirection = -facingDirection;
                }
                facingDirection += 1;
            }
            else if(RightCast.IsColliding() && !LeftCast.IsColliding())
            {
                avoidancePosition = Basis * Vector3.Left * 100f;
                if (facingDirection == -2)
                {
                    facingDirection = -facingDirection;
                }
                facingDirection -= 1;
            }
            else
            {
                avoidancePosition = Basis * Vector3.Left * 100f;
                if (facingDirection == -2)
                {
                    facingDirection = -facingDirection;
                }
                facingDirection -= 1;
            }
        }
        if(facingDirection == -1 || facingDirection == Mathf.Abs(2))
        {
            obstacleEvadeRay = RightCast;
            evadeDirection = Vector3.Right; 
        }
        else
        {
            obstacleEvadeRay = LeftCast;
            evadeDirection = Vector3.Left;
        }
        nextPosition = avoidancePosition;
        EvadeDelay();
        //hitObstacle = false;
        //GD.Print("Next: " + nextPosition);
    }

    async void EvadeDelay()
    {
        await Task.Delay(500);
        evadingObstacle = true;
        hitObstacle = false;
    }

    async void EvadeObstacle()
    {
        if(evadingObstacle)
        {
            //await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (!obstacleEvadeRay.IsColliding())
            {
                evadingObstacle = false;
                if(facingDirection < 0)
                {
                    //GD.Print("PastObstacle");
                    facingDirection += 1;
                    evadeDirection = Vector3.Right;
                }
                else if (facingDirection == 0)
                {
                    evadingObstacle = false;
                    rejoiningPath = true;
                    RejoinPath();
                    return;
                }
                else
                {
                    facingDirection -= 1;
                    evadeDirection = Vector3.Left;
                    //GD.Print("PastObstacle");
                }
                nextPosition = Basis * evadeDirection * 100;
                await Task.Delay(400);
                evadingObstacle = true;
            }
            else
            {
                //GD.Print("Not Past Yet");
            }
        }
    }
    void RejoinPath()
    {
        if(rejoiningPath)
        {
            float dist = 1000f;
            Vector3 nextPoint = Vector3.Zero;
            foreach(Vector3 point in pathWaypoints)
            {
                if (GlobalPosition.DistanceTo(point) < dist //&&
                    /*FrontTrans.GlobalPosition.DistanceTo(point) < BackTrans.GlobalPosition.DistanceTo(point)*/)
                {
                    dist = GlobalPosition.DistanceTo(point);
                    nextPoint = point;
                    //GD.Print("point: " + point);
                }
            }
            nextPosition = nextPoint;
            //GD.Print("Rejoining");
        }
    }

    void RejoinedPath()
    {
        if (rejoiningPath)
        {
            if(GlobalPosition.DistanceTo(nextPosition) < .1f)
            {
                nextPosition = destination;
                rejoiningPath = false;
            }
        }
    }

    void SetPathWaypoints()
    {
        pathWaypoints.Clear();
        float dist = GlobalPosition.DistanceTo(destination);
        Vector3 dir = (GlobalPosition - destination).Normalized();
        for (float i = 0; i < dist; i += .1f)
        {

            Vector3 point = destination + (dir * i);
            pathWaypoints.Add(point);
        }
    }

    //Get closest marker to position and set that marker's script as current
    void GetCurrentMarker()
    {
        float dist = 1000f;
        foreach (Marker3D marker in NavMarkersArray.navMarkersStatic)
        {
            float length = GlobalTransform.Origin.DistanceTo(marker.GlobalTransform.Origin);
            if (length < dist)
            {
                dist = length;
                //GD.Print("Dist: " + marker.Name + " " + dist);
                currentMarker = marker;
            }
        }
        currentMarkerScript = currentMarker as NavigationMarkers;
        Vector3 offset = new Vector3((float)GD.RandRange(-0.6f, 0.6f), 0f, (float)GD.RandRange(-0.6f, 0.6f));
        destination = currentMarker.GlobalPosition + offset;
        nextPosition = destination;
        SetPathWaypoints();
        //GD.Print("GetCurrentMarker: " + currentMarker.Name);
    }

    ////Choose next marker randomly from current marker's script of neighbors.
    ////If selected marker is same as the last marker, select again
    void SetNextMarker()
    {
        int i = GD.RandRange(0, currentMarkerScript.neighborMarkers.Length - 1);
        nextMarker = currentMarkerScript.neighborMarkers[i];
        if (nextMarker == previousMarker)
        {
            SetNextMarker();
            return;
        }
        previousMarker = currentMarker;
        currentMarker = nextMarker;
        currentMarkerScript = currentMarker as NavigationMarkers;
        Vector3 offset = new Vector3((float)GD.RandRange(-0.6f, 0.6f), 0f, (float)GD.RandRange(-0.6f, 0.6f));
        destination = currentMarker.GlobalPosition + offset;
        nextPosition = destination;
        SetPathWaypoints();
        //GD.Print("SetNextMarker: " + currentMarker.Name);
    }

    //Reach destination and start delay method
    void ReachDestination()
    {
        if(GlobalTransform.Origin.DistanceTo(destination) < destinationOffset)
        {
            if (!destinationReached)
            {
                destinationReached = true;
                nextPosition = GlobalPosition;
                destination = GlobalPosition;
                DestinationDelay();
                //GD.Print("Destination Reached");
                return;
            }
        }
    }
    //Delay setting new destination by random amount
    async void DestinationDelay()
    {
        int i = GD.RandRange(0, 3000);
        await Task.Delay(i);
        destinationReached = false;
        isIdle = false;
        //GD.Print("Delay Over");
        SetNextMarker();
    }

    //Ragdoll
    void ActivateRagdoll()
    {
        if (isDead)
        {
            if (!ragdoll.IsSimulatingPhysics())
            {
                if (isLimb)
                {
                    int amount = GD.RandRange(1, Limbs.Length);
                    //GD.Print("IsLimb: " + amount);
                    List<int> nums = new List<int>(Limbs.Length);
                    var activeBoneNames = new Godot.Collections.Array<StringName>();
                    for (int i = 0; i < Limbs.Length; i++)
                    {
                        nums.Add(i);
                    }
                    List<string> bones = new List<string>();

                    for (int i = 0; i < amount; i++)
                    {
                        if (nums.Count > 0)
                        {
                            var j = GD.Randi() % nums.Count;
                            int k = nums[(int)j];
                            Limbs[k].Visible = true;
                            Limbs[k].Reparent(GetTree().Root.GetChild(0), true);
                            Limbs[k].GlobalPosition = spineBone.GlobalPosition;
                            bones.Add(physBones[k].Name);
                            LimbBloods[k].Visible = true;
                            skeleton.SetBonePoseScale(boneIndexes[k], new Vector3(.001f, .001f, .001f));
                            nums.Remove(nums[(int)j]);
                        }
                        else
                        {
                            //GD.Print("Break Loop");
                            break;
                        }
                    }
                    foreach (var child in ragdoll.GetChildren())
                    {
                        foreach (string b in bones)
                        {
                            if (child is PhysicalBone3D pb && pb.Name != b)
                            {
                                activeBoneNames.Add(skeleton.GetBoneName(pb.GetBoneId()));  
                            }
                            else
                            {
                                if(child is PhysicalBone3D p)
                                {
                                    rejectedBoneIndexes.Add(p.GetBoneId());
                                    //GD.Print("Rejected Bone: " + child.Name);
                                }
                            }
                        }
                    }
                    int legs = 0;
                    for(int i = 0; i < rejectedBoneIndexes.Count; i++)
                    {
                        //GD.Print("Bone Name: " + skeleton.GetBoneName(rejectedBoneIndexes[i]));
                        if (skeleton.GetBoneName(rejectedBoneIndexes[i]) == "thigh.L" ||
                            skeleton.GetBoneName(rejectedBoneIndexes[i]) == "thigh.R")
                        {
                            legs++;
                        }
                    }
                    if(legs == 2)
                    {
                        //intestineJoint.NodeA = spinePath;
                        //intestineJoint.NodeB = intestinePath;
                        intestine.Active = true;
                        intestine.PhysicalBonesStartSimulation();
                        IntestineNode.Visible = true;
                        
                        
                        //var cols = intestine.FindChildren("*", "CollisionShape3D", recursive: true, owned: false);
                        //foreach (CollisionShape3D child in cols)
                        //{
                        //    child.Disabled = false;
                        //}
                        GD.Print("intestine Spawned");
                    }
                    else
                    {
                        intestineJoint.NodeA = null;
                        intestineJoint.NodeB = null;
                    }
                    ragdoll.Active = true;
                    ragdoll.PhysicalBonesStartSimulation(activeBoneNames);
                }
                else
                {
                    intestineJoint.NodeA = null;
                    intestineJoint.NodeB = null;
                    ragdoll.Active = true;
                    ragdoll.PhysicalBonesStartSimulation();
                }
                AnimTree.Active = false;
                bodyCol.QueueFree();
                hitArea.QueueFree();
                if (isShot)
                {
                    Callable.From(ApplyBulletImpulse).CallDeferred();
                    GD.Print("Bullet Vel: " + bulletVelocity);
                    isShot = false;
                }
                
                SpawnGroundBlood();
            }

        }
    }
    async void ApplyBulletImpulse()
    {
        await Task.Delay(10);
        var chestBone = GetNode<PhysicalBone3D>("Pedestrian/Player/Skeleton3D/Ragdoll/Physical Bone spine_003");
        chestBone.ApplyImpulse(bulletVelocity, Vector3.Zero);
    }

    void OnSkeletonUpdated()
    {
        if (rejectedBoneIndexes.Count > 0)
            {
                for (int i = 0; i < rejectedBoneIndexes.Count; i++)
                {
                    skeleton.SetBonePoseScale(rejectedBoneIndexes[i], new Vector3(.001f, .001f, .001f));
                    //GD.Print("Scale: " + skeleton.GetBoneName(rejectedBoneIndexes[i]));
                }
            }
    }

    async void SpawnGroundBlood()
    {
        while (true)
        {
            if (!groundBloodSpawned && spineBone.LinearVelocity.Length() < 0.04f)
            {
                await Task.Delay(groundBloodDelay);
                if (!groundBloodSpawned && spineBone.LinearVelocity.Length() < 0.04f)
                {
                    groundBloodSpawned = true;
                    var groundBlood = GroundBlood.Instantiate<Decal>();
                    Node3D groundNode = null;
                    groundNode = GetGround(groundNode);
                    //GD.Print(groundNode.Name);
                    groundNode.AddChild(groundBlood);
                    groundBlood.GlobalPosition = new Vector3(spineBone.GlobalPosition.X, groundNode.GlobalPosition.Y, spineBone.GlobalPosition.Z);
                    //GD.Print("Ground Blood Spawned");
                }
            }
            //GD.Print("Moving..." + spineBone.LinearVelocity.Length());
            await Task.Delay(5);
        }
 
    }
    private async Task SpawnBloodStreaks()
    {
        Node3D groundNode = null;
        while (true)
        {
            groundNode = GetGround(groundNode);
            while (spineBone.LinearVelocity.Length() > BloodStreakThreshold && isDead &&
                spineBone.GlobalPosition.DistanceTo(new Vector3(spineBone.GlobalPosition.X, groundNode.GlobalPosition.Y, spineBone.GlobalPosition.Z)) < BloodStreakDist)
            {
                dynamicSortingOrder = (dynamicSortingOrder + 1) % maxSortingLimit;
                int i = GD.RandRange(0, BloodStreak.Length - 1);
                var streakPrefab = BloodStreak[i].Instantiate<Decal>();
                groundNode.AddChild(streakPrefab);
                streakPrefab.Size = new Vector3(streakPrefab.Size.X, streakPrefab.Size.Y + (dynamicSortingOrder * .001f), streakPrefab.Size.Z);
                Vector3 lookBasis = spineBone.LinearVelocity.Normalized();
                streakPrefab.Basis = Basis.LookingAt(lookBasis);
                streakPrefab.GlobalPosition = new Vector3(spineBone.GlobalPosition.X, groundNode.GlobalPosition.Y + streakOffset, spineBone.GlobalPosition.Z);
                streakPrefab.SortingOffset += dynamicSortingOrder * 001f;

                //GD.Print("Spawned Streak");
                await Task.Delay(10);
            }
            await Task.Delay(10);
            
        }

    }

    Node3D GetGround(Node3D groundNode)
    {
        var cols = GetTree().GetNodesInGroup("Roads");
        Node3D[] colNodes = new Node3D[cols.Count];
        for (int i = 0; i < cols.Count; i++)
        {
            colNodes[i] = (Node3D)cols[i];
        }
        float distance = 1000;
        foreach (var col in colNodes)
        {
            if (GlobalPosition.DistanceTo(col.GlobalPosition) < distance)
            {
                groundNode = col;
                //GD.Print("Ground Node: " + groundNode.Name);
                distance = GlobalPosition.DistanceTo(col.GlobalPosition);
            }
        }
        return groundNode;
    }

    ///Person Type
    
    void SetType()
    {
        int m = GD.RandRange(0, 1);
        isMale = m == 0 ? true : false;

        walkSpeed = (float)GD.RandRange(1.8, 2.5);
        runSpeed = (float)GD.RandRange(3.8, 5.3);
        injuredRunSpeed = walkSpeed - 0.5f;
        health = 4;

        //Clothing
        ///Add to these if adding more clothing or blendshapes
        int p = GD.RandRange(0, 1);
        if(p == 0)
        {
            Pants.Visible = true;
            Pants.SetBlendShapeValue(0, 1);
            if (!isMale)
            {
                Pants.SetBlendShapeValue(1, 1);
            }
            Body.SetBlendShapeValue(2, 1);
        }
        else
        {
            Shorts.Visible = true;
            Shorts.SetBlendShapeValue(0, 1);
            
            if (!isMale)
            {
                Shorts.SetBlendShapeValue(1, 1);
            }
            int sock = GD.RandRange(0, 2);
            if(sock == 0)
            {
                Socks.SetBlendShapeValue(0, 1);
                Socks.Visible = true;
            }
            else if(sock == 1)
            {
                Socks.Visible = true;
            }
            Body.SetBlendShapeValue(5, 1);
        }

        int s = GD.RandRange(0, 3);
        if(s == 0)
        {
            Tshirt.Visible = true;
            if (!isMale)
            {
                Tshirt.SetBlendShapeValue(0, 1);
            }
            Body.SetBlendShapeValue(1, 1);
        }
        else if(s == 1)
        {
            Hoodie.Visible = true;
            if (!isMale)
            {
                Hoodie.SetBlendShapeValue(0, 1);
            }
            Body.SetBlendShapeValue(3, 1);
        }
        else if( s == 2)
        {
            Tanktop.Visible = true;
            if (!isMale)
            {
                Tanktop.SetBlendShapeValue(0, 1);
            }
            Body.SetBlendShapeValue(4, 1);
        }
        else
        {
            if (!isMale)
            {
                Tshirt.Visible = true;
                Tshirt.SetBlendShapeValue(0, 1);
                Body.SetBlendShapeValue(1, 1);
            }
        }

        if(isMale)
        {
            int h = GD.RandRange(0, ManHair.Count - 1);
            ManHair[h].Visible = true;
            //SeveredManHair[h].Visible = true;
            //GD.Print("Man Hair");
        }
        else
        {
            int h = GD.RandRange(0, WomanHair.Count - 1);
            WomanHair[h].Visible = true;
            //SeveredWomanHair[h].Visible = true;
            //GD.Print("Woman Hair");
        }
            
    }

}
