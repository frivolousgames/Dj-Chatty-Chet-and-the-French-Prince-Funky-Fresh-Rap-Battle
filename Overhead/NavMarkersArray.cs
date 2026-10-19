using Godot;
using System;
using System.Collections.Generic;

public partial class NavMarkersArray : Node3D
{
    public static List<Marker3D> navMarkersStatic;
    public override void _Ready()
    {
        navMarkersStatic = new List<Marker3D>();
        foreach (var marker in GetTree().GetNodesInGroup("Destination Markers"))
        {
            if (marker is Marker3D marker3d)
            {
                navMarkersStatic.Add(marker3d);
            }
        }

        //CallDeferred(nameof(_Start));
    }

    //void _Start()
    //{
    //    //Navigation
    //    for (int i = 0; i < navMarkersStatic.Count; i++)
    //    {
    //        GD.Print(navMarkersStatic[i].Name);
    //    }
    //}
}
