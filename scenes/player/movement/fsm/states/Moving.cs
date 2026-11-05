using Godot;

[GlobalClass]
public partial class Moving : PlayerState
{
    public override void Update(double delta)
    {


        if(!Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "idle");

        else if (Player.IsOnFloor() && Input.IsActionJustPressed("jump"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "jumping");

        else if(Player.Velocity.Y > 0)
            EmitSignal(PlayerState.SignalName.Transitioned, this, "falling");

        HandleMovement();
        HandleGravity(delta);
        base.Update(delta);
    }
}
