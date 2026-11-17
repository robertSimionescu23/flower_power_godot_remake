using Godot;

[GlobalClass]
public partial class WallClimbing: PlayerState
{
    public override void Enter()
    {
       Player.Attack();
    }
    public override void Exit()
    {
       Player.AttackBufferTimer.Start();
    }
    public override void Update(double delta)
    {
        base.Update(delta);
        if (Input.IsActionJustPressed("jump"))
        {
            EmitSignal(PlayerState.SignalName.Transitioned, this, "walljumping");
        }
        else if(Player.VerticalClimbHitBox.HasOverlappingBodies()){
            Player.Velocity =  new Vector2(Player.LastPositiveDirectionUsed * Player.WallTraversingSpeed, -Player.WallTraversingSpeed);
        }
        else
            EmitSignal(PlayerState.SignalName.Transitioned, this, "idle");
    }
}
