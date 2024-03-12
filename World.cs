using Godot;
using Godot.Collections;
using 勇者传说.Assets.generic_char.player;
using 勇者传说.classes;
using 勇者传说.enemies;
using Direction = 勇者传说.Assets.generic_char.player.Direction;

namespace 勇者传说;
[GlobalClass]
public partial class World : Node2D,IDataSave
{
    [Export] public TileMap  TileMap;
    [Export] public Camera2D Camera2D;
    [Export] public Player   Player;
    [Export] public AudioStream Bgm;
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
        if (Bgm is not null)
        {
            SoundManager.Instance.PlayBgm(Bgm);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void UpdatePlayer(Vector2 position,Direction direction)
    {
        Player.GlobalPosition = position;
        Player.FallFromY = position.Y;
        Player.Direction = direction;
        Camera2D.ResetSmoothing();
        Camera2D.ForceUpdateScroll();
    }
    public Dictionary<string,Variant> ToDictionary()
    {
        var enemiesAlive = new Array<string>();
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy)
            {
                var path = GetPathTo(enemy);
                enemiesAlive.Add(path.ToString());
            }
        }
        return new Dictionary<string,Variant>()
        {
            {"enemies_alive", enemiesAlive}
        };
    }
    public void FromDictionary(Dictionary<string,Variant> dictionary)
    {

        var enemiesAlive = dictionary.TryGetValue("enemies_alive", out var value) ? (Array<string>)value : new Array<string>();
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && !enemiesAlive.Contains(GetPathTo(enemy)))
            {
                enemy.QueueFree();
            }
        }
    }
}