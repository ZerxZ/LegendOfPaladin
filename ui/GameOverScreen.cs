using Godot;
using System;
using 勇者传说.globals;

public partial class GameOverScreen : Control
{
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public AudioStream     Bgm;
    public override void _Input(InputEvent @event)
    {
        GetWindow().SetInputAsHandled();
        if (AnimationPlayer.IsPlaying())
        {
            return;
        }
        using var input = @event;
        if (input is InputEventKey or InputEventMouseButton or InputEventJoypadButton)
        {
            if (input.IsPressed() && !input.IsEcho())
            {
                GD.Print($"[GameOverScreen] Input: {input}");
                if (Game.HasSave)
                {
                    Game.Instance.LoadGame();
                }
                else
                {
                    Game.Instance.TitleMenu();
                }
            }
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Hide();
        SetProcessInput(false);
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public void ShowGameOver()
    {
        Show();
        AnimationPlayer.Play("enter");
        SoundManager.Instance.PlayBgm(Bgm);
        SetProcessInput(true);
    }
}