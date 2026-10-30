using Godot;
public  partial class PlayerState: BaseState{
    //Used to hold methods for common actions
    public void HandleMovement()
    {
            if (Input.IsActionPressed("move_right"))
            {
                // Direction = 1;
                // _lastPositiveDirectionUsed = 1;
                player.Velocity = new Vector2(player.Speed, player.Velocity.Y);
                // AnimatedSprite.FlipH = false;
            }
            else if (Input.IsActionPressed("move_left"))
            {
                // Direction = -1;
                // _lastPositiveDirectionUsed = -1;
                player.Velocity = new Vector2(-player.Speed, player.Velocity.Y);
                // AnimatedSprite.FlipH = true;
            }
            else
            {
                player.Velocity = new Vector2(0, player.Velocity.Y); //TODO: Involve direction change speed in this
            }
        }

    public void HandleGravity(double delta)
    {
        var newVelocity = player.Velocity;
        newVelocity  += player.GetGravity() * (float)delta;
        newVelocity.Y = Mathf.Clamp(newVelocity.Y, -400f, 400f); //Handle Gravity in States

        player.Velocity = newVelocity;
    }
}
