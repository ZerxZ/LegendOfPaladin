using Godot;

namespace 勇者传说.classes;

[GlobalClass]
public partial class Teleporter : Interactable
{
    [Export(PropertyHint.File, "*.tscn")] public string ScenePath { get; set; }
    [Export]                              public string Target    { get; set; }
    public override void Interact()
    {
        base.Interact();
        if (string.IsNullOrEmpty(ScenePath))
        {
            GD.PrintErr("[Teleport] Scene is null!");
            return;
        }
        globals.Game.Instance.ChangeScene(ScenePath, new Godot.Collections.Dictionary<string,Variant>
        {
            { "entry_point", Target }
        });

    }
}