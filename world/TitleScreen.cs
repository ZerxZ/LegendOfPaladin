using Godot;
using System;
using 勇者传说.globals;

public partial class TitleScreen : Control
{
    [Export] public Button        NewGameButton;
    [Export] public Button        LoadGameButton;
    [Export] public Button        ExitGameButton;
    [Export] public VBoxContainer Menu;
    [Export] public AudioStream  Bgm;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        NewGameButton.GrabFocus();
        NewGameButton.Pressed += OnNewGameButtonPressed;
        LoadGameButton.Pressed += OnLoadGameButtonPressed;
        ExitGameButton.Pressed += OnExitGameButtonPressed;
        GD.Print($"[TitleScreen] HasSave: {Game.HasSave}");
        LoadGameButton.Visible = Game.HasSave;
        foreach (var node in Menu.GetChildren())
        {
            if (node is not Button button)
            {
                continue;
            }
            button.MouseEntered += button.GrabFocus;
        }
        SoundManager.Instance.SetupUiSounds(this);
        SoundManager.Instance.PlayBgm(Bgm);
    }
    private void OnExitGameButtonPressed()
    {
        GetTree().Quit();
    }
    private void OnLoadGameButtonPressed()
    {
        Game.Instance.LoadGame();
    }
    private void OnNewGameButtonPressed()
    {
        Game.Instance.NewGame();
    }

}