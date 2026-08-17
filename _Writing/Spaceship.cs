using Godot;
using System;
using System.Diagnostics;

public partial class Spaceship : Sprite2D
{
	//Move
	float minX;
	float maxX;
	float minY;
	float maxY;
	[Export]
	public float speed { get; set; }

    //Shoot
    [Export]
	public Node2D bulletSpawnL { get; set; }
    [Export]
    public Node2D bulletSpawnR { get; set; }
    Node2D[] spawns;
	bool isShooting;
	bool isShootDelay;
	[Export]
	float shootWait { get; set; }
    [Export]
	public PackedScene bullet { get; set; }
    [Export]
	public CanvasLayer mainNode { get; set; }


    public override void _Ready()
	{
		maxX = 1873f;
		minX = 889f;
		minY = 540f;
		maxY = 1030f;
		isShooting = false;
		isShootDelay = false;
		spawns = new Node2D[] { bulletSpawnL,  bulletSpawnR };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MoveShip(delta);
		Shoot(delta);
	}

	void MoveShip(double delta)
	{
        float dirH = Input.GetAxis("move_left", "move_right") * speed * (float)delta;
		float dirV = Input.GetAxis("move_up", "move_down") * speed * (float)delta;
        if ( dirH < 0)
		{
			if(Position.X > minX)
			{
                Position = new Vector2(Position.X + dirH, Position.Y);
            }
		}
        if (dirH > 0)
		{
			
			if (Position.X < maxX)
			{
                Position = new Vector2(Position.X + dirH, Position.Y);
            }
		}
        if (dirV < 0)
        {
            if (Position.Y > minY)
            {
                Position = new Vector2(Position.X, Position.Y + dirV);
            }
        }
        if (dirV > 0)
        {

            if (Position.Y < maxY)
            {
                Position = new Vector2(Position.X, Position.Y + dirV);
            }
        }
    }
	void Shoot(double delta)
	{
		if (!isShooting)
		{
			if(Input.IsMouseButtonPressed(MouseButton.Left))
			{
				isShooting = true;
				isShootDelay = true;
				for(int i =  0; i < 2; i++)
				{
                    var bulletPrefab = bullet.Instantiate<RigidBody2D>();
                    mainNode.AddChild(bulletPrefab);
                    bulletPrefab.Position = spawns[i].GlobalPosition;
					GD.Print(bulletPrefab.GetParent());
                }
				Timer timer = new Timer();
				this.AddChild(timer);
				timer.WaitTime = shootWait;
				timer.OneShot = true;
				timer.Start();
                timer.Timeout += () =>
                {
                    isShooting = false;
                    timer.QueueFree();
                };

            }
			
		}
	}
	
}
