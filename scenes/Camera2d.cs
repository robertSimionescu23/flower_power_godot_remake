using Godot;
using System;

public partial class Camera2d : Camera2D
{
    private Rect2 _levelTilemapBounds;
    private CharacterBody2d _player;

    // private Vector2 _screenSize;

    private Vector2 _targetPos;

    [Export]
    private float _horizontalMargin = 30f;
    [Export]
    private float  _verticalMargin = 30f;

    [Export]
    private float _lookAheadDistance = 200f;
    [Export]
    private float _dynamicLookAheadSpeed = 50f; //How fast the lookahead distance changes abed on time moving
    private float _currLookOffset        = 0f;
    [Export]
    private float _defaultFollowSpeed = 300f;
    private float _followSpeed;
    [Export]
    private float _maxDistance = 300f;


    private Vector2 _tileMapStart;
    private Vector2 _tileMapEnd;

    private Vector2 _currOffset;

    private bool _isMoving = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var _tilemap = GetNode<TileMapLayer>("../BackGroundTileMap");
        _player = GetNode<CharacterBody2d>("../Player");

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
        //TODO:Maybe screen size is irrelevant to dead zone size
        // _screenSize = GetViewport().GetVisibleRect().Size;
        _targetPos   = Position;
        _followSpeed = _defaultFollowSpeed;

    }
    public override void _PhysicsProcess(double delta){
        var playerGlobalPos = _player.GlobalPosition;


        var lookAheadOffset = _player.Direction *_lookAheadDistance;
        _currLookOffset = Mathf.MoveToward(_currLookOffset, lookAheadOffset, _dynamicLookAheadSpeed * (float) delta);


        if(Mathf.Abs(playerGlobalPos.X - Position.X) > _horizontalMargin && !_isMoving){
            _isMoving = true;
        }

        if(Mathf.Abs(playerGlobalPos.Y - Position.Y) > _verticalMargin && !_isMoving){
            _isMoving = true;
        }

        if (_isMoving)
        {
            _targetPos.Y = playerGlobalPos.Y;
            _targetPos.X = playerGlobalPos.X + _currLookOffset;
        }

        if(_player.Velocity.X == 0 && _isMoving)
        {
            _isMoving = false;
            _targetPos.Y = playerGlobalPos.Y;
            _targetPos.X = playerGlobalPos.X;
        }

        var distance    = Position.DistanceTo(_targetPos);

        _followSpeed = Mathf.Lerp(
            _defaultFollowSpeed,
            _defaultFollowSpeed * 2,
            Mathf.Clamp(distance/_maxDistance, 0f, 1f)
        );

        Position = Position.MoveToward(_targetPos, _followSpeed * (float)delta);

    }

}
