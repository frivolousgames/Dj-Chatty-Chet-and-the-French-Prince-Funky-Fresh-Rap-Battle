using Godot;
using System;

public partial class NavigationMarkers : Marker3D
{
	[Export]
	public Marker3D[] neighborMarkers {  get; set; }
	Marker3D lastMarker;


	// Called every frame. 'delta' is the elapsed time since the previous frame.


	//void AddNeighbors()
	//{
	//	for(int i = 0; i < GameManager.navMarkersStatic.Length; i++)
	//	{
			
	//	}
	//}
}
