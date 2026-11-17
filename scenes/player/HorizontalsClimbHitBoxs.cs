using Godot;
using System;

public partial class HorizontalsClimbHitBoxs : Area2D
{
    private Player _Player;
    public override void _Ready()
    {
        _Player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }
    //Mirro box based on player direction
    public override void _Process(double delta)
    {
        if(_Player.Velocity.X < 0)
            Scale = new Vector2(-1, 1);
        else if (_Player.Velocity.X > 0)
            Scale = new Vector2(1, 1);

    }
}
