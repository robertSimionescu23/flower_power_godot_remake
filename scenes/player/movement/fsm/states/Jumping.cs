using Godot;

[GlobalClass]
public partial class Jumping : PlayerState
{

    public override void Enter()
    {
        Player.Velocity = new Vector2 (Player.Velocity.X, Player.JumpVelocity);
    }

    public override void Update(double delta)
    {
        if(!Player.GroundPoundRay.IsColliding() && Input.IsActionJustPressed("pound"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "groundpound");

        else if(Input.IsActionJustPressed("dash"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "dashing");

        else if (Input.IsActionJustReleased("jump") && Player.Velocity.Y < 0)
            Player.Velocity = new Vector2 (Player.Velocity.X, Player.Velocity.Y / 5);

        else if(Player.Velocity.Y >= 0)
            EmitSignal(PlayerState.SignalName.Transitioned, this, "falling");


        HandleMovement();
        HandleGravity(delta);
        base.Update(delta);
    }
}
