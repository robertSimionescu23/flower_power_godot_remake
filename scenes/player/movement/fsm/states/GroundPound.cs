using Godot;

[GlobalClass]
public partial class GroundPound : PlayerState
{
    public override void Update(double delta)
    {
        base.Update(delta);

        Player.Velocity =  new Vector2(0, Player.GroundPoundStrength);

        if(Player.IsOnFloor())
            EmitSignal(PlayerState.SignalName.Transitioned, this, "idle" );
    }

}
