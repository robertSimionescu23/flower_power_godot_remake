using Godot;
using System;

public partial class Beetle : Enemy
{


    public RayCast2D GroundCheck;
    public RayCast2D WallCheck;
    public override void _Ready()
    {
        base._Ready();
        GroundCheck = GetNode<RayCast2D>("GroundCheck");
        WallCheck = GetNode<RayCast2D>("WallCheck");
    }

    public void ChangeDirection()
    {
        Direction = -Direction;

        Vector2 scale = Scale;
        scale.X = -scale.X;
        Scale = scale;
    }

    public Vector2 Move(float delta){
        Vector2 velocity = Velocity;

        velocity  += GetGravity() * (float)delta;
        velocity.Y = Mathf.Clamp(velocity.Y, -400f, 400f);

        var canGo = GroundCheck.IsColliding() && IsOnFloor() && !WallCheck.IsColliding();

        if(canGo)
            velocity.X = Speed * Direction;
        else
            ChangeDirection();

        return velocity;

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Velocity = Move((float)delta);
    }

}
