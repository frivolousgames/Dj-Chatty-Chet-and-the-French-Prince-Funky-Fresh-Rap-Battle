using Godot;
using System;

public partial class Parallax_Stars : Sprite2D
{
    float verticalBounds;
    Vector2 startPos;
    float endPosY;
    //Vector2 endPos;
    Vector2 size;
    Vector2 scaledSize;
    [Export]
    public float scrollSpeed;
    Vector2 scrollDir;

    public override void _Ready()
    {
        startPos = new Vector2(Position.X, -561f);
        size = Texture.GetSize();
        scaledSize = size * Scale;
        endPosY = (startPos.Y + scaledSize.Y) * 3;
        //endPos = new Vector2(startPos.X, endPosY);
        scrollDir = new Vector2(0, 1);
    }

    public override void _Process(double delta)
    {
        Parallax(delta);
    }

    void Parallax(double delta)
    {
        Position += scrollDir * scrollSpeed * (float)delta;
        if(Position.Y > endPosY)
        {
            Position = startPos;
        }
    }

}
