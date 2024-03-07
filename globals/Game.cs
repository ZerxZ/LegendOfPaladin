using Godot;
using System;
using 勇者传说;
using 勇者传说.classes;

public partial class Game : Node
{
    public static Game   Instance { get; private set; }
    public static string ScenePath;
    public static string EntryPoint;
    [Export] public Stats PlayerStats;
    public override void _Ready()
    {
        Instance = this;
        var tree = GetTree();
        tree.TreeChanged += OnTreeChanged;
    }
    private async void OnTreeChanged()
    {
        var tree = GetTree();
        if (tree.CurrentScene is World world)
        {
            var player = world.Player;
            player.Stats = PlayerStats;
            if ( player.StatusPanel.Stats is null)
            {
                player.StatusPanel.Stats = PlayerStats;
                player.StatusPanel._Ready();
            }
            
        }
    }
    public  async void ChangeScene(string scenePath, string entryPoint)
    {
        PlayerStats.Clear();
        ScenePath  = scenePath;
        EntryPoint = entryPoint;
        var tree  = GetTree();
        
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
        
        GD.Print($"[Teleport] {ScenePath} => {EntryPoint}");
        var entryPoints = tree.GetNodesInGroup("entry_points");
        GD.Print($"[Teleport] Node: {entryPoints.Count}");
        foreach (var node in entryPoints)
        {
            GD.Print($"[Teleport] Node: {node.Name}");
            if (node is not EntryPoint entryPointNode || entryPointNode.Name != EntryPoint) continue;
            GD.Print($"[Teleport] W {tree.CurrentScene .Name} => N {node.Name}");
            if (tree.CurrentScene is World world)
            {
                GD.Print($"[Teleport] W {world.Name} => N {node.Name}");
                world.UpdatePlayer(entryPointNode);
            }
            break;
        }
        ScenePath = null;
        EntryPoint = null;
    }
}