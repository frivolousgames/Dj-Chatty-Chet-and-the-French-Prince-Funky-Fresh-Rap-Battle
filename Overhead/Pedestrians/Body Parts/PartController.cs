using Godot;
using System;

public partial class PartController : Node3D
{
    [Export]
    public CollisionShape3D[] Cols;
    bool colsActive;
    public override void _PhysicsProcess(double delta)
    {
        if (IsVisibleInTree())
        {
            if (!colsActive)
            {
                foreach (var col in Cols)
                {
                    col.Disabled = false;
                }
                colsActive = true;
            }
        }
    }
}
