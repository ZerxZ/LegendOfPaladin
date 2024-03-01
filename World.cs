using Godot;
using System;

public partial class World : Node2D
{
	public TileMap  TileMap;
	public Camera2D Camera2D;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TileMap = GetNode<TileMap>("TileMap");
		Camera2D = GetNode<Camera2D>("Player/Camera2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var used = TileMap.GetUsedRect().Grow(-1);
		var tileSize = TileMap.TileSet.TileSize;
		Camera2D.LimitTop = used.Position.Y * tileSize.Y;
		Camera2D.LimitRight = used.End.X * tileSize.X;
		Camera2D.LimitBottom = used.End.Y * tileSize.Y;
		Camera2D.LimitLeft = used.Position.X * tileSize.X;
		Camera2D.ResetSmoothing();
	}
}
