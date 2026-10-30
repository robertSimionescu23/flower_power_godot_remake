using Godot;

[GlobalClass]
public partial class Idle : PlayerState
{
    public override void Enter()
    {
        player.Velocity = new Vector2(0,0); //Make the stop smoother in update
    }
    public override void Update(double delta)
    {
        if(Input.IsActionJustPressed("move_right") || Input.IsActionJustPressed("move_left"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "moving");

        else if (player.IsOnFloor() && Input.IsActionJustPressed("jump"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "jumping");

        HandleGravity(delta);
    }
}
