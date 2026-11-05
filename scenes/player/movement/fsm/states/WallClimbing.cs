using Godot;

[GlobalClass]
public partial class WallClimbing: PlayerState
{
    //TODO: Add attack to this
    public override void Update(double delta)
    {
        base.Update(delta);
        if(GetSpecialCollisionAngle() == 90)
            Player.Velocity =  new Vector2(Player.LastPositiveDirectionUsed * Player.WallTraversingSpeed, -Player.WallTraversingSpeed);
        else
            EmitSignal(PlayerState.SignalName.Transitioned, this, "idle");
    }
}
