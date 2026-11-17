using Godot;


public partial class Player : CharacterBody2D
{
    //TODO: clean up some attributes here
    private AnimatedSprite2D AnimatedSprite;

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

        AttackHitBox      = GetNode<Area2D>("AttackHitBox");
        GroundPoundRay  = GetNode<RayCast2D>("RayCast2D");
        KnockBackTimer  = GetNode<Timer>("./Timers/KnockBackTimer");
        GraceTimer      = GetNode<Timer>("./Timers/GraceTimer");
        AttackBufferTimer  = GetNode<Timer>("./Timers/AttackBufferTimer");

        HorizontalClimbHitBox  = GetNode<Area2D>("HorizontalClimbHitBox");
        VerticalClimbHitBox     = GetNode<Area2D>("VerticalClimbHitBox");

        MovementFSM = GetNode<PlayerFsm>("PlayerFsm");
    }

    //If hit by enemy(collision with enemy hurtbox while attack is not up), get knocked back and recover once on the ground TODO: Play a animation and a small delay while
    public void GetDamaged(Vector2 knockBackDirection)
    {
        if(!isInvincible){
            CanMove      = false;
            isHit        = true;
            isInvincible = true;

            KnockBackTimer.Start();

            var knockBackVelocity = _knockBackStrength * knockBackDirection;
            Velocity = knockBackVelocity;

            health -= 1;

            if(health <= 0)
                Respawn();
        }
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
        if(isHit)
            if(IsOnFloor() && KnockBackTimer.IsStopped()){
                CanMove = true;
                isHit   = false;
                Velocity = new Vector2(0,0);

                GraceTimer.Start();
            }
    }

    public void GraceTimerTimeout()
    {
        //TODO: ADD visual clue to this
        isInvincible = false;
    }

    public void Attack()
    {
        AttackHitBox.Visible    = true;
        AttackHitBox.Monitoring = true;

        AttackBufferTimer.Stop(); //To prevent using the buffer time if the dash started before it ended
    }


    public void OnAttackBufferTimeout()
    {
        AttackHitBox.Visible    = false;
        AttackHitBox.Monitoring = false;
    }

}
