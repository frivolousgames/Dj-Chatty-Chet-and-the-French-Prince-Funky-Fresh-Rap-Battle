using Godot;
using System;

public partial class NavigationRegion : NavigationRegion3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        NavigationServer3D.MapSetCellHeight(GetNavigationMap(), NavigationMesh.CellSize);

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
