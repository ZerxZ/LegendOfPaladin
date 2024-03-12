using Godot;
using Godot.Collections;
using 勇者传说.Assets.generic_char.player;
using 勇者传说.classes;
using 勇者传说.enemies;
using 勇者传说.globals;
using Direction = 勇者传说.Assets.generic_char.player.Direction;

namespace 勇者传说;
[GlobalClass]
public partial class Cave : World
{
    [Export] public Enemy Boss;
    public override void _Ready()
    {
        base._Ready();
        Boss.Died += OnBossDied;
    }
    private async void OnBossDied()
    {
      var timer=  GetTree().CreateTimer(1);
      await ToSignal(timer, Timer.SignalName.Timeout);
      Game.Instance.ChangeScene("res://ui/game_finish_screen.tscn",new Dictionary<string, Variant>());
    }
}