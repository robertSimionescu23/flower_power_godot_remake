using Godot;

[GlobalClass]
public partial class WallJumping: PlayerState
{
    public override void Enter()
    {
        Player.Velocity = new Vector2(Player.GetWallNormal().X * Player.WallJumpStrength, 0);
        GD.Print(Player.Velocity);
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        if(!Player.GroundPoundRay.IsColliding() && Input.IsActionJustPressed("pound"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "groundpound");

        else if(Input.IsActionJustPressed("dash"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "dashing");

        else if(Player.IsOnFloor())
            EmitSignal(PlayerState.SignalName.Transitioned, this, "moving");

        HandleGravity(delta);
        // HandleMovement();
    }
}
