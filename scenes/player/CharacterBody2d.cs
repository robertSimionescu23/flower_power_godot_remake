using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    [Export]
    public  float WallSlideSpeed = 200f;
    AnimatedSprite2D AnimatedSprite;

    [Export]
    float JumpBufferTimerDuration = 0.1f;

    Timer JumpBufferTimer;
    Timer WallJumpDurationTimer;
    Vector2 localGravity ;

    private bool IsWallJumping = false;

    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        localGravity = GetGravity();
        JumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
        WallJumpDurationTimer = GetNode<Timer>("WallJumpDurationTimer");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity +=  GetGravity() * (float)delta;
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

        if(!IsWallJumping){
            if (direction != 0)
            {
                velocity.X = direction * Speed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            }
        }
        if (!IsOnFloor())
        {
            if(velocity.Y > 0)
                AnimatedSprite.Animation = "fall";
            else
                AnimatedSprite.Animation = "jump";
        }

        // Handle Jump.
        if(Input.IsActionJustPressed("jump"))
        {
            JumpBufferTimer.Start(JumpBufferTimerDuration);
        }

        if (IsOnFloor() && JumpBufferTimer.TimeLeft > 0)
        {
            velocity.Y = JumpVelocity;
            JumpBufferTimer.Stop();
        }
        else if (IsOnWallOnly() && JumpBufferTimer.TimeLeft > 0)
        {
            IsWallJumping = true;
            WallJumpDurationTimer.Start();
            JumpBufferTimer      .Stop ();

            var jumpDirection = GetWallNormal().X < 0? 1 : -1;
            var diagonalJump = new Vector2(jumpDirection,1);
            GD.Print(diagonalJump);
            velocity = diagonalJump * JumpVelocity;
        }

        if(Input.IsActionJustReleased("jump") && velocity.Y < 0)
            velocity.Y /= 5;

        //Grip to wall based on direction
        if(IsOnWallOnly()){
            if ((Input.IsActionPressed("move_right") && GetWallNormal().X < 0) || (Input.IsActionPressed("move_left") && GetWallNormal().X > 0) )
                   velocity.Y = WallSlideSpeed;
        }

        //TODO: Add wall sliding cooldown


        Velocity = velocity;
        MoveAndSlide();
    }

    public void OnWallJumpDurationTimerTimeout()
    {
        IsWallJumping = false;
    }

}
