using Godot;
using 勇者传说.classes;
using Range = Godot.Range;

namespace 勇者传说.ui;

public partial class StatusPanel : HBoxContainer
{
    [Export] public TextureProgressBar HealthBar;
    [Export] public TextureProgressBar EasedHealthBar;
    [Export] public TextureProgressBar EnergyBar;
    [Export] public TextureProgressBar EasedEnergyBar;
    [Export] public Stats              Stats;
    [Export] public Tween              EnergyTween;
    [Export] public Tween              HealthTween;

    // Called when the node enters the scene tree for the first time.
    public override  void _Ready()
    {
        if (Stats is null)
        {
            return;
        }
        Stats.RegisterHealthChanged(OnHealthChanged);
        Stats.RegisterEnergyChanged(OnEnergyChanged);

        UpdateHealth(true);
        UpdateEnergy(true);
    }
    private void OnEnergyChanged()
    {

        UpdateEnergy();
    }
    private void OnHealthChanged()
    {

        UpdateHealth();
    }
    public void UpdateEnergy(bool skip = false)
    {
        if (Stats is null)
        {
            return;
        }
        var percent = Stats.Energy / Stats.MaxEnergy;
        if (!skip)
        {

            EnergyTween = CreateTween();
            EnergyTween.TweenProperty(EasedEnergyBar, Range.PropertyName.Value.ToString(), percent, 0.3f);
        }
        else
        {
            EasedEnergyBar.Value = percent;
        }
        EnergyBar.Value = percent;
    }

    public void UpdateHealth(bool skip = false)
    {
        if (Stats is null)
        {
            return;
        }
        var percent = (float)Stats.Health / Stats.MaxHealth;
        if (!skip)
        {
           
            HealthTween = CreateTween();
            HealthTween.TweenProperty(EasedHealthBar, Range.PropertyName.Value.ToString(), percent, 0.3f);
        }
        else
        {
            EasedHealthBar.Value = percent;
        }
        HealthBar.Value = percent;
    }
}