using Godot;
using System;
using System.Threading.Tasks;

public partial class GroundBloodDecal : Decal
{
	float startX = 0.1f;
	float startZ = 0.1f;
	float endX = 1.2f;
	float endZ = 1.2f;
	Vector3 startSize;
	Vector3 endSize;

    [Export]
    public int FadeDelay;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		endSize = new Vector3(endX, Size.Y, endZ);
		startSize = new Vector3(startX, Size.Y, startZ);
		Size = startSize;
		Grow();
        _ = FadeOut();
    }

    async void Grow()
	{
        if (GodotObject.IsInstanceValid(this))
        {
            float i = 0;
            while (Mathf.Abs(Size.X - endSize.X) > .005f)
            {
                if (GodotObject.IsInstanceValid(this))
                {
                    Vector3 newSize = Size.Lerp(endSize, i);
                    Size = newSize;
                    //GD.Print("Growing");
                    i += .0001f;
                    await Task.Delay(5);
                }
                else
                {
                    return;
                }
            }
            return;
        }     
	}

    async Task FadeOut()
    {
        await Task.Delay(FadeDelay);
        while (AlbedoMix > 0f)
        {
            AlbedoMix -= .01f;
            await Task.Delay(10);
        }
        Free();
    }
}
