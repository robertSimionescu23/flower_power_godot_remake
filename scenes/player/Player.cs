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
    public bool _canMove = true;

    public bool IsFreeFalling;
    private bool _canDash = true;

    public int LastPositiveDirectionUsed = 1;

    public Area2D AttackZone;

    public RayCast2D GroundPoundRay;

    public PlayerState State;


    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        AttackZone = GetNode<Area2D>("AttackShape");
        GroundPoundRay = GetNode<RayCast2D>("RayCast2D");

        MovementFSM = GetNode<PlayerFsm>("PlayerFsm");
    }

    //TODO: make damage do knock back
    public void GetDamaged()
    {
        GD.Print("Ouch");
    }

    public void OnAttackShapeEntered(Node2D enemy)
    {
        GD.Print(enemy.Name);
    }

}
