using Godot;

[GlobalClass]
public partial class Idle : PlayerState
{
    public override void Enter()
    {
        if(Player.CanMove) //Do not stop movement if player is already immobile
            Player.Velocity = new Vector2(0,0); //Make the stop smoother in update
    }
    public override void Update(double delta)
    {
        if(Player.CanMove){ //Movement if handled elesewhere while immobile (it is force upon the player)
            if(Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left"))
                EmitSignal(PlayerState.SignalName.Transitioned, this, "moving");

            else if (Player.IsOnFloor() && Input.IsActionJustPressed("jump"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "jumping");

            else if(Player.Velocity.Y > 0)
                EmitSignal(PlayerState.SignalName.Transitioned, this, "falling");
        }

        HandleGravity(delta);
        base.Update(delta);
    }
}
