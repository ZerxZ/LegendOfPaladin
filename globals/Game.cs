using System.IO;
using Godot;
using Godot.Collections;
using 勇者传说.Assets.generic_char.player;
using 勇者传说.classes;

namespace 勇者传说.globals;

public partial class Game : Node
{
    public const    string                      SavePath  = "user://data.sav";
    public          Dictionary<string, Variant> WorldData = new Dictionary<string, Variant>();
    public static   Game                        Instance { get; private set; }
    public static   string                      ScenePath;
    public static   string                      EntryPoint;
    [Export] public Stats                       PlayerStats;
    [Export] public ColorRect                   Fade;
    public          Dictionary<string, Variant> PlayerDefaultStatsData;
    public override void _UnhandledInput(InputEvent @event)
    {
        using var @input = @event;
        if (@input.IsActionPressed("ui_cancel"))
        {
            SaveGame();
        }
        if (@input.IsActionPressed("ui_focus_next"))
        {
            LoadGame();
        }
    }
    public override void _Ready()
    {
        Instance = this;
        Fade.Color = Fade.Color with
        {
            A = 0
        };
        var tree = GetTree();
        tree.TreeChanged += OnTreeChanged;
        PlayerDefaultStatsData = PlayerStats.ToDictionary();
    }
    private void OnTreeChanged()
    {
        var tree = GetTree();
        if (tree.CurrentScene is not World world) return;
        var player = world.Player;
        player.Stats = PlayerStats;
        if (player.StatusPanel.Stats is not null) return;
        player.StatusPanel.Stats = PlayerStats;
        player.StatusPanel._Ready();
    }
    public async void ChangeScene(string scenePath, Dictionary<string, Variant> param)
    {
        PlayerStats.Clear();
        ScenePath = scenePath;
        EntryPoint = param.TryGetValue("entry_point", out var entryPoint) ? (string)entryPoint : null;
        var tree         = GetTree();
        var oldSceneName = Path.GetFileNameWithoutExtension(tree.CurrentScene.SceneFilePath);
        if (tree.CurrentScene is World currentWorld)
        {
            if (oldSceneName != null) WorldData[oldSceneName] = currentWorld.ToDictionary();
        }
        if (param.TryGetValue("init", out var init))
        {
            var initCallable = init.AsCallable();
            initCallable.Call();
        }
        tree.Paused = true;
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(Fade, "color:a", 1, 0.5f);
        await ToSignal(tween, Tween.SignalName.Finished);
        GD.Print($"[Teleport] {Name} => {scenePath}");
        var error = tree.ChangeSceneToFile(scenePath);

        if (error == Error.Ok)
        {
            GD.Print($"[Teleport] {Name} => {scenePath}");
        }
        else
        {
            GD.PrintErr($"[Teleport] {Name} => {scenePath} => {error}");
        }
        await ToSignal(tree, SceneTree.SignalName.TreeChanged);
        var newSceneName = Path.GetFileNameWithoutExtension(scenePath);
        if (tree.CurrentScene is World newWorld)
        {
            if (newSceneName != null && WorldData.TryGetValue(newSceneName, out var value))
            {
                newWorld.FromDictionary((Dictionary<string, Variant>)value);

            }
        }
        GD.Print($"[Teleport] {ScenePath} => {EntryPoint}");
        var entryPoints = tree.GetNodesInGroup("entry_points");
        GD.Print($"[Teleport] Node: {entryPoints.Count}");
        if (!string.IsNullOrEmpty(EntryPoint))
        {
            foreach (var node in entryPoints)
            {
                GD.Print($"[Teleport] Node: {node.Name}");
                if (node is not EntryPoint entryPointNode || entryPointNode.Name != EntryPoint) continue;
                GD.Print($"[Teleport] W {tree.CurrentScene.Name} => N {node.Name}");
                if (tree.CurrentScene is World world)
                {
                    GD.Print($"[Teleport] W {world.Name} => N {node.Name}");
                    world.UpdatePlayer(entryPointNode.GlobalPosition, entryPointNode.Direction);
                }
                break;
            }
        }
        if (param.TryGetValue("position", out var position))
        {
            var pos = position.AsVector2();
            if (tree.CurrentScene is World world)
            {
                world.UpdatePlayer(pos, param.TryGetValue("direction", out var direction) ? (Direction)(int)direction : (Direction)0);
            }
        }
        tree.Paused = false;
        tween = CreateTween();
        tween.TweenProperty(Fade, "color:a", 0, 0.5f);
        ScenePath = null;
        EntryPoint = null;
    }
    public void SaveGame()
    {
        var scene = GetTree().CurrentScene as World;
        if (scene is null) return;
        var sceneName = Path.GetFileNameWithoutExtension(scene.SceneFilePath)!;
        WorldData[sceneName] = scene.ToDictionary();
        var (x, y) = scene.Player.GlobalPosition;
        var player = new Dictionary<string, Variant>()
        {
            {
                "position", new Dictionary<string, Variant>()
                {
                    { "x", x },
                    { "y", y },
                }
            },
            { "direction", (int)scene.Player.Direction }
        };
        var data = new Dictionary<string, Variant>()
        {
            { "world_data", WorldData },
            { "player_stats", PlayerStats.ToDictionary() },
            { "scene_path", scene.SceneFilePath },
            { "player", player },
        };

        var       json = Json.Stringify(data);
        using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
        if (file is null)
        {
            return;
        }
        file.StoreString(json);
        GD.Print("[Game Save] Saved!");
        GD.Print($"[Game Save] {json}");
    }
    public void LoadGame()
    {
        using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return;
        }
        var json = file.GetAsText();
        GD.Print($"[Game Load] {json}");
        var data        = Json.ParseString(json).AsGodotDictionary();
        var playerStats = data.TryGetValue("player_stats", out var stats) ? (Dictionary<string, Variant>)stats : new Dictionary<string, Variant>();
        PlayerStats.FromDictionary(playerStats);
        var sceneName = data.TryGetValue("scene_path", out var scenePath) ? scenePath.AsString() : null;
        if (sceneName is null) return;
        var player         = data.TryGetValue("player", out var playerData) ? (Dictionary<string, Variant>)playerData : new Dictionary<string, Variant>();
        var position       = player.TryGetValue("position", out var pos) ? (Dictionary<string, Variant>)pos : new Dictionary<string, Variant>();
        var x              = position.TryGetValue("x", out var xValue) ? (float)xValue : 0;
        var y              = position.TryGetValue("y", out var yValue) ? (float)yValue : 0;
        var playerPosition = new Vector2(x, y);
        var direction      = player.TryGetValue("direction", out var dir) ? (int)dir : 0;
        ChangeScene(sceneName, new Dictionary<string, Variant>()
        {
            { "direction", direction },
            { "position", playerPosition },
            {
                "init", Callable.From(() =>
                {
                    WorldData = data.TryGetValue("world_data",     out var variant) ? (Dictionary<string, Variant>)variant : new Dictionary<string, Variant>();
                    playerStats = data.TryGetValue("player_stats", out stats) ? (Dictionary<string, Variant>)stats : new Dictionary<string, Variant>();
                    PlayerStats.FromDictionary(playerStats);
                })
            }
        });
        WorldData = data.TryGetValue("world_data", out var worldData) ? (Dictionary<string, Variant>)worldData : new Dictionary<string, Variant>();


    }
    public void NewGame()
    {
        ChangeScene("res://world/world.tscn", new Dictionary<string, Variant>()
        {
            {
                "init", Callable.From(() =>
                {
                    WorldData = new Dictionary<string, Variant>();
                    PlayerStats.FromDictionary(PlayerStats.ToDictionary());
                })
            }
        });
    }
}