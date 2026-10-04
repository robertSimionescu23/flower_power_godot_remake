using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
    [Export]
    public float Speed = 300.0f;
    [Export]
    public float JumpVelocity = -400.0f;
    [Export]
    public float DashStrength = 800f;
    [Export]
    public  float WallSlideSpeed = 200f;
    private AnimatedSprite2D AnimatedSprite;

    private Timer _jumpBufferTimer;
    private Timer _wallJumpDurationTimer;
    private Timer _wallClimbEnabledTimer;
    private Timer _dashDurationTimer;
    private Vector2 _localGravity ;

    private bool _isWallJumping = false;
    private bool _isDashing     = false;

    private int _lastPositiveDirectionUsed = 1;



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
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity +=  GetGravity() * (float)delta;

            //Change animation based on velocity
            AnimatedSprite.Animation = velocity.Y > 0? "fall" : "jump";
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        int _direction;

        if (Input.IsActionPressed("move_right")){
            _direction = 1;
            _lastPositiveDirectionUsed = 1;
            AnimatedSprite.FlipH = false;
        }
        else if (Input.IsActionPressed("move_left")){
            _direction = -1;
            _lastPositiveDirectionUsed = -1;
            AnimatedSprite.FlipH = true;
        }
        else{
            _direction = 0;
        }

        if(IsOnFloor())
            AnimatedSprite.Animation = _direction == 0? "idle" : "running";


        //Add wall sliding cooldown
        if(IsOnFloor())
            _canWallSlide = false;
        else if (_wallClimbEnabledTimer.IsStopped())
            _wallClimbEnabledTimer.Start();

        //Prevent "running" into walls
        if(IsOnFloor() && IsOnWall() && _direction != 0)
            AnimatedSprite.Animation = "idle";

        if(!_isWallJumping && !_isDashing)
            velocity.X = _direction != 0 ?
                _direction * Speed                     :
                Mathf.MoveToward(Velocity.X, 0, Speed);

        // Handle Jump.
        if(Input.IsActionJustPressed("jump"))
        {
            _jumpBufferTimer.Start();
        }

        if (IsOnFloor() && _jumpBufferTimer.TimeLeft > 0)
        {
            velocity.Y = JumpVelocity;
            _jumpBufferTimer.Stop();
        }

        if(Input.IsActionJustReleased("jump") && velocity.Y < 0)
            velocity.Y /= 5;

        //Grip to wall based on _direction
        if(IsOnWallOnly()){
            var isPressingWallRight = Input.IsActionPressed("move_right") && GetWallNormal().X < 0f;
            var isPressingWallLeft  = Input.IsActionPressed("move_left")  && GetWallNormal().X > 0f;

            //If hugging a wall (on the left or on the right) while not grounded, slide on wall
            if ((isPressingWallRight || isPressingWallLeft) && _canWallSlide)
                velocity.Y = WallSlideSpeed;


            if (_jumpBufferTimer.TimeLeft > 0)
            {
                _isWallJumping = true;

                _wallJumpDurationTimer.Start();
                _jumpBufferTimer      .Stop ();

                var jump_Direction = isPressingWallRight? 1 : -1; //Change jump _direction based on where the wall is
                var diagonalJump = new Vector2(jump_Direction, 2).Normalized();

                velocity = diagonalJump * JumpVelocity;
            }
        }

        //TODO: Add dash cooldown

        if(Input.IsActionJustPressed("dash")){
            _isDashing = true;
            _dashDurationTimer.Start();
        };

        if(_isDashing){
            velocity = new Vector2(_lastPositiveDirectionUsed * DashStrength, 0);

        }
        GD.Print(_isDashing);


        Velocity = velocity;
        MoveAndSlide();
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
