using Godot;
using System;

public partial class Main : Node2D
{
    public PackedScene CurrLevel;
    public GameCamera ActiveCamera;
    public CharacterBody2d Player;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ActiveCamera = GetNode<GameCamera>("Camera2D");
        CurrLevel = GD.Load<PackedScene>("res://scenes/levels/level1/Level1.tscn");
        Player = GetNode<CharacterBody2d>("Player");

        //TODO: Tidy up and make level be chosen by text or otehr means
        var level = CurrLevel.Instantiate<BaseLevel>();
        ActiveCamera.AddSibling(level);

        ActiveCamera.UpdateTilemapBounds(level);

        ActiveCamera.GlobalPosition = level.StartLocation;
        Player.GlobalPosition = level.StartLocation;
        Player.Visible = true;

        GD.Print(level.StartLocation);
        GD.Print(level.FinsihLocation);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
