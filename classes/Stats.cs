using System;
using Godot;

namespace 勇者传说.classes;

[GlobalClass]
public partial class Stats : Node
{
    [Signal] public delegate void HealthChangedEventHandler();

    [Signal] public delegate void EnergyChangedEventHandler();

    [Export] public int    MaxHealth   = 3;
    [Export] public double MaxEnergy   = 10;
    [Export] public double EnergyRegen = 0.8;
    private         int    _health;
    private         double _energy;
    [Export] public int Health
    {
        get => _health;
        set
        {
            value = Math.Clamp(value, 0, MaxHealth);
            if (_health == value)
            {
                return;
            }
            _health = value;
            EmitSignal(SignalName.HealthChanged);
        }
    }
    [Export] public double Energy
    {
        get => _energy;
        set
        {
            value = Math.Clamp(value, 0, MaxEnergy);
            _energy = value;
            EmitSignal(SignalName.EnergyChanged);
        }
    }
    public override void _Process(double delta)
    {
        Energy += EnergyRegen * delta;
    }
    public override void _Ready()
    {
        Health = MaxHealth;
        Energy = MaxEnergy;

    }
}