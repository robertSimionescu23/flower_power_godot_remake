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
    [Export]
    public bool _canMove = true;

    private Timer _jumpBufferTimer;
    private Timer _wallJumpDurationTimer;
    private Timer _wallClimbEnabledTimer;
    private Timer _dashDurationTimer;
    private Timer _freeFallingTimer;
    private Timer _dashCoolDownTimer;
    private Timer _attackBufferTimer;
    private Timer _attackDurationTimner;

    private Vector2 _localGravity ;

    private bool _isWallJumping = false;

    public bool IsFalling;
    private bool _isDashing     = false;
    private bool _canDash       = true;

    private int _lastPositiveDirectionUsed = 1;

    public bool IsClimbing = false;

    public Area2D _attackZone;


    public bool isWallSliding;
    public bool isCeillingSliding;



    //Make wallclimbing possible X seconds after jumping to prevent weird behavior at corners
    private bool _canWallSlide = false;

    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        _localGravity = GetGravity();
        _jumpBufferTimer       = GetNode<Timer>("JumpBufferTimer"       );
        _wallJumpDurationTimer = GetNode<Timer>("WallJumpDurationTimer" );
        _wallClimbEnabledTimer = GetNode<Timer>("WallClimbEnabledTimer" );
        _dashDurationTimer     = GetNode<Timer>("DashDurationTimer"     );
        _freeFallingTimer      = GetNode<Timer>("FreeFallingTimer"      );
        _dashCoolDownTimer     = GetNode<Timer>("DashCoolDownTimer"     );
        _attackBufferTimer    = GetNode<Timer>("AttackBufferTimer"     );
        // _attackDurationTimner  = GetNode<Timer>("AttackDurationTimer"   );
        _attackZone            = GetNode<Area2D>("AttackShape");
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
        if(_canMove){
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
            }
        return YVelocity;
    }
    //TODO: Handle ceilling grabs
    //TODO: Make tiles sepcififically made to be interactable by dashing into them instead of using all tiles for this
    //Make a new state for going up a wall or reuse wallsliding
    //Go from dash -> hit wall -> stop dashing -> Go up wall until the wall is gone
    public Vector2 HandleDash(Vector2 velocity)
    {

        if(Input.IsActionJustPressed("dash") && !_isDashing && _canDash && !IsOnWall()){
            if(_dashCoolDownTimer.IsStopped()){
                var attackZone = GetNode<Area2D>("AttackShape");

                //While dashing you cannot dash again and cannot input movement
                _isDashing = true;
                _canDash   = false;
                _canMove   = false;


                //While dashing you also attack
                attackZone.Visible = true;
                attackZone.Monitoring = true;


                //Start timers for dash cooldown and dash duration (how long the dash lasts)
                _dashDurationTimer.Start();
                _dashCoolDownTimer.Start();
            }
        };

        if(_isDashing){
            //if hitting a wall while dashing, switch to going up the wall
            if(IsOnWall()){
               _isDashing = false;
               isWallSliding = true;
               _dashDurationTimer.Stop();
            }
            else if(IsOnFloor())
                return new Vector2(_lastPositiveDirectionUsed * DashStrength, 0);
            else if (IsOnCeiling()){
                isCeillingSliding = true; //TODO: make a ceilling dash duration instead of making it infinite. Could do the same for wall jumping
                _isDashing        = false;
            }
            else
                return new Vector2(_lastPositiveDirectionUsed, -1).Normalized() * DashStrength; //TODO: get a different variable for the air dash

        }

        //TODO: Make it so you can't chain dashes from wall to ceilling
        if (isWallSliding)
        {

            //While on wall, keep going
            if(IsOnWall())
                return new Vector2(_lastPositiveDirectionUsed * DashStrength, JumpVelocity);
            //if off wall, stop
            else{
                isWallSliding = false;
                _canMove      = true;
               _attackBufferTimer.Start();

                return new Vector2(_lastPositiveDirectionUsed * Speed, JumpVelocity);
            }
        }

        if (isCeillingSliding)
        {
            if (IsOnCeiling())
            {
                return new Vector2(_lastPositiveDirectionUsed * DashStrength, JumpVelocity);
            }
            else
            {
                _canMove = true;
                isCeillingSliding = true;
                _attackBufferTimer.Start();
            }
        }

        if(IsOnFloor())
            _canDash = true;

        return velocity;
    }


    public float HandleHorizontalMovement(float XVelocity)
    {
        if(_canMove){
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
        }

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

    //TODO: Implement actual attack mechanics
    public void OnAttackShapeEntered(Node2D body)
    {
        GD.Print(body.GetType());
    }


    public Vector2 HandleMovement(double delta, Vector2 velocity)
    {
        velocity   = HandleGravity           (delta, velocity);
        velocity   = HandleDash              (velocity);
        velocity.Y = HandleJump              (velocity.Y);
        velocity.X = HandleHorizontalMovement(velocity.X);
        // velocity   = HandleWallJumping       (velocity);

        return velocity;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        velocity = HandleMovement(delta, velocity);
        HandleAnimations(velocity);

        if(velocity.Y <= 0){
            IsFalling = false;
            _freeFallingTimer.Stop();
        }
        else if(_freeFallingTimer.IsStopped())
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
        _canMove   = true;
        _attackBufferTimer.Start();
    }

    //Keep the attack zone for a bit after dash
    private void AttackBufferTimeout()
    {
        _attackZone.Visible = false;
        _attackZone.Monitoring = false;
    }

}
