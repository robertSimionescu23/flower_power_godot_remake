using Godot;

[GlobalClass]
public partial class GroundPound : PlayerState
{
    [Export]
    private Timer _attackBufferTimer;

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

        Player.Velocity =  new Vector2(0, Player.GroundPoundStrength);

        if(Player.IsOnFloor())
            EmitSignal(PlayerState.SignalName.Transitioned, this, "idle" );

        base.Update(delta);
    }

}
