using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class PlayerFsm : Node
{
    private Dictionary<string, PlayerState> States = new Dictionary<string, PlayerState>();
    [Export]
    private PlayerState CurrState {set; get;}
    [Export]
    public AnimatedSprite2D PlayerAnimation;

    [Export]
    private PlayerState InitialState;
    private Player Player;

    [Export]
    private bool _stateDebug = false;

    public override void _Ready()
    {
        CurrState = InitialState;
        Player = GetTree().GetFirstNodeInGroup("Player") as Player;
        Player.State = CurrState;

        foreach(PlayerState child in GetChildren().OfType<PlayerState>())
        {
            States.Add(child.Name.ToString().ToLower(), child);

            child.Player = Player;

            //connect each FSM state Transitioned signal to the handler defined here
            child.Connect(PlayerState.SignalName.Transitioned,
                          Callable.From<PlayerState, string>(HandleTransition)
            );

        }

    }

    public void HandleTransition(PlayerState state, string newStateName)
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

        if(!newState.CanTransitionToState){
            if(_stateDebug) GD.Print("Transition not successfull: transition not allowed now");
            return;
        }

        if(_stateDebug) GD.Print("Transition successfull");

        CurrState.Exit();

        newState.Enter();

        CurrState = newState;
        Player.State = CurrState;
    }

     public void HandleAnimations()
    {
        var animation = CurrState.Name.ToString().ToLower();
        if(Player.Velocity.X < 0)
            PlayerAnimation.FlipH = true;
        else if (Player.Velocity.X > 0)
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
