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
        if(Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "moving");

        else if (player.IsOnFloor() && Input.IsActionJustPressed("jump"))
            EmitSignal(BaseState.SignalName.Transitioned, this, "jumping");

        else if(Input.IsActionJustPressed("dash"))
             EmitSignal(BaseState.SignalName.Transitioned, this, "dashing");

        else if(player.Velocity.Y > 0){
            EmitSignal(BaseState.SignalName.Transitioned, this, "falling");
        }


        HandleGravity(delta);
    }
}
