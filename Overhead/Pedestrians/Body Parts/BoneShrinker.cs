using Godot;
using System;

[GlobalClass]
public partial class BoneShrinker : SkeletonModifier3D
{
    [Export]
    public Pedestrian pedestrian;
    private Skeleton3D skeleton;
    public override void _Ready()
    {
        skeleton = GetSkeleton();
    }
    public override void _ProcessModificationWithDelta(double delta)
    {
        if(pedestrian.rejectedBoneIndexes == null)
        {
            return;
        }  
        if(pedestrian.rejectedBoneIndexes.Count > 0)
        {
            for (int i = 0; i < pedestrian.rejectedBoneIndexes.Count; i++)
            {
                skeleton.SetBonePoseScale(pedestrian.rejectedBoneIndexes[i], new Vector3(.001f, .001f, .001f));
                //GD.Print("Scale: " + skeleton.GetBoneName(pedestrian.rejectedBoneIndexes[i]));
            }
        }
    }
}
