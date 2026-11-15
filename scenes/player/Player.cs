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
    public float GroundPoundStrength = 1600f;

    public PlayerFsm MovementFSM;


    public int Direction;

    public bool IsFreeFalling;

    public int LastPositiveDirectionUsed = 1;

    public Area2D AttackZone;

    public RayCast2D GroundPoundRay;

    public PlayerState State;

    public bool CanMove = true;

    public Timer KnockBackTimer;


    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        AttackZone      = GetNode<Area2D>("AttackShape");
        GroundPoundRay  = GetNode<RayCast2D>("RayCast2D");
        KnockBackTimer  = GetNode<Timer>("./Timers/KnockBackTimer");

        MovementFSM = GetNode<PlayerFsm>("PlayerFsm");
    }

    //TODO: make damage do knock back
    public void GetDamaged(Vector2 knockBackDirection)
    {
        CanMove = false;

        KnockBackTimer.Start();

        var knockBackVelocity = 600f * knockBackDirection;
        Velocity = knockBackVelocity;
    }

    public void OnAttackShapeEntered(Node2D enemy)
    {
        GD.Print(enemy.Name);
    }


   public override void _PhysicsProcess(double delta)
    {
        if(!CanMove)
            if(IsOnFloor() && KnockBackTimer.IsStopped()){
                CanMove = true;
                Velocity = new Vector2(0,0);
            }
    }

}
