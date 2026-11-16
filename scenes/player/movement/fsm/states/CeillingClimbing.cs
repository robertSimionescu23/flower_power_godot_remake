using Godot;

[GlobalClass]
public partial class CeillingClimbing: PlayerState
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
        if(Input.IsActionJustPressed("pound"))
            EmitSignal(PlayerState.SignalName.Transitioned, this, "groundpound");
        else if(GetSpecialCollisionAngle() == 180)
            Player.Velocity =  new Vector2(Player.LastPositiveDirectionUsed * Player.WallTraversingSpeed, -Player.WallTraversingSpeed);
        else
            EmitSignal(PlayerState.SignalName.Transitioned, this, "idle");
        base.Update(delta);
    }
}
