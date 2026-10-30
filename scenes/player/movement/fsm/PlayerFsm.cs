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

    [Export]
    private bool _stateDebug = false;

    public override void _Ready()
    {
        CurrState = InitialState;
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        foreach(BaseState child in GetChildren().OfType<BaseState>())
        {
            States.Add(child.Name.ToString().ToLower(), child);

            child.player = player;

            //connect each FSM state Transitioned signal to the handler defined here
            child.Connect(BaseState.SignalName.Transitioned,
                          Callable.From<BaseState, string>(HandleTransition)
            );

        }

    }

    public void HandleTransition(BaseState state, string newStateName)
    {

        if(_stateDebug) GD.Print("Transition attempt: from " + state.Name.ToString().ToLower() + " to " + newStateName );

        if (state != CurrState)
        {
            if(_stateDebug) GD.Print("Transition not successfull: calling state is not current state");
            return;
        }

        var newState = States[newStateName.ToLower()];

        if(newState == null){
            if(_stateDebug) GD.Print("Transition not successfull: new state is not defined");
            return;
        }

        if(newState == state){
            if(_stateDebug) GD.Print("Transition not successfull: same state for destination and source");
            return;
        }

        if(!newState.TransitionAllowed){
            if(_stateDebug) GD.Print("Transition not successfull: transition not allowed now");
            return;
        }

        if(_stateDebug) GD.Print("Transition successfull");

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
            PlayerAnimation.Animation != animation &&
            PlayerAnimation.SpriteFrames.GetAnimationNames().Contains(animation))
        {
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
