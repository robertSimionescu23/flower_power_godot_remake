using Godot;
using System;

public partial class Main : Node2D
{
    [Export]
    public PackedScene startLevel;
    public BaseLevel   CurrLevelInstance;
    public GameCamera ActiveCamera;
    public Player Player;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ActiveCamera = GetNode<GameCamera>("Camera2D");
        Player = GetNode<Player>("Player");

        ChangeCurrentLevel(startLevel);
    }

    private void ChangeCurrentLevel(PackedScene currLevel)
    {
        CurrLevelInstance?.QueueFree();

        CurrLevelInstance = currLevel.Instantiate<BaseLevel>();
        //Connect the current level to the onLevelFish signal that is declared in it's parent
        CurrLevelInstance.LevelChanged += OnLevelFinish;

        ActiveCamera.AddSibling         (CurrLevelInstance);
        ActiveCamera.UpdateTilemapBounds(CurrLevelInstance);
        ActiveCamera.GlobalPosition   = CurrLevelInstance.StartLocation;

        Player.GlobalPosition = CurrLevelInstance.StartLocation;
        Player.Visible = true;
    }

    public void OnLevelFinish(PackedScene level)
    {
        GD.Print("Level changes");
        Player.Visible = false;

        CallDeferred(nameof(ChangeCurrentLevel), level);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
