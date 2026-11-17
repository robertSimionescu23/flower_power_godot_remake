using Godot;

public partial class Enemy: CharacterBody2D
{
    [Export]
    public float Speed = 50.0f;
    public int Direction = -1;
    public bool IsAlive = true;


    public Player Player;

    public virtual void GetHit(){}

    public override void _Ready()
    {
        Player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }
}
