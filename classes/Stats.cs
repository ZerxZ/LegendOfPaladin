using System;
using Godot;

namespace 勇者传说.classes;

[GlobalClass]
public partial class Stats : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int health);
    [Export] public int MaxHealth = 3;
    private         int _health;
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
            EmitSignal(SignalName.HealthChanged, _health);
        }
    }
    public override void _Ready()
    {
        Health = MaxHealth;

    }
}