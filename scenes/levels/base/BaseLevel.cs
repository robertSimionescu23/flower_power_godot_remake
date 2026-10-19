// meta-name: Base Level
// meta-description: A level template containing the basics needed

using Godot;
using System;

public partial class BaseLevel: Node2D
{
    public Vector2 StartLocation = new (1,1);
    public Vector2 FinsihLocation;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        StartLocation = GetNode<Area2D>("Start").GlobalPosition;
        FinsihLocation   = GetNode<Area2D>("Finish").GlobalPosition;

    }
}
