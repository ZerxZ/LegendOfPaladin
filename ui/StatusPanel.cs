using Godot;
using System;
using 勇者传说.classes;
using Range = Godot.Range;

public partial class StatusPanel : HBoxContainer
{
    [Export] public TextureProgressBar HealthBar;
    [Export] public TextureProgressBar EasedHealthBar;
    [Export] public Stats              Stats;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Stats.HealthChanged += OnHealthChanged;
    }
    private void OnHealthChanged(int health)
    {
        UpdateHealth();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public void UpdateHealth()
    {
        var percent = (float)Stats.Health / Stats.MaxHealth;
        CreateTween().TweenProperty(EasedHealthBar, Range.PropertyName.Value.ToString(),percent,0.3f);
        HealthBar.Value = percent;
    }
}