using Godot;


public partial class Player : CharacterBody2D
{
    //TODO: clean up some attributes here
    private AnimatedSprite2D AnimatedSprite;

    private Color OriginalModulate;

    [Export]
    public float Speed = 300.0f;
    [Export]
    public float WallJumpStrength = 300f;
    [Export]
    public float JumpVelocity = -400.0f;
    [Export]
    public float DirectionChangeSpeed = 50f;
    [Export]
    public float DashStrength = 800f;
    [Export]
    public float WallTraversingSpeed = 350f;
    [Export]
    public float GroundPoundStrength = 800;
    [Export]
    private float _knockBackStrength = 400f;

    private int health = 3;

    public PlayerFsm MovementFSM;


    public int Direction;

    public bool IsFreeFalling;

    public int LastPositiveDirectionUsed = 1;

    public Area2D AttackHitBox;
    public Area2D VerticalClimbHitBox;
    public Area2D HorizontalClimbHitBox;
    public Area2D HurtBox;

    public RayCast2D GroundPoundRay;

    public PlayerState State;

    public bool CanMove = true;
    public bool isHit   = false;
    public bool isInvincible = false;

    public Timer KnockBackTimer;
    public Timer GraceTimer;
    public Timer AttackBufferTimer;

    public Vector2 LastCheckPoint;


    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();
        OriginalModulate = AnimatedSprite.Modulate;

        AttackHitBox      = GetNode<Area2D>("AttackHitBox");
        GroundPoundRay  = GetNode<RayCast2D>("RayCast2D");
        KnockBackTimer  = GetNode<Timer>("./Timers/KnockBackTimer");
        GraceTimer      = GetNode<Timer>("./Timers/GraceTimer");
        AttackBufferTimer  = GetNode<Timer>("./Timers/AttackBufferTimer");

        HorizontalClimbHitBox   = GetNode<Area2D>("HorizontalClimbHitBox");
        VerticalClimbHitBox     = GetNode<Area2D>("VerticalClimbHitBox");
        HurtBox                 = GetNode<Area2D>("HurtBox");

        MovementFSM = GetNode<PlayerFsm>("PlayerFsm");
    }

    //If hit by enemy(collision with enemy hurtbox while attack is not up), get knocked back and recover once on the ground TODO: Play a animation and a small delay while
    public void GetDamaged(Vector2 damageDirection)
    {
        if(!isInvincible){
            CanMove      = false;
            isHit        = true;
            isInvincible = true;

            //Give player a color tint while damaged
            var damagedColor = AnimatedSprite.Modulate;
            damagedColor.A = 0.5f;
            damagedColor.R = 125f;

            AnimatedSprite.Modulate = damagedColor;

            KnockBackTimer.Start();

            var knockBackDirection = new Vector2(0.5f, -0.5f);


            //Standardize knock back direction
            if(damageDirection.X < 0)
                knockBackDirection.X = -0.5f;
            if(damageDirection.Y > 0.3)
                knockBackDirection.Y = 0.5f;

            // GD.Print(damageDirection + " - damage direction " + knockBackDirection + " - knock back direction");

            var knockBackVelocity = _knockBackStrength * knockBackDirection;
            Velocity = knockBackVelocity;

            health -= 1;

            if(health <= 0)
                Respawn();
        }
    }


    public void OnHurtBoxEntered(Node2D body)
    {
        // GD.Print("HIT!");
        var attackedDirection =  GlobalPosition - body.GlobalPosition;
        if (body is TileMapLayer)
            GetDamaged(new Vector2(-Velocity.X, -Velocity.Y));
        else if(body is Enemy enemy)
            if (!enemy.IsAlive)
                return;
            else
                GetDamaged(attackedDirection.Normalized());
        else
            GetDamaged(attackedDirection.Normalized());
    }

    public void Respawn()
    {
        GD.Print("DEAD!");
        Visible = false;
        GlobalPosition = LastCheckPoint;
        Visible = true;
        health = 3;
        Velocity = new Vector2(0,0);
    }

    public void OnAttackShapeEntered(Node2D body)
    {
        var enemy = body as Enemy;
        enemy.GetHit();
    }

   public override void _PhysicsProcess(double delta)
    {
        if(isHit && IsOnFloor() && KnockBackTimer.IsStopped()){
            CanMove = true;
            isHit   = false;
            Velocity = new Vector2(0,0);

            GraceTimer.Start();
        }
    }


    public void GraceTimerTimeout()
    {
        AnimatedSprite.Modulate = OriginalModulate;
        isInvincible = false;
    }

    public void Attack()
    {
        AttackHitBox.Visible    = true;
        AttackHitBox.Monitoring = true;

        //Cannot be hurt while attacking TODO: Change this so that dashing into spikes hurts the player and knocks them out of dash
        HurtBox.Visible    = false;
        HurtBox.Monitoring = false;

        AttackBufferTimer.Stop(); //To prevent using the buffer time if the dash started before it ended
    }

    public void OnAttackBufferTimeout()
    {
        AttackHitBox.Visible    = false;
        AttackHitBox.Monitoring = false;
        HurtBox.Visible    = true;
        HurtBox.Monitoring = true;
    }

}
