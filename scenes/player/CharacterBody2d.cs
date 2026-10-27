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


    public Area2D _attackZone;


    public bool isWallSliding;
    public bool isCeillingSliding;
    public bool isGroundPounding;

    public Area2D AttackZone;
    private RayCast2D _groundPoundRay;


    private Timer _jumpBufferTimer;
    private Timer _dashDurationTimer;
    private Timer _freeFallingTimer;
    private Timer _dashCoolDownTimer;
    private Timer _attackBufferTimer;
    // private Timer _attackDurationTimer;
    private Timer _wallClingBufferTimer;

    [Export]
    private State PlayerState = State.Idle;

    enum State
    {
        Idle,
        Moving,
        Jumping,
        Falling,
        Dashing,
        GroundPound,
        WallTraversing,
        CeillingTraversing

    }



    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        _localGravity = GetGravity();
        _jumpBufferTimer                = GetNode<Timer>("JumpBufferTimer");
        _dashDurationTimer              = GetNode<Timer>("DashDurationTimer");
        _freeFallingTimer               = GetNode<Timer>("FreeFallingTimer");
        _dashCoolDownTimer              = GetNode<Timer>("DashCoolDownTimer");
        _attackBufferTimer              = GetNode<Timer>("AttackBufferTimer");
        _wallClingBufferTimer           = GetNode<Timer>("WallClingBufferTimer");
        // _attackDurationTimer  = GetNode<Timer>("AttackDurationTimer"   );
        _attackZone = GetNode<Area2D>("AttackShape");
        _groundPoundRay = GetNode<RayCast2D>("RayCast2D");
    }

    public Vector2 HandleGravity(double delta, Vector2 velocity)
    {
        if (PlayerState is State.Jumping or State.Falling or State.Idle or State.Moving)
        {
            velocity  += GetGravity() * (float)delta;
            velocity.Y = Mathf.Clamp(velocity.Y, JumpVelocity, -JumpVelocity);
        }
        return velocity;
    }

    public float HandleJump(float YVelocity)
    {
        if (PlayerState is State.Idle or State.Moving)
        {
            if (Input.IsActionJustPressed("jump"))
            {
                _jumpBufferTimer.Start();
            }

            if (IsOnFloor() && _jumpBufferTimer.TimeLeft > 0)
            {
                PlayerState = State.Jumping;
                return JumpVelocity;
            }
        }

        if (PlayerState is State.Jumping && Input.IsActionJustReleased("jump") && YVelocity < 0){
            return YVelocity /= 5;
        }

        if(PlayerState is State.Idle or State.Jumping or State.Moving && YVelocity > 0)
            PlayerState = State.Falling;

        if (PlayerState is State.Idle or State.Moving && YVelocity < 0)
            PlayerState = State.Jumping;

        if(PlayerState is State.Falling && IsOnFloor())
            PlayerState = State.Idle;

        return YVelocity;
    }


    public void HandleDashInput()
    {
        if(
            Input.IsActionJustPressed("dash")
            && PlayerState is not (State.Dashing or State.GroundPound or State.WallTraversing or State.CeillingTraversing)
            && _canDash
        ){
            if (_dashCoolDownTimer.IsStopped())
            {

                PlayerState = State.Dashing;
                _canDash = false;

                //Start timers for dash cooldown and dash duration (how long the dash lasts)
                _dashDurationTimer.Start();
                _dashCoolDownTimer.Start();
                _wallClingBufferTimer.Start();
            }
        };
    }

    public Vector2 HandleDashCollisions(Vector2 velocity)
    {
        if (PlayerState is State.Dashing)
        {
           if (IsOnFloor())
                return new Vector2(_lastPositiveDirectionUsed * DashStrength, 0);
            else
                return new Vector2(_lastPositiveDirectionUsed, -1).Normalized() * -JumpVelocity; //TODO: get a different variable for the air dash
        }

        if(PlayerState is State.Dashing || _wallClingBufferTimer.TimeLeft > 0)
        {
            //if hitting a wall while dashing, switch to going up the wall
            if (IsOnWall())
            {
                // _isDashing = false;
                // isWallSliding = true;
                // isCeillingSliding = false;

                PlayerState = State.WallTraversing;

                _dashDurationTimer.Stop();
                _wallClingBufferTimer.Stop();
            }
            //if hitting a ceilling while dashing, switch to going up the wall
            else if (IsOnCeilingOnly())
            {
                // _isDashing = false;
                // isCeillingSliding = true; //TODO: make a ceilling dash duration instead of making it infinite. Could do the same for wall jumping
                // isWallSliding = false;

                PlayerState = State.CeillingTraversing;

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
        if (PlayerState is State.WallTraversing)
        {
            //While on wall, keep going
            if (IsOnWall()){
                // _canMove = false;
                // _canGroundPound = false;
                PlayerState = State.WallTraversing;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
            //if off wall, stop
            else
            {
                // isWallSliding = false;
                // _canMove = true;
                // _canGroundPound = true;

                PlayerState = State.Idle;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
        }

        return velocity;
    }

    public Vector2 HandleCeillingTraversal(Vector2 velocity)
    {
        if (PlayerState is State.CeillingTraversing)
        {
            if (IsOnCeilingOnly())
            {
                // _canMove = false;
                // _canGroundPound = true;
                PlayerState = State.CeillingTraversing;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
            else
            {
                // isCeillingSliding = false;
                // _canMove = true;
                // _canGroundPound = true;

                PlayerState = State.Idle;
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

        if(
            Input.IsActionJustPressed("pound") &&
            PlayerState is State.Jumping or State.Falling or State.CeillingTraversing &&
            !_groundPoundRay.IsColliding()
        ){

            PlayerState = State.GroundPound;

            _dashCoolDownTimer.Start();
        }

        if (PlayerState is State.GroundPound)
        {
            if (IsOnFloor())
            {
                PlayerState = State.Idle;
            }
            else
            {
                return new Vector2( 0, 2f *  DashStrength); //TODO: Use a dedicated value for this strength
            }

        }

        return velocity;
    }


    public float HandleHorizontalMovement(float XVelocity)
    {
        if (PlayerState is State.Moving or State.Idle or State.Falling or State.Jumping)
        {
            if (Input.IsActionPressed("move_right"))
            {
                Direction = 1;
                _lastPositiveDirectionUsed = 1;
                if(PlayerState is State.Idle  && XVelocity != 0)
                    PlayerState = State.Moving;
                AnimatedSprite.FlipH = false;
            }
            else if (Input.IsActionPressed("move_left"))
            {
                Direction = -1;
                _lastPositiveDirectionUsed = -1;
                if(PlayerState is State.Idle && XVelocity != 0)
                    PlayerState = State.Moving;
                AnimatedSprite.FlipH = true;
            }
            else
            {
                Direction = 0;
                if(PlayerState is State.Moving)
                    PlayerState = State.Idle;
            }

            if (Direction != 0)
                return Direction * Speed;
            else if (Direction == 0)
                return Mathf.MoveToward(XVelocity, 0, 50f); //TODO: Add variable for stop speed
        }

        return XVelocity;
    }

    public void HandleAnimations()
    {
        string animation = PlayerState switch
        {
            State.Idle    => "idle",
            State.Moving  => "running",
            State.Jumping => "jump",
            State.Falling => "fall",
            _ => ""
        };

        if (!string.IsNullOrEmpty(animation) &&
            AnimatedSprite.Animation != animation)
        {
            AnimatedSprite.Play(animation);
        }
    }

    public void HandleAttack()
    {
        if (PlayerState is State.Dashing or State.CeillingTraversing or State.WallTraversing or State.GroundPound)
        {
            _attackZone.Visible    = true;
            _attackZone.Monitoring = true;
            _attackBufferTimer.Stop();
        }
        else if(_attackBufferTimer.TimeLeft == 0)
        {
            _attackBufferTimer.Start();
        }
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


        HandleAttack();

        Velocity = velocity;

        MoveAndSlide();
        HandleAnimations();
    }

    private void OnFreeFallingTimerTimeout()
    {
        IsFalling = true;
    }


    private void OnDashDurationTimeout()
    {
        PlayerState = State.Idle;
    }

    //Keep the attack zone for a bit after dash
    private void AttackBufferTimeout()
    {
        _attackZone.Visible = false;
        _attackZone.Monitoring = false;
    }

}
