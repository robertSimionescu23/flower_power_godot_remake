using Godot;
using System;

public partial class BaseLevel: Node2D
{
    public Vector2 StartLocation;
    public Vector2 FinsihLocation;
    [Export]
    public PackedScene NextLevel;
    [Export]
    public PackedScene PrevLevel;

    [Signal]
    public delegate void LevelChangedEventHandler(PackedScene NextLevel);
    public override void _Ready()
    {
        Area2D finish = GetNode<Area2D>("Finish");
        StartLocation = GetNode<Area2D>("Start").GlobalPosition;
        FinsihLocation   = finish.GlobalPosition;
        finish.BodyEntered += OnFinishBodyEntered;
    }

    public void OnFinishBodyEntered(Node2D body)
    {
        if(body is CharacterBody2D){
            GD.Print("Finished");
            EmitSignal(SignalName.LevelChanged, NextLevel);
        }
    }
}
