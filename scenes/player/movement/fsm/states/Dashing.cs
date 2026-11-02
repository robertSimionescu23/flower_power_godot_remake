using Godot;

[GlobalClass]
public partial class Dashing: PlayerState
{
    private Timer _dashCooldownTimer;
    private Timer _dashDurationTimer;
    private bool _upDash  = false;

    [Export]
    private Timer _attackBufferTimer;

    private void Attack()
    {
        var attackZone = player.AttackZone;

        attackZone.Visible    = true;
        attackZone.Monitoring = true;

        _attackBufferTimer.Stop(); //To prevent using the buffer time if the dash started before it ended
    }

    public override void Enter()
    {
        Attack();
        _dashDurationTimer.Start();
        _upDash = Input.IsActionPressed("move_up");
    }

    public override void Exit()
    {
        _dashDurationTimer.Stop();
        _dashCooldownTimer.Start();
        _attackBufferTimer.Start();
    }
    public override void _Ready()
    {
        base._Ready();
        _dashCooldownTimer = GetNode<Timer>("DashCoolDownTimer");
        _dashDurationTimer = GetNode<Timer>("DashDurationTimer");
    }

    public void ChangeState(string state)
    {
        EmitSignal(BaseState.SignalName.Transitioned, this, state);
        _dashDurationTimer.Stop();
        _attackBufferTimer.Start();
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        var isColliding = player.IsOnWall() || player.IsOnCeiling();

        if(_dashCooldownTimer.IsStopped()){
            if (_upDash)
                player.Velocity = new Vector2(0, -player.DashStrength);
            else
                player.Velocity =  new Vector2(player.LastPositiveDirectionUsed * player.DashStrength, 0);

            if (isColliding)
            {
                if(GetSpecialCollisionAngle()!= null)
                    ChangeState("climbing");
                else
                    ChangeState("idle");
            }



        TransitionAllowed = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        //dash is available if the player is grounded and the cooldown has passed
        if(player.IsOnFloor())
            TransitionAllowed = true;
    }



    public void OnDashDurationTimeout()
    {
        EmitSignal(BaseState.SignalName.Transitioned, this, "idle");
    }

    public void OnAttackBufferTimeout()
    {
        var attackZone = player.AttackZone;

        attackZone.Visible    = false;
        attackZone.Monitoring = false;

    }

}
