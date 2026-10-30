using Godot;

[GlobalClass]
public partial class Dashing: PlayerState
{
    private Timer _dashCooldownTimer;
    private Timer _dashDurationTimer;
    private bool _dashAvailable = true;
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
        _dashDurationTimer.Start();
        _upDash = Input.IsActionPressed("move_up");
    }

    public override void Exit()
    {
        _dashDurationTimer.Stop();
        _dashCooldownTimer.Start();
    }
    public override void _Ready()
    {
        base._Ready();
        _dashCooldownTimer = GetNode<Timer>("DashCoolDownTimer");
        _dashDurationTimer = GetNode<Timer>("DashDurationTimer");
    }

    public override void Update(double delta)
    {
        if (_upDash)
            player.Velocity = new Vector2(0, -player.DashStrength);
        else
            player.Velocity =  new Vector2(player.LastPositiveDirectionUsed * player.DashStrength, 0);

        _dashAvailable = false;
    }

    public override void _Process(double delta)
    {
        //dash is available if the player is grounded and the cooldown has passed
        if(player.IsOnFloor() && _dashCooldownTimer.IsStopped())
            _dashAvailable = true;

        TransitionAllowed = _dashAvailable && (!player.IsOnWallOnly() || CheckIfTouchingClimableWall() || _upDash);

        //can dash if: has dash available and either of:
        //1) is touching no wall
        //2) is touchhing a climbale wall
        //3) is updashing (ignores if it's touching walls)
    }

    public bool CheckIfTouchingClimableWall()
    {
        var _touchingClimbableWall = false;

        for (int i = 0; i < player.GetSlideCollisionCount(); i++)
        {
            var collision          = player.GetSlideCollision(i);
            var collidedObject     = collision.GetCollider(); //Get the object player collided with

            if(collidedObject is not TileMapLayer tilemap)
                continue;

            var collisionPosGlobal = collision.GetPosition() - collision.GetNormal() * 3.0f; //Check slightly closer to cell center to avoid duds
            var collisionPosLocal  = tilemap.ToLocal(collisionPosGlobal);
            var cell               = tilemap.LocalToMap(collisionPosLocal);
            var tileData           = tilemap.GetCellTileData(cell);

            if(tileData == null)
                continue;

            if(tileData.GetCustomData("isClimbable").AsBool())
            {
                _touchingClimbableWall = true;
                break;
            }
        }
        return _touchingClimbableWall;
    }

    public void OnDashDurationTimeout()
    {
        _attackBufferTimer.Start();

        GD.Print("Dash stopped!");

        // if(Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left"))
        //     EmitSignal(BaseState.SignalName.Transitioned, this, "moving");
        // else
            EmitSignal(BaseState.SignalName.Transitioned, this, "idle");
    }

    public void OnAttackBufferTimeout()
    {
        var attackZone = player.AttackZone;

        attackZone.Visible    = false;
        attackZone.Monitoring = false;

    }

}
