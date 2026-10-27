using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
    private AnimatedSprite2D AnimatedSprite;
    private Vector2 _localGravity;

    [Export]
    public float Speed = 300.0f;
    [Export]
    public float DirectionChangeSpeed = 50f;
    [Export]
    public float JumpVelocity = -400.0f;
    [Export]
    public float DashStrength = 800f;
    [Export]
    public float WallTraversingSpeed = 350f;


    public int Direction;
    public bool _canMove = true;



    public bool IsFalling;
    private bool _isDashing = false;
    private bool _canDash = true;

    private int _lastPositiveDirectionUsed = 1;

    public bool IsClimbing = false;

    public Area2D _attackZone;


    public bool isWallSliding;
    public bool isCeillingSliding;
    public bool isGroundPounding;

    public Area2D AttackZone;
    private bool _canGroundPound;


    private Timer _jumpBufferTimer;
    private Timer _dashDurationTimer;
    private Timer _freeFallingTimer;
    private Timer _dashCoolDownTimer;
    private Timer _attackBufferTimer;
    // private Timer _attackDurationTimer;
    private Timer _groundPoundActivationTimer;
    private Timer _wallClingBufferTimer;


    //Make wallclimbing possible X seconds after jumping to prevent weird behavior at corners
    private bool _canWallSlide = false;

    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        _localGravity = GetGravity();
        _jumpBufferTimer                = GetNode<Timer>("JumpBufferTimer");
        _dashDurationTimer              = GetNode<Timer>("DashDurationTimer");
        _freeFallingTimer               = GetNode<Timer>("FreeFallingTimer");
        _dashCoolDownTimer              = GetNode<Timer>("DashCoolDownTimer");
        _attackBufferTimer              = GetNode<Timer>("AttackBufferTimer");
        _groundPoundActivationTimer     = GetNode<Timer>("GroundPoundActivationTimer");
        _wallClingBufferTimer           = GetNode<Timer>("WallClingBufferTimer");
        // _attackDurationTimer  = GetNode<Timer>("AttackDurationTimer"   );
        _attackZone = GetNode<Area2D>("AttackShape");
    }

    public Vector2 HandleGravity(double delta, Vector2 velocity)
    {
        if (!IsOnFloor())
        {
            velocity  += GetGravity() * (float)delta;
            velocity.Y = Mathf.Clamp(velocity.Y, JumpVelocity, -JumpVelocity);
        }
        return velocity;
    }

    public float HandleJump(float YVelocity)
    {
        if (_canMove)
        {
            if (Input.IsActionJustPressed("jump"))
            {
                _jumpBufferTimer.Start();
                _groundPoundActivationTimer.Start();
            }

            if (IsOnFloor() && _jumpBufferTimer.TimeLeft > 0)
            {
                return JumpVelocity;
            }

            if (Input.IsActionJustReleased("jump") && YVelocity < 0){
                _groundPoundActivationTimer.Stop();
                return YVelocity /= 5;
            }
        }
        return YVelocity;
    }


    public void HandleDashInput()
    {
        if (Input.IsActionJustPressed("dash") && !_isDashing && _canDash)
        {
            if (_dashCoolDownTimer.IsStopped())
            {

                //While dashing you cannot dash again and cannot input movement
                _isDashing = true;
                _canDash = false;
                _canMove = false;
                _canGroundPound = false;


                //While dashing you also attack
                _attackZone.Visible = true;
                _attackZone.Monitoring = true;


                //Start timers for dash cooldown and dash duration (how long the dash lasts)
                _dashDurationTimer.Start();
                _dashCoolDownTimer.Start();
                _wallClingBufferTimer.Start();
            }
        };
    }

    public Vector2 HandleDashCollisions(Vector2 velocity)
    {
        if (_isDashing)
        {
            //if hitting a wall while dashing, switch to going up the wall
           if (IsOnFloor())
                return new Vector2(_lastPositiveDirectionUsed * DashStrength, 0);
            else
                return new Vector2(_lastPositiveDirectionUsed, -1).Normalized() * -JumpVelocity; //TODO: get a different variable for the air dash
        }

        if(_isDashing || _wallClingBufferTimer.TimeLeft > 0)
        {
            if (IsOnWall())
            {
                _isDashing = false;
                isWallSliding = true;
                isCeillingSliding = false;
                _dashDurationTimer.Stop();
                _wallClingBufferTimer.Stop();
            }
            //if hitting a ceilling while dashing, switch to going up the wall
            else if (IsOnCeilingOnly())
            {
                _isDashing = false;
                isCeillingSliding = true; //TODO: make a ceilling dash duration instead of making it infinite. Could do the same for wall jumping
                isWallSliding = false;
                _dashDurationTimer.Stop();
                _wallClingBufferTimer.Stop();
            }
        }

        velocity = HandleWallTraversal    (velocity);
        velocity = HandleCeillingTraversal(velocity);


        if (IsOnFloor())
            _canDash = true;

        return velocity;

    }


    public Vector2 HandleWallTraversal(Vector2 velocity)
    {
        if (isWallSliding)
        {
            //While on wall, keep going
            if (IsOnWall()){
                _canMove = false;
                _canGroundPound = false;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
            //if off wall, stop
            else
            {
                isWallSliding = false;
                _canMove = true;
                _canGroundPound = true;
                _attackBufferTimer.Start();

                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
        }

        return velocity;
    }

    public Vector2 HandleCeillingTraversal(Vector2 velocity)
    {
        if (isCeillingSliding)
        {
            if (IsOnCeilingOnly())
            {
                _canMove = false;
                _canGroundPound = true;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
            else
            {
                isCeillingSliding = false;
                _canMove = true;
                _canGroundPound = true;
                _attackBufferTimer.Start();
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, 0);
            }
        }

        return velocity;

    }


    //TODO: Use a different "wall traverse speed" instead of dash speed
    //TODO: Make tiles sepcififically made to be interactable by dashing into them instead of using all tiles for this
    public Vector2 HandleDash(Vector2 velocity)
    {

        HandleDashInput();

        velocity = HandleDashCollisions(velocity);

        return velocity;
    }

    //TODO: Rework this to allow ground pound while on ceilling (but not on wall)
    public Vector2 HandlePound(Vector2 velocity)
    {
        if(Input.IsActionJustPressed("pound") && _canGroundPound && !IsOnFloor())
        {
            isGroundPounding = true;
            _canGroundPound  = false;

            _isDashing = false;
            isWallSliding  = false;
            _canDash   = false;
            _canMove   = false;

            _attackZone.Visible    = true;
            _attackZone.Monitoring = true;

            _dashCoolDownTimer.Start();
        }

        if (isGroundPounding)
        {
            if (IsOnFloor())
            {
                _canMove = true;
                isGroundPounding = false;
                _attackBufferTimer.Start();
            }
            else
            {
                isGroundPounding = true;
                _canMove         = false;
                return new Vector2( 0, 2f *  DashStrength); //TODO: Use a dedicated value for this strength
            }

        }

        if (IsOnFloor())
            _canGroundPound = false;


        return velocity;
    }


    public float HandleHorizontalMovement(float XVelocity)
    {
        if (_canMove)
        {
            if (Input.IsActionPressed("move_right"))
            {
                Direction = 1;
                _lastPositiveDirectionUsed = 1;
                AnimatedSprite.FlipH = false;
            }
            else if (Input.IsActionPressed("move_left"))
            {
                Direction = -1;
                _lastPositiveDirectionUsed = -1;
                AnimatedSprite.FlipH = true;
            }
            else
            {
                Direction = 0;
            }

            if (Direction != 0)
                return Direction * Speed;
            else if (Direction == 0)
                return Mathf.MoveToward(XVelocity, 0, 50f); //TODO: Add variable for stop speed
        }

        return XVelocity;
    }

    public void HandleAnimations(Vector2 velocity)
    {
        if (!IsOnFloor())
        {
            //Change animation based on velocity
            AnimatedSprite.Animation = velocity.Y > 0 ? "fall" : "jump";
        }

        if (IsOnFloor())
        {
            AnimatedSprite.Animation = Direction == 0 ? "idle" : "running";
            IsClimbing = false;
        }

        //Prevent "running" into walls
        if (IsOnFloor() && IsOnWall() && Direction != 0)
            AnimatedSprite.Animation = "idle";

    }

    //TODO: Implement actual attack mechanics
    public void OnAttackShapeEntered(Node2D body)
    {
        GD.Print(body.GetType());
    }


    public Vector2 HandleMovement(double delta, Vector2 velocity)
    {
        velocity   = HandleGravity(delta, velocity);
        velocity   = HandleDash   (velocity);
        velocity   = HandlePound  (velocity);
        velocity.Y = HandleJump   (velocity.Y);
        velocity.X = HandleHorizontalMovement(velocity.X);
        // velocity   = HandleWallJumping       (velocity);


        //Handle free falling for camera movement (free falling is falling for X seconds)
        if (velocity.Y <= 0)
        {
            IsFalling = false;
            _freeFallingTimer.Stop();
        }
        else if (_freeFallingTimer.IsStopped())
            _freeFallingTimer.Start();

        return velocity;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        velocity = HandleMovement(delta, velocity);

        HandleAnimations(velocity);



        Velocity = velocity;

        MoveAndSlide();
    }

    private void OnFreeFallingTimerTimeout()
    {
        IsFalling = true;
    }

    private void OnWallClimbEnabledTimerTimeout()
    {
        _canWallSlide = true;
    }


    private void OnDashDurationTimeout()
    {
        _isDashing      = false;
        _canMove        = true;
        _canGroundPound = true;
        _attackBufferTimer.Start();
    }

    //Keep the attack zone for a bit after dash
    private void AttackBufferTimeout()
    {
        _attackZone.Visible = false;
        _attackZone.Monitoring = false;
    }

    public void OnGroundPoundActivationTimeout(){
        _canGroundPound = true;
    }

}
