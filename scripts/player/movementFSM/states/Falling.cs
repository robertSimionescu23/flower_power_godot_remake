using Godot;

[GlobalClass]
public partial class Falling : PlayerState
{

    private Timer _jumpBufferTimer;
    private Timer _freeFallTimer;

    public override void  _Ready()
    {
        _jumpBufferTimer = GetNode<Timer>("JumpBufferTimer" );
        _freeFallTimer   = GetNode<Timer>("FreeFallingTimer");
    }

    public override void Enter()
    {
        _freeFallTimer.Start();
    }

    public void ChangeState(string state)
    {
        EmitSignal(PlayerState.SignalName.Transitioned, this, state);
        _freeFallTimer.Stop();
        Player.IsFreeFalling = false;
    }

    public void CheckForJump()
    {
        if (Input.IsActionJustPressed("jump"))
            _jumpBufferTimer.Start();
    }

        public void OnFreeFallingTimerTimeout()
    {
        Player.IsFreeFalling = true;
    }

    public override void Update(double delta)
    {
        if(!Player.GroundPoundRay.IsColliding() && Input.IsActionJustPressed("pound"))
            ChangeState("groundpound");

        else if(Player.IsOnFloor() && _jumpBufferTimer.TimeLeft > 0){
            ChangeState("jumping");
            _jumpBufferTimer.Stop();
        }
        else if(Player.IsOnFloor() && !Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left"))
            ChangeState("idle");
        else if(Player.IsOnFloor())
            ChangeState("moving");
        else if(Input.IsActionJustPressed("dash"))
            ChangeState("dashing");

        HandleMovement();
        CheckForJump();
        HandleGravity(delta);
        base.Update(delta);
    }
}
