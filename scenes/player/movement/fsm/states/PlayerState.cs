using Godot;
public  partial class PlayerState: Node{
    //Used to hold methods for common actions

    public bool CanClimb = false;
    public bool CanDash = true;

    [Signal]
    public delegate void TransitionedEventHandler(PlayerState stateCalling, string newStateName);

    [Export]
    public Player Player ;

    public bool CanTransitionToState = true;

    public virtual void Exit()
    {
        return;
    }
    public virtual void Enter()
    {
        return;
    }

    public void HandleMovement()
    {
            if (Input.IsActionPressed("move_right"))
            {
                // Direction = 1;
                Player.LastPositiveDirectionUsed = 1;
                Player.Velocity = new Vector2(Player.Speed, Player.Velocity.Y);
                // AnimatedSprite.FlipH = false;
            }
            else if (Input.IsActionPressed("move_left"))
            {
                // Direction = -1;
                Player.LastPositiveDirectionUsed = -1;
                Player.Velocity = new Vector2(-Player.Speed, Player.Velocity.Y);
                // AnimatedSprite.FlipH = true;
            }
            else
                Player.Velocity = new Vector2(0, Player.Velocity.Y); //TODO: Involve direction change speed in this
        }

    public void HandleGravity(double delta)
    {
        var newVelocity = Player.Velocity;
        newVelocity  += Player.GetGravity() * (float)delta;
        newVelocity.Y = Mathf.Clamp(newVelocity.Y, -400f, 400f); //Handle Gravity in States


        Player.Velocity = newVelocity;
    }


    public float? GetSpecialCollisionAngle()
    {
        for (int i = 0; i < Player.GetSlideCollisionCount(); i++)
        {
            var collision          = Player.GetSlideCollision(i);
            var collidedObject     = collision.GetCollider(); //Get the object Player collided with

            if(collidedObject is not TileMapLayer tilemap)
                return null;

            var collisionPosGlobal = collision.GetPosition() - collision.GetNormal() * 3.0f; //Check slightly closer to cell center to avoid duds
            var collisionPosLocal  = tilemap.ToLocal(collisionPosGlobal);
            var cell               = tilemap.LocalToMap(collisionPosLocal);
            var tileData           = tilemap.GetCellTileData(cell);

            if(tileData == null)
                return null;

            if(tileData.GetCustomData("isClimbable").AsBool())
                return Mathf.RadToDeg(collision.GetAngle());
        }

        return null;

    }

    public virtual void Update(double delta)
    {
        if(Player == null)
        {
            GD.PrintErr("Player is null!");
            return;
        }

        Player.MoveAndSlide();
    }



}
