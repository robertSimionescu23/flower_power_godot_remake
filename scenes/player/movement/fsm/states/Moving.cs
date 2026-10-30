using Godot;

[GlobalClass]
public partial class Moving : PlayerState
{
    public override void Update(double delta)
    {
        if(!Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "idle");

        else if (player.IsOnFloor() && Input.IsActionJustPressed("jump"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "jumping");

        HandleMovement();
        HandleGravity(delta);
    }
}
