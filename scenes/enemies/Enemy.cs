using Godot;

public partial class Enemy: CharacterBody2D
{
    [Export]
    public float Speed = 50.0f;
    public int Direction = -1;
    public bool isAlive = true;

    Player _Player;

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

}
