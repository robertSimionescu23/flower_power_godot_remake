using Godot;


public partial class Player : CharacterBody2D
{
    //TODO: clean up some attributes here
    private AnimatedSprite2D AnimatedSprite;

    [Export]
    public float Speed = 300.0f;
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

    private bool _canClimb = false;
    private bool _upDash   = false;


    public override void _Ready(){
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AnimatedSprite.Play();

        // _wallClingBufferTimer           = GetNode<Timer>("Timers/WallClingBufferTimer");

        AttackZone = GetNode<Area2D>("AttackShape");
        GroundPoundRay = GetNode<RayCast2D>("RayCast2D");

        MovementFSM = GetNode<PlayerFsm>("PlayerFsm");
    }

}
