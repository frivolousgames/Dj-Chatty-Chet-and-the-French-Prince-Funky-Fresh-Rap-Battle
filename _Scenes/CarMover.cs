using Godot;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public partial class CarMover : VehicleBody3D
{
	[Export]
	public float MaxSteeringAngle;
    [Export]
    public float MaxEngineForce;
    [Export]
    public float MaxBrakeForce;
    [Export]
    public OmniLight3D[] TailLights;

    bool isLit;
    bool isBraking;

    [Export]
    public Node3D MainNode;
    [Export]
    public float TireDecalThreshold;
    [Export]
    public PackedScene TireTrack;

    [Export]
    public Node3D HeadLights;

    [Export]
    public PackedScene DamageParticles;
    
    PhysicsDirectBodyState3D state;
    public override void _Ready()
    {
        _ = SpawnTireDecals();

        //Hitting buildings or other static objects creates damage particles
        ContactMonitor = true;
        MaxContactsReported = 5;
        BodyEntered += HitStaticObject;
    }
    public override void _PhysicsProcess(double delta)
    {
        float steering = Input.GetAxis("ui_right", "ui_left");
        Steering = steering * MaxSteeringAngle;

        float brake = Input.IsActionPressed("ui_accept") ? 1.0f : 0.0f;
        Brake = brake * MaxBrakeForce;
        

        if (brake < 1f)
        {
            float throttle = Input.GetAxis("ui_up", "ui_down");
            EngineForce = throttle * -MaxEngineForce;
            SetLightsOff();
            isBraking = false;
        }
        else
        {
            SetLightsOn();
            EngineForce = 0f;
            isBraking = true;
            
        }

        state = PhysicsServer3D.BodyGetDirectState(GetRid());
    }

    public override void _Process(double delta)
    {
        HeadlightController();
    }

    void SetLightsOn()
    {
        if (!isLit)
        {
            foreach (OmniLight3D light in TailLights)
            {
                light.Visible = true;
            }
            isLit = true;
        }
    }
    void SetLightsOff()
    {
        if (isLit)
        {
            foreach (OmniLight3D light in TailLights)
            {
                light.Visible = false;
            }
            isLit = false;
        }
    }
    private async Task SpawnTireDecals()
    {
        while (true)
        {
            while (LinearVelocity.Length() > TireDecalThreshold && isBraking)
            {
                var trackPrefab = TireTrack.Instantiate<Decal>();
                MainNode.AddChild(trackPrefab);
                Vector3 offset = ToGlobal(new Vector3(0f, -.15f, -.56f));
                trackPrefab.GlobalPosition = offset;
                //trackPrefab.Position = new Vector3(Transform.Basis.X.X, Transform.Origin.Y + offsetY, Transform.Origin.Z + offsetZ);
                trackPrefab.Rotation = new Vector3(0f, Rotation.Y, 0f);
                trackPrefab.Scale = Vector3.One;
                trackPrefab.Size = new Vector3(.819f, .075f, .218f);
                trackPrefab.Modulate = new Color(0f, 0f, 0f, 1f);
                await Task.Delay(10);
            }
            await Task.Delay(10);
        }
        
    }

    void HeadlightController()
    {
        if (Input.IsActionJustPressed("headlights"))
        {
            if (HeadLights.IsVisibleInTree())
            {
                HeadLights.Visible = false;
            }
            else
            {
                HeadLights.Visible = true;
            }
        }
    }

    private void HitStaticObject(Node body)
    {
        if (body.IsInGroup("Vehicle Static Hit"))
        {
            int hitPoints = state.GetContactCount();
            for (int i = 0; i < hitPoints; i++)
            {
                GodotObject obj = state.GetContactColliderObject(i);
                if(obj == body)
                {
                    var damageParts = DamageParticles.Instantiate<Node3D>();
                    MainNode.AddChild(damageParts);
                    damageParts.Position = state.GetContactLocalPosition(i);
                    GpuParticles3D parts = damageParts.GetNode<GpuParticles3D>("GPUParticles3D");
                    parts.Emitting = true;
                    GD.Print("Hit Static: " + damageParts.Position);
                }
            }

        }
    }
}
