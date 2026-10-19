using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
    //TODO: Refactor this into functions easier to follow
    [Export]
    public float Speed = 300.0f;
    [Export]
    public float DirectionChangeSpeed = 50f;
    [Export]
    public float JumpVelocity = -400.0f;
    [Export]
    public float DashStrength = 800f;
    [Export]
    public  float WallSlideSpeed = 200f;
    private AnimatedSprite2D AnimatedSprite;
    public int Direction;

    private Timer _jumpBufferTimer;
    private Timer _wallJumpDurationTimer;
    private Timer _wallClimbEnabledTimer;
    private Timer _dashDurationTimer;
    private Timer _freeFallingTimer;
    private Timer _dashCoolDownTimer;
    private Vector2 _localGravity ;

    private bool _isWallJumping = false;

    public bool IsFalling;
    private bool _isDashing     = false;
    private bool _canDash       = true;

    private int _lastPositiveDirectionUsed = 1;

    public bool IsClimbing = false;



    //Make wallclimbing possible X seconds after jumping to prevent weird behavior at corners
    private bool _canWallSlide = false;

    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        _localGravity = GetGravity();
        _jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
        _wallJumpDurationTimer = GetNode<Timer>("WallJumpDurationTimer");
        _wallClimbEnabledTimer = GetNode<Timer>("WallClimbEnabledTimer");
        _dashDurationTimer     = GetNode<Timer>("DashDurationTimer");
        _freeFallingTimer      = GetNode<Timer>("FreeFallingTimer");
        _dashCoolDownTimer     = GetNode<Timer>("DashCoolDownTimer");
    }

    public Vector2 HandleGravity(double delta, Vector2 velocity)
    {
        if (!IsOnFloor())
        {
            velocity +=  GetGravity() * (float)delta;
            velocity.Y = Mathf.Clamp(velocity.Y, JumpVelocity, -JumpVelocity);
        }
        return velocity;
    }

    public float HandleJump(float YVelocity)
    {
        if(Input.IsActionJustPressed("jump"))
        {
            _jumpBufferTimer.Start();
        }

        if (IsOnFloor() && _jumpBufferTimer.TimeLeft > 0)
        {
            _jumpBufferTimer.Stop();
            return JumpVelocity;
        }

        if(Input.IsActionJustReleased("jump") && YVelocity < 0)
           return YVelocity /= 5;

        return YVelocity;
    }


    public Vector2 HandleDash(Vector2 velocity)
    {

        if(Input.IsActionJustPressed("dash") && !_isDashing && _canDash && !IsOnWall()){
            if(_dashCoolDownTimer.TimeLeft == 0){
                _isDashing = true;
                _canDash   = false;
                _dashDurationTimer.Start();
                _dashCoolDownTimer.Start();
            }
        };

        if(_isDashing){
            return new Vector2(_lastPositiveDirectionUsed * DashStrength, 0);
        }

        if(IsOnFloor())
            _canDash = true;

        return velocity;
    }

    public float HandleHorizontalMovement(float XVelocity)
    {
          if (Input.IsActionPressed("move_right")){
            Direction = 1;
            _lastPositiveDirectionUsed = 1;
            AnimatedSprite.FlipH = false;
        }
        else if (Input.IsActionPressed("move_left")){
            Direction = -1;
            _lastPositiveDirectionUsed = -1;
            AnimatedSprite.FlipH = true;
        }
        else{
            Direction = 0;
        }

        if(!_isWallJumping && !_isDashing && Direction != 0)
            return Direction * Speed;
        else if (!_isWallJumping && !_isDashing && Direction == 0)
            return Mathf.MoveToward(XVelocity, 0, 50f); //TODO: Add variable for stop speed

        return XVelocity;
    }

    public void HandleAnimations(Vector2 velocity)
    {
         if (!IsOnFloor())
        {
            //Change animation based on velocity
            AnimatedSprite.Animation = velocity.Y > 0? "fall" : "jump";
        }

        if(IsOnFloor()){
            AnimatedSprite.Animation = Direction == 0? "idle" : "running";
            IsClimbing = false;
        }

        //Prevent "running" into walls
        if(IsOnFloor() && IsOnWall() && Direction != 0)
            AnimatedSprite.Animation = "idle";

    }

    public Vector2 HandleMovement(double delta, Vector2 velocity)
    {
        velocity   = HandleGravity           (delta, velocity);
        velocity   = HandleDash              (velocity);
        velocity.Y = HandleJump              (velocity.Y);
        velocity.X = HandleHorizontalMovement(velocity.X);

        return velocity;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        velocity = HandleMovement(delta, velocity);

        HandleAnimations(velocity);

        //Add wall sliding cooldown
        if(IsOnFloor())
            _canWallSlide = false;
        else if (_wallClimbEnabledTimer.IsStopped())
            _wallClimbEnabledTimer.Start();


        //Grip to wall based on Direction
        if(IsOnWallOnly()){
            var isPressingWallRight = Input.IsActionPressed("move_right") && GetWallNormal().X < 0f;
            var isPressingWallLeft  = Input.IsActionPressed("move_left")  && GetWallNormal().X > 0f;
            IsClimbing = true;

            //If hugging a wall (on the left or on the right) while not grounded, slide on wall
            if ((isPressingWallRight || isPressingWallLeft) && _canWallSlide)
                velocity.Y = WallSlideSpeed;


            if (_jumpBufferTimer.TimeLeft > 0)
            {
                _isWallJumping = true;

                _wallJumpDurationTimer.Start();
                _jumpBufferTimer      .Stop ();

                var jumpDirection = isPressingWallRight? 1 : -1; //Change jump Direction based on where the wall is
                var diagonalJump = new Vector2(jumpDirection, 2).Normalized();

                velocity = diagonalJump * JumpVelocity;
            }
        }



        if(velocity.Y <= 0){
            IsFalling = false;
            _freeFallingTimer.Stop();
        }
        else if(_freeFallingTimer.TimeLeft == 0)
            _freeFallingTimer.Start();

        Velocity = velocity;
        MoveAndSlide();
    }

    private void OnFreeFallingTimerTimeout(){
        IsFalling = true;
    }

    private void OnWallJumpDurationTimerTimeout()
    {
        _isWallJumping = false;
    }

    private void OnWallClimbEnabledTimerTimeout(){
        _canWallSlide = true;
    }


    private void OnDashDurationTimeout()
    {
        _isDashing = false;
    }

}
