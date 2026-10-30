using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class PlayerFsm : Node
{
    private Dictionary<string, BaseState> States = new Dictionary<string, BaseState>();
    [Export]
    private BaseState CurrState {set; get;}
    [Export]
    public AnimatedSprite2D PlayerAnimation;

    [Export]
    private BaseState InitialState;
    private Player player;

    public override void _Ready()
    {
        CurrState = InitialState;
        player = (Player)GetTree().GetNodesInGroup("Player")[0];

        foreach(BaseState child in GetChildren().OfType<BaseState>())
        {
            States.Add(child.Name.ToString().ToLower(), child);

            //connect each FSM state Transitioned signal to the handler defined here
            child.Connect(BaseState.SignalName.Transitioned,
                          Callable.From<BaseState, string>(HandleTransition)
            );

        }

    }

    public void HandleTransition(BaseState state, string newStateName)
    {

        // GD.Print("Transition attempt to " + newStateName + " while state is " + CurrState.Name);

        if (state != CurrState)
            return;

        var newState = States[newStateName.ToLower()];

        if(newState == null)
            return;

        CurrState.Exit();

        newState.Enter();

        CurrState = newState;
    }

     public void HandleAnimations()
    {
        var animation = CurrState.Name.ToString().ToLower();
        if(player.Velocity.X < 0)
            PlayerAnimation.FlipH = true;
        else if (player.Velocity.X > 0)
            PlayerAnimation.FlipH = false;

        if (CurrState != null &&
            PlayerAnimation.Animation != animation)
        {
            GD.Print("Animation now is " + animation);
            PlayerAnimation.Play(animation);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrState?.Update(delta);
        HandleAnimations();
        // GD.Print(CurrState.Name);
    }
}
