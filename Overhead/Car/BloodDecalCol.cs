using Godot;
using System;
using System.Threading.Tasks;

public partial class BloodDecalCol : Area3D
{
	float bloodTime;
	float bloodMaxTime;
	[Export]
	public float SpawnDistance;
	Vector3 lastSpawnPos; 

    [Export]
    public PackedScene TireTrack { get; set; }

	
	float maxSortingLimit = 200f;
	float dynamicSortingOrder;
    public override void _Ready()
	{
		lastSpawnPos = GlobalPosition;
        dynamicSortingOrder = (float)GD.RandRange(0, maxSortingLimit - 1);
        bloodMaxTime = 1.7f;
		AreaEntered += BloodHit;
		_ = BloodTracks();
	}

	private void BloodHit(Area3D body)
	{
		if(body.IsInGroup("Blood Decals"))
		{
			//GD.Print("Hit Blood");
			bloodTime = bloodMaxTime;
		}
	}

	private async Task BloodTracks()
	{
		while (true)
		{
			while(bloodTime > 0f)
			{
				if(GlobalPosition.DistanceTo(lastSpawnPos) >= SpawnDistance)
				{
					lastSpawnPos = GlobalPosition;
                    dynamicSortingOrder = (dynamicSortingOrder + 1) % maxSortingLimit;
                    var trackPrefab = TireTrack.Instantiate<Decal>();
                    GetTree().Root.GetChild(0).AddChild(trackPrefab);
                    trackPrefab.Size = new Vector3(trackPrefab.Size.X, trackPrefab.Size.Y + (dynamicSortingOrder * .001f), trackPrefab.Size.Z);
                    trackPrefab.GlobalPosition = new Vector3(GlobalPosition.X, 0.01f, GlobalPosition.Z);
                    trackPrefab.GlobalRotation = new Vector3(0f, GlobalRotation.Y, 0f);
                    trackPrefab.Scale = Vector3.One;
                    trackPrefab.SortingOffset += dynamicSortingOrder * 001f;
                    bloodTime -= .01f;
                    //GD.Print("Spawned: " + trackPrefab.SortingOffset);
                }
                await Task.Delay(1);
            }
			await Task.Delay(5);
		}
	}
}
