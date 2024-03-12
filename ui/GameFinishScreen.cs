using Godot;
using System;
using 勇者传说.globals;

public partial class GameFinishScreen : Control
{
    public          string[]    Messages     = new string[] { "恭喜你通关游戏!", "我的朋友\n你是一个真正的英雄!", "感谢你游玩!" };
    private         int         _currentLine = -1;
    public          Tween       Tween;
    [Export] public Label       Label;
    [Export] public AudioStream Bgm;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ShowLine(0);
        SoundManager.Instance.PlayBgm(Bgm);
    }
    public override void _Input(InputEvent @event)
    {
        if (Tween.IsRunning())
        {
            return;
        }
        using var input = @event;
        if (input is InputEventKey or InputEventMouseButton or InputEventJoypadButton)
        {
            if (input.IsPressed() && !input.IsEcho())
            {
                if (_currentLine < Messages.Length - 1)
                {
                    GD.Print($"[GameFinishScreen] Current line: {_currentLine} Next line: {_currentLine + 1}");
                    ShowLine(_currentLine + 1);
                }
                else
                {
                    Game.Instance.TitleMenu(3f);
                }
            }
        }
    }
    public void ShowLine(int line)
    {
        _currentLine = line;
        GD.Print($"[GameFinishScreen] Showing line: {line}");
        if (Tween is not null)
        {
            Tween.Stop();
            Tween.Kill();
        }
        Tween = CreateTween();
        Tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        if (line > 0)
        {
            Tween.TweenProperty(Label, "modulate:a", 0, 1);
        }
        else
        {
            Label.Modulate = Label.Modulate with
            {
                A = 0
            };
        }
        Tween.TweenCallback(Callable.From(SetText));
        Tween.TweenProperty(Label, "modulate:a", 1, 1);
    }
    private void SetText()
    {
        
        GD.Print("[GameFinishScreen] Setting text...");
        GD.Print($"[GameFinishScreen] Current Line: {_currentLine}");
        if (_currentLine < 0 || _currentLine > Messages.Length - 1)
        {
            return;
        }
  
        Label.Text = Messages[_currentLine];
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}