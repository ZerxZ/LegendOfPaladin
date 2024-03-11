using Godot;
using 勇者传说.Assets.generic_char.player;

namespace 勇者传说.classes;
[GlobalClass]
public partial class SaveStone : Interactable
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    public override void _Ready()
    {
      base._Ready();
      BodyExited += OnBodyExited;
    }
    private void OnBodyExited(Node2D body)
    {
        if (body is not Player player) return;
       // if (AnimationPlayer.IsPlaying() && AnimationPlayer.CurrentAnimation == "activated")
       // {
       //     AnimationPlayer.Play("ready");
       // }
    }
    public override void Interact()
    {
        base.Interact();
        AnimationPlayer.Play("activated");
        globals.Game.Instance.SaveGame();
    }
}