using Godot;
using System;

public partial class DoorOpenCol : Area3D
{
	[Export]
	public Node3D StandPosNode; //Add each of these nodes to the vehicle
    [Export]
    public Node3D StandRotNode;
    [Export]
    public Node3D SitPosNode;

    [Export]
    CarDoor CarDoor;
    [Export]
    Camera3D CarCam;
    CarMover carMover;
	public override void _Ready()
	{
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        carMover = StandPosNode.GetParent<VehicleBody3D>() as CarMover;
	}

    void OnBodyEntered(Node3D body)
    {
        if (body is OverheadPlayer player)
        {
            player.standPosNode = StandPosNode;
            player.standRotNode = StandRotNode;
            player.sitPosNode = SitPosNode;
            player.isCarDoor = true;
            player.carDoor = CarDoor;
            player.carCam = CarCam;
            player.carMover = carMover;
        }
    }
    void OnBodyExited(Node3D body)
    {
        if (body is OverheadPlayer player)
        {
            player.isCarDoor = false;
        }
    }
}
