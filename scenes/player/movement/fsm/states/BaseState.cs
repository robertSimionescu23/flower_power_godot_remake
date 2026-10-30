using Godot;

[GlobalClass]
public partial class BaseState : Node
{
    [Signal]
    public delegate void TransitionedEventHandler(BaseState stateCalling, string newStateName);

    [Export]
    public Player player ;

    public bool TransitionAllowed = true;

    public virtual void Exit()
    {
        return;
    }
    public virtual void Enter()
    {
        return;
    }

    public virtual void Update(double delta)
    {
        return;
    }

}
