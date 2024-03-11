using System.IO;
using Godot;
using Godot.Collections;
using 勇者传说.Assets.generic_char.player;
using 勇者传说.classes;

namespace 勇者传说.globals;

public partial class Game : Node
{
    public const    string                         SavePath  = "user://data.sav";
    public          Dictionary<string, Dictionary> WorldData = new Dictionary<string, Dictionary>();
    public static   Game                           Instance { get; private set; }
    public static   string                         ScenePath;
    public static   string                         EntryPoint;
    [Export] public Stats                          PlayerStats;
    [Export] public ColorRect                      Fade;
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
    public async void ChangeScene(string scenePath, Dictionary param)
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
                newWorld.FromDictionary(value);

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
        var data = new Dictionary();
        data.Add("world_data",   WorldData);
        data.Add("player_stats", PlayerStats.ToDictionary());
        data.Add("scene_path",   scene.SceneFilePath);
        var player   = new Dictionary();
        var position = new Dictionary();
        var (x, y) = scene.Player.GlobalPosition;
        position.Add("x", x);
        position.Add("y", y);
        player.Add("position",  position);
        player.Add("direction", (int)scene.Player.Direction);
        data.Add("player", player);
        var json = Json.Stringify(data);
        var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
        if (file is null)
        {
            return;
        }
        file.StoreString(json);
        GD.Print("[Game] Saved!");
        GD.Print($"[Game] {json}");
    }
    public void LoadGame()
    {
        var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return;
        }
        var json        = file.GetAsText();
        var data        = Json.ParseString(json).AsGodotDictionary();
        var playerStats = data.TryGetValue("player_stats", out var stats) ? (Dictionary)stats : new Dictionary();
        PlayerStats.FromDictionary(playerStats);
        var sceneName = data.TryGetValue("scene_path", out var scenePath) ? scenePath.AsString() : null;
        if (sceneName is null) return;
        var player         = data.TryGetValue("player", out var playerData) ? (Dictionary)playerData : new Dictionary();
        var position       = player.TryGetValue("position", out var pos) ? (Dictionary)pos : new Dictionary();
        var x              = position.TryGetValue("x", out var xValue) ? (float)xValue : 0;
        var y              = position.TryGetValue("y", out var yValue) ? (float)yValue : 0;
        var playerPosition = new Vector2(x, y);
        var direction      = player.TryGetValue("direction", out var dir) ? (int)dir : 0;
        ChangeScene(sceneName, new Dictionary()
        {
            { "direction", direction },
            { "position", playerPosition },
            {
                "init", Callable.From(() =>
                {
                    WorldData = data.TryGetValue("world_data",     out var variant) ? (Dictionary<string, Dictionary>)variant : new Dictionary<string, Dictionary>();
                    playerStats = data.TryGetValue("player_stats", out stats) ? (Dictionary)stats : new Dictionary();
                    PlayerStats.FromDictionary(playerStats);
                })
            }
        });
        WorldData = data.TryGetValue("world_data", out var worldData) ? (Dictionary<string, Dictionary>)worldData : new Dictionary<string, Dictionary>();


    }
}