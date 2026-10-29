using Godot;
using System;
public enum State
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


public partial class Player : CharacterBody2D
{
    private AnimatedSprite2D AnimatedSprite;

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

    [Export]
    public float GroundPoundStrength = 1600f;


    public int Direction;
    public bool _canMove = true;

    public bool IsFreeFalling;
    private bool _canDash = true;

    private int _lastPositiveDirectionUsed = 1;

    public Area2D _attackZone;

    private RayCast2D _groundPoundRay;

    private Timer _jumpBufferTimer;
    private Timer _dashDurationTimer;
    private Timer _freeFallingTimer;
    private Timer _dashCoolDownTimer;
    private Timer _attackBufferTimer;
    private Timer _wallClingBufferTimer;

    public State PlayerState = State.Idle;

    private bool _canClimb = false;
    private bool _upDash   = false;


    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        _jumpBufferTimer                = GetNode<Timer>("Timers/JumpBufferTimer");
        _dashDurationTimer              = GetNode<Timer>("Timers/DashDurationTimer");
        _freeFallingTimer               = GetNode<Timer>("Timers/FreeFallingTimer");
        _dashCoolDownTimer              = GetNode<Timer>("Timers/DashCoolDownTimer");
        _attackBufferTimer              = GetNode<Timer>("Timers/AttackBufferTimer");
        _wallClingBufferTimer           = GetNode<Timer>("Timers/WallClingBufferTimer");

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

        if(PlayerState is State.Falling && IsOnFloor()){
            PlayerState = State.Idle;
        }

        if(PlayerState is State.Falling)
        {
            if(_freeFallingTimer.IsStopped() && !IsFreeFalling){
                IsFreeFalling = false;
                _freeFallingTimer.Start();
            }
        }
        else
        {
            IsFreeFalling = false;
            if(_freeFallingTimer.TimeLeft > 0)
                _freeFallingTimer.Stop();
        }

        return YVelocity;
    }


    public void HandleDashInput()
    {
        var isNotHuggingNormalWall = !IsOnWall() || (IsOnWall() && _canClimb) || Input.IsActionPressed("move_up"); //can dash while hugging wall only if that wall is climbable


        if(
            Input.IsActionJustPressed("dash")
            && PlayerState is not (State.Dashing or State.GroundPound or State.WallTraversing or State.CeillingTraversing)
            && _canDash
            && isNotHuggingNormalWall
        ){
            if (_dashCoolDownTimer.IsStopped())
            {

                _upDash = Input.IsActionPressed("move_up");

                PlayerState = State.Dashing;

                _canDash = false;

                //Start timers for dash cooldown and dash duration (how long the dash lasts)
                _dashDurationTimer   .Start();
                _dashCoolDownTimer   .Start();
                _wallClingBufferTimer.Start();
            }
        };
    }

    public Vector2 HandleDashCollisions(Vector2 velocity)
    {
        //Check if the player is colliding to climbale walls that influence dash behavior
        CheckForClimbableWall();

        if (PlayerState is State.Dashing)
        {
            if(_upDash)
                return new Vector2(0, -DashStrength);

            return new Vector2(_lastPositiveDirectionUsed * DashStrength, 0);
        }

        if(PlayerState is State.Dashing || _wallClingBufferTimer.TimeLeft > 0)
        {
            //if hitting a rock wall while dashing, switch to going up the wall
            if (_canClimb && IsOnWall())
            {
                PlayerState = State.WallTraversing;

                _dashDurationTimer.Stop();
                _wallClingBufferTimer.Stop();
            }

            //If hitting a normal wall while dashing, stop the dash
            else if(!_canClimb && IsOnWall())
            {
                PlayerState = State.Idle;

                _dashDurationTimer.Stop();
                _wallClingBufferTimer.Stop();

            }
            //if hitting a rock ceilling while dashing, switch to going up the wall
            else if (IsOnCeilingOnly() && _canClimb)
            {
                 //TODO: make a ceilling dash duration instead of making it infinite. Could do the same for wall jumping
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
            if (_canClimb){
                PlayerState = State.WallTraversing;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
            //if off wall, stop
            else
            {
                PlayerState = State.Idle;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity / 2);
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
                PlayerState = State.CeillingTraversing;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, JumpVelocity);
            }
            else
            {

                PlayerState = State.Idle;
                return new Vector2(_lastPositiveDirectionUsed * WallTraversingSpeed, 0);
            }
        }

        return velocity;

    }

    //TODO: Make tiles sepcififically made to be interactable by dashing into them instead of using all tiles for this
    public Vector2 HandleDash(Vector2 velocity)
    {

        HandleDashInput();

        velocity = HandleDashCollisions(velocity);

        return velocity;
    }

    public Vector2 HandleGroundPound(Vector2 velocity)
    {
        //TODO: Add visual cue that a floor pound is possible

        //Ignore distance requirment for pounding while on ceilling
        var isHighEnough = PlayerState is State.Jumping or State.Falling && !_groundPoundRay.IsColliding();

        if(Input.IsActionJustPressed("pound") && ( isHighEnough || PlayerState is State.CeillingTraversing))
        {
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
                return new Vector2( 0, GroundPoundStrength); //TODO: Use a dedicated value for this strength
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
                return Mathf.MoveToward(XVelocity, 0, DirectionChangeSpeed);
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
            _attackBufferTimer.Stop(); //To prevent using the buffer time if the dash started before it ended
        }
        else if(_attackBufferTimer.TimeLeft == 0)
        {
            _attackBufferTimer.Start();
        }
    }

    //TODO: Implement actual attack mechanics
    public void OnAttackShapeEntered(Node2D body)
    {
        // if (body is TileMapLayer tileMapLayer)
        //     GD.Print(tileMapLayer.TileSet.Name);
    }


    public Vector2 HandleMovement(double delta, Vector2 velocity)
    {
        velocity   = HandleGravity(delta, velocity);
        velocity   = HandleDash   (velocity);
        velocity   = HandleGroundPound  (velocity);
        velocity.Y = HandleJump   (velocity.Y);
        velocity.X = HandleHorizontalMovement(velocity.X);

        return velocity;
    }


    public void CheckForClimbableWall()
    {
        _canClimb = false;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision          = GetSlideCollision(i);
            var collidedObject     = collision.GetCollider(); //Get the object player collided with

            if(collidedObject is not TileMapLayer tilemap)
                continue;

            var collisionPosGlobal = collision.GetPosition() - collision.GetNormal() * 3.0f; //Check slightly closer to cell center to avoid duds
            var collisionPosLocal  = tilemap.ToLocal(collisionPosGlobal);
            var cell               = tilemap.LocalToMap(collisionPosLocal);
            var tileData           = tilemap.GetCellTileData(cell);

            if(tileData == null)
                continue;

            if(tileData.GetCustomData("isClimbable").AsBool())
            {
                _canClimb = true;
                break;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        velocity = HandleMovement(delta, velocity);

        HandleAttack();

        Velocity = velocity;

        MoveAndSlide();

        // GD.Print(PlayerState);

        HandleAnimations();
    }

    private void OnFreeFallingTimerTimeout()
    {
        IsFreeFalling = true;
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
