using Godot;
using System;

public partial class GameCamera : Camera2D
{
    private Rect2 _levelTilemapBounds;
    private CharacterBody2d _player;

    // private Vector2 _screenSize;

    private Vector2 _targetPos;

    private float _horizontalMargin = 30f;
    [Export]
    private float _defaultHorizontalMargin = 30f;
    private float  _verticalMargin = 30f;
    [Export]
    private float _defaultVerticalMargin = 30f;

    [Export]
    private float _lookAheadHorizontalDistance = 200f;
    [Export]
    private float _lookAheadVerticalDistance = 200f;
    private float _currHorizontalLookOffset        = 0f;
    [Export]
    private float _dynamicLookAheadSpeed = 50f; //How fast the lookahead distance changes based on time moving
    [Export]
    private float _defaultFollowSpeed = 300f;
    private float _followSpeed;
    [Export]
    private float _maxDistance = 300f; //Distance at which camera reaches max follow speedg


    private Vector2 _tileMapStart;
    private Vector2 _tileMapEnd;

    private Vector2 _currOffset;

    private bool _isMoving = true;


    //TODO: Adjust to better work on new level handling

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        //TODO:Maybe screen size is irrelevant to dead zone size
        // _screenSize = GetViewport().GetVisibleRect().Size;
        _targetPos   = Position;
        _followSpeed = _defaultFollowSpeed;

        _horizontalMargin = _defaultHorizontalMargin;
        _verticalMargin   = _defaultVerticalMargin;

        _player = GetNode<CharacterBody2d>("../Player"); //Handle getting tyhe player better

    }

    public void UpdateTilemapBounds(BaseLevel currLevel)
    {
        var _tilemap = currLevel.GetNode<TileMapLayer>("BackGroundTileMap");

        var _tile_size = _tilemap.TileSet.TileSize;
        _levelTilemapBounds = _tilemap.GetUsedRect();

        _tileMapStart.X   = (int)_levelTilemapBounds.Position.X * _tile_size.X;
        _tileMapStart.Y   = (int)_levelTilemapBounds.Position.Y * _tile_size.Y;
        _tileMapEnd.X     = (int)_levelTilemapBounds.End.X      * _tile_size.X;
        _tileMapEnd.Y     = (int)_levelTilemapBounds.End.Y      * _tile_size.Y;

        LimitLeft   = (int)_tileMapStart.X;
        LimitRight  = (int)_tileMapEnd  .X;
        LimitTop    = (int)_tileMapStart.Y;
        LimitBottom = (int)_tileMapEnd  .Y;
    }
    public override void _PhysicsProcess(double delta){
        if(_player != null){
            var playerGlobalPos = _player.GlobalPosition;


            var lookAheadHorizontalOffset = _player.IsClimbing? 0: _player.Direction *_lookAheadHorizontalDistance;
            var lookAheadVerticalOffset   = _player.IsFalling? _lookAheadVerticalDistance: 0; //TODO: Make a separate lookahead vertical variable
            _currHorizontalLookOffset = Mathf.MoveToward(_currHorizontalLookOffset, lookAheadHorizontalOffset, _dynamicLookAheadSpeed * (float) delta);


            if(Mathf.Abs(playerGlobalPos.X - Position.X) > _horizontalMargin && !_isMoving){
                _isMoving = true;
            }

            if(Mathf.Abs(playerGlobalPos.Y - Position.Y) > _verticalMargin && !_isMoving){
                _isMoving = true;
            }


            if((_player.Velocity.X == 0 || _player.IsClimbing) && _isMoving )
            {
                _isMoving = false;
                _targetPos.Y = playerGlobalPos.Y;
                _targetPos.X = playerGlobalPos.X;
            }

            if (_isMoving)
            {
                _targetPos.Y = playerGlobalPos.Y + lookAheadVerticalOffset;
                _targetPos.X = playerGlobalPos.X + _currHorizontalLookOffset;
            }

            var distance    = Position.DistanceTo(_targetPos);

            _followSpeed = Mathf.Lerp(
                _defaultFollowSpeed,
                _defaultFollowSpeed * 2.5f,
                Mathf.Clamp(distance/_maxDistance, 0f, 1f)
            );

            Position = Position.MoveToward(_targetPos, _followSpeed * (float)delta);
        }


    }
}
