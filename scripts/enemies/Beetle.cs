using Godot;
using System;

public partial class Beetle : Enemy
{

    public RayCast2D GroundCheck;
    public RayCast2D WallCheck;
    public Timer KnockBackTimer;
    [Export]
    private float _knockBackStrength = 400f;

    public override void _Ready()
    {
        base._Ready();
        GroundCheck     = GetNode<RayCast2D>("GroundCheck");
        WallCheck       = GetNode<RayCast2D>("WallCheck");
        KnockBackTimer  = GetNode<Timer>("KnockBackTimer");
    }

    public void ChangeDirection()
    {
        Direction = -Direction;

        Vector2 scale = Scale;
        scale.X = -scale.X;
        Scale = scale;
    }

    public Vector2 Move(){
        Vector2 velocity = Velocity;

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
        if(IsAlive)
            Velocity = Move();

        else {
            if(IsOnFloor() && KnockBackTimer.IsStopped())
                Velocity = new Vector2(0,0);
        }

        Velocity  += GetGravity() * (float)delta;
    }

    public override void GetHit()
    {
        if(IsAlive){
            IsAlive = false;
            //TODO: Impelemnt animation
            var distanceToPlayer = Player.GlobalPosition - GlobalPosition;
            var directionToPlayer = distanceToPlayer.Normalized();

            KnockBackTimer.Start();

            Vector2 knockBackDirection = new Vector2(0.5f, -0.5f);

            if (directionToPlayer.X > 0)
                knockBackDirection.X = -0.5f;

            Velocity  =  knockBackDirection * _knockBackStrength;
        }
    }
}
