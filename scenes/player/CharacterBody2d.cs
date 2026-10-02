using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    [Export]
    public  float WallSlideSpeed = 200f;
    AnimatedSprite2D AnimatedSprite;

    Vector2 localGravity ;

    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        localGravity = GetGravity();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor() && !IsOnWallOnly())
        {
            velocity +=  GetGravity() * (float)delta;
        }

        if(Input.IsActionJustReleased("jump") && velocity.Y < 0)
            velocity.Y /= 5;

        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        int direction;

        if (Input.IsActionPressed("move_right")){
            direction = 1;
            if(IsOnFloor()){
                AnimatedSprite.Animation = "running";
            }
            AnimatedSprite.FlipH = false;
        }
        else if (Input.IsActionPressed("move_left")){
            direction = -1;
            if(IsOnFloor()){
                AnimatedSprite.Animation = "running";
            }
            AnimatedSprite.FlipH = true;
        }
        else{
            direction = 0;
            if(IsOnFloor())
                AnimatedSprite.Animation = "idle";
        }

        if (direction != 0)
        {
            velocity.X = direction * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
        }

        if (!IsOnFloor())
        {
            if(velocity.Y > 0)
                AnimatedSprite.Animation = "fall";
            else
                AnimatedSprite.Animation = "jump";
        }

        if(IsOnWallOnly() && velocity.X !=0){
            velocity.Y = WallSlideSpeed;
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
