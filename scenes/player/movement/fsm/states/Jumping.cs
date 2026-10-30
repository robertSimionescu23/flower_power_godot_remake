using Godot;

[GlobalClass]
public partial class Jumping : PlayerState
{

    public override void Enter()
    {
        Jump();
    }

    public void Jump()
    {
        player.Velocity = new Vector2 (player.Velocity.X, player.JumpVelocity);
    }
    public override void Update(double delta)
    {

        if(Input.IsActionJustPressed("dash"))
             EmitSignal(BaseState.SignalName.Transitioned, this, "dashing");

        if (Input.IsActionJustReleased("jump") && player.Velocity.Y < 0){
            player.Velocity = new Vector2 (player.Velocity.X, player.Velocity.Y / 5);
        }

        else if(player.Velocity.Y > 0){
            EmitSignal(BaseState.SignalName.Transitioned, this, "falling");
        }


        HandleMovement();
        HandleGravity(delta);

    }
}
