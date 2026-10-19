using Godot;
using System;
using System.Threading.Tasks;

public partial class FadeOverTime : Decal
{
	[Export]
	public int FadeDelay;
	public override void _Ready()
	{
		
		_ = FadeOut();
	}

	async Task FadeOut()
	{
        await Task.Delay(FadeDelay);
        while (AlbedoMix > 0f)
		{
			AlbedoMix -= .01f;
			await Task.Delay(10);
		}
		QueueFree();
	}
}
