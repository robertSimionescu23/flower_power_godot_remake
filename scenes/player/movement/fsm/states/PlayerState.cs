using Godot;
public  partial class PlayerState: BaseState{
    //Used to hold methods for common actions

    public bool CanClimb = false;
    public bool CanDash = true;
    public void HandleMovement()
    {
            if (Input.IsActionPressed("move_right"))
            {
                // Direction = 1;
                player.LastPositiveDirectionUsed = 1;
                player.Velocity = new Vector2(player.Speed, player.Velocity.Y);
                // AnimatedSprite.FlipH = false;
            }
            else if (Input.IsActionPressed("move_left"))
            {
                // Direction = -1;
                player.LastPositiveDirectionUsed = -1;
                player.Velocity = new Vector2(-player.Speed, player.Velocity.Y);
                // AnimatedSprite.FlipH = true;
            }
            else
                player.Velocity = new Vector2(0, player.Velocity.Y); //TODO: Involve direction change speed in this
        }

    public void HandleGravity(double delta)
    {
        var newVelocity = player.Velocity;
        newVelocity  += player.GetGravity() * (float)delta;
        newVelocity.Y = Mathf.Clamp(newVelocity.Y, -400f, 400f); //Handle Gravity in States


        player.Velocity = newVelocity;
    }


    public float? GetSpecialCollisionAngle()
    {
        for (int i = 0; i < player.GetSlideCollisionCount(); i++)
        {
            var collision          = player.GetSlideCollision(i);
            var collidedObject     = collision.GetCollider(); //Get the object player collided with

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



    public override void Update(double delta)
    {
        if(player == null)
        {
            GD.PrintErr("Player is null!");
            return;
        }


        player.MoveAndSlide();
    }



}
