using Godot;
using 勇者传说.Assets.generic_char.player;

namespace 勇者传说;

public partial class World : Node2D
{
    [Export] public TileMap  TileMap;
    [Export] public Camera2D Camera2D;
    [Export] public Player   Player;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var used     = TileMap.GetUsedRect().Grow(-1);
        var tileSize = TileMap.TileSet.TileSize;
        Camera2D.LimitTop = used.Position.Y * tileSize.Y;
        Camera2D.LimitRight = used.End.X * tileSize.X;
        Camera2D.LimitBottom = used.End.Y * tileSize.Y;
        Camera2D.LimitLeft = used.Position.X * tileSize.X;
        Camera2D.ResetSmoothing();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public  void UpdatePlayer(EntryPoint entryPoint)
    {
        Player.GlobalPosition = entryPoint.GlobalPosition;
        Player.Direction = entryPoint.Direction;
        Camera2D.ResetSmoothing();
    }
}