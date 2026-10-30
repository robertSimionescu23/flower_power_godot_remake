using Godot;

[GlobalClass]
public partial class Falling : PlayerState
{

    private Timer _jumpBufferTimer;

    public override void  _Ready()
    {
        _jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
    }

    public void CheckForJump()
    {
        if (Input.IsActionJustPressed("jump"))
        {
            _jumpBufferTimer.Start();
        }

        if (player.IsOnFloor() && _jumpBufferTimer.TimeLeft > 0)
        {
            EmitSignal(BaseState.SignalName.Transitioned, this, "jumping");
            _jumpBufferTimer.Stop();
        }
    }
    //TODO: Implement the free falling timer
    public override void Update(double delta)
    {
        if(player == null)
        {
            GD.PrintErr("Player is null!");
            return;
        }
        if(player.IsOnFloor() && !Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "idle");
        else if(player.IsOnFloor() && (Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left")))
            EmitSignal(BaseState.SignalName.Transitioned, this, "moving");
        else if(player.IsOnFloor())
            EmitSignal(BaseState.SignalName.Transitioned, this, "moving");
        else if(Input.IsActionJustPressed("dash"))
             EmitSignal(BaseState.SignalName.Transitioned, this, "dashing");

        HandleMovement();
        CheckForJump();
        HandleGravity(delta);
    }
}
