using Godot;
using System;

public partial class SetMeshMaterial : MeshInstance3D
{
	[Export]
	public Godot.Collections.Array<Color> Colors;

	public override void _Ready()
    {
        Callable.From(SetMats).CallDeferred();   
    }

    void SetMats()
    {
        if (IsVisibleInTree())
        {
            this.MaterialOverride = null;

            int c = GD.RandRange(0, Colors.Count - 1);
            Color chosenColor = Colors[c];
            string hexColor = chosenColor.ToHtml(false).ToLower();

            if (GetSurfaceOverrideMaterial(0) is StandardMaterial3D stan)
            {
                StandardMaterial3D s = (StandardMaterial3D)stan.Duplicate();
                s.AlbedoColor = chosenColor;
                SetSurfaceOverrideMaterial(0, s);
            }
        }
    }
}
