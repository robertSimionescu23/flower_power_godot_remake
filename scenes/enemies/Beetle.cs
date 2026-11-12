using Godot;
using System;

public partial class Beetle : CharacterBody2D
{
    [Export]
    public float Speed = 50.0f;
    public int Direction = -1;

    public RayCast2D GroundCheck;
    public override void _Ready()
    {
        GroundCheck = GetNode<RayCast2D>("GroundCheck");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        velocity  += GetGravity() * (float)delta;
        velocity.Y = Mathf.Clamp(velocity.Y, -400f, 400f);

        if(GroundCheck.IsColliding())
            velocity.X = Direction * Speed;
        else
            velocity.X = 0;

        Velocity = velocity;
        MoveAndSlide();
    }
}
