using Godot;
public  partial class PlayerState: Node{
    //Used to hold methods for common actions

    public bool CanClimb = false;
    public bool CanDash = true;

    [Signal]
    public delegate void TransitionedEventHandler(PlayerState stateCalling, string newStateName);

    [Export]
    public Player Player ;

    public string LowerCaseName;

    public bool CanTransitionToState = true;

    public virtual void Exit()
    {
        return;
    }
    public virtual void Enter()
    {
        return;
    }

    public override void _Ready()
    {
        LowerCaseName = Name.ToString().ToLower();
    }

    public void HandleMovement()
    {
        if (Player.CanMove)
        {
            if (Input.IsActionPressed("move_right"))
            {
                Player.LastPositiveDirectionUsed = 1;
                Player.Velocity = new Vector2(Player.Speed, Player.Velocity.Y);
            }
            else if (Input.IsActionPressed("move_left"))
            {
                Player.LastPositiveDirectionUsed = -1;
                Player.Velocity = new Vector2(-Player.Speed, Player.Velocity.Y);
            }
            else
               Player.Velocity = new Vector2(0, Player.Velocity.Y); //TODO: Involve direction change speed in this
        }
    }

    public void HandleGravity(double delta)
    {
        var newVelocity = Player.Velocity;
        newVelocity  += Player.GetGravity() * (float)delta;
        newVelocity.Y = Mathf.Clamp(newVelocity.Y, -400f, 400f); //Handle Gravity in States

        Player.Velocity = newVelocity;
    }


    public virtual void Update(double delta)
    {
        //If the player is immobilezed, change state to idle TODO: Decide if a new state is needed for this
        if(!Player.CanMove)
            EmitSignal(SignalName.Transitioned, this, "idle");

        Player.MoveAndSlide();
    }



}
