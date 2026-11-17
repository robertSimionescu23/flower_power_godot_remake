using Godot;

public partial class Enemy: CharacterBody2D
{
    [Export]
    public float Speed = 50.0f;
    public int Direction = -1;
    public bool IsAlive = true;


    Player _Player;

    public virtual void GetHit(){}

    public override void _Ready()
    {
        _Player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }
    public Vector2 GetDirectionToPlayer(){
        var location = _Player.GlobalPosition - GlobalPosition;
        return location.Normalized();
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    public void OnPlayerHit(Node2D body)
    {
        Player player = (Player)body;
        if(!player.AttackHitBox.Visible && IsAlive){ //TODO: player.AttackHitBox.Visible is inintuitive - change to a bool or something

            var directionToPlayer = GetDirectionToPlayer();
            Vector2 knockBackDirection = new Vector2(0.5f, -0.5f);

            if (directionToPlayer.X < 0)
                knockBackDirection.X = -0.5f;
            if (directionToPlayer.Y > 0.2)
                knockBackDirection.Y = 0.5f;

            _Player.GetDamaged(knockBackDirection);
        }
    }



}
