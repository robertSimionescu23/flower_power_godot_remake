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
        var attackZone = Player.AttackZone;

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
        EmitSignal(PlayerState.SignalName.Transitioned, this, state);
        _dashDurationTimer.Stop();
        _attackBufferTimer.Start();
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        var isColliding = Player.IsOnWall() || Player.IsOnCeiling();

        if(_dashCooldownTimer.IsStopped()){
            if (_upDash)
                Player.Velocity = new Vector2(0, -Player.DashStrength);
            else
                Player.Velocity =  new Vector2(Player.LastPositiveDirectionUsed * Player.DashStrength, 0);

            if (isColliding)
            {
                var specialCollisionAngle = GetSpecialCollisionAngle();
                if(specialCollisionAngle!= null)
                    if(specialCollisionAngle == 90)
                        ChangeState("wallclimbing");
                    else if(specialCollisionAngle == 180)
                        ChangeState("ceillingclimbing");
                else
                    ChangeState("idle");
            }
        CanTransitionToState = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        //dash is available if the Player is grounded and the cooldown has passed
        if(Player.IsOnFloor())
            CanTransitionToState = true;
    }



    public void OnDashDurationTimeout()
    {
        EmitSignal(PlayerState.SignalName.Transitioned, this, "idle");
    }

    public void OnAttackBufferTimeout()
    {
        var attackZone = Player.AttackZone;

        attackZone.Visible    = false;
        attackZone.Monitoring = false;

    }

}
