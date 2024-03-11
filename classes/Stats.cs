using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace 勇者传说.classes;

[GlobalClass]
public partial class Stats : Node, IDataSave
{
    [Signal] public delegate void HealthChangedEventHandler();

    [Signal] public delegate void EnergyChangedEventHandler();

    [Export] public int                             MaxHealth   = 3;
    [Export] public double                          MaxEnergy   = 10;
    [Export] public double                          EnergyRegen = 0.8;
    private         int                             _health;
    private         double                          _energy;
    public          List<HealthChangedEventHandler> HealthChangedList = new List<HealthChangedEventHandler>();
    public          List<EnergyChangedEventHandler> EnergyChangedList = new List<EnergyChangedEventHandler>();
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
    public void RegisterHealthChanged(HealthChangedEventHandler handler)
    {
        if (!HealthChangedList.Contains(handler))
        {
            HealthChangedList.Add(handler);
        }

        HealthChanged += handler;
    }
    public void RegisterEnergyChanged(EnergyChangedEventHandler handler)
    {
        if (!EnergyChangedList.Contains(handler))
        {
            EnergyChangedList.Add(handler);
        }
        EnergyChanged += handler;
    }
    public void Clear()
    {
        GD.Print("Clearing Stats...");
        foreach (var healthChangedEventHandler in HealthChangedList)
        {
            HealthChanged -= healthChangedEventHandler;
        }
        foreach (var energyChangedEventHandler in EnergyChangedList)
        {
            EnergyChanged -= energyChangedEventHandler;
        }
    }
    public Godot.Collections.Dictionary<string, Variant> ToDictionary()
    {

        return new Godot.Collections.Dictionary<string, Variant>()
        {
            { "health", Health },
            { "max_energy", MaxEnergy },
            { "max_health", MaxHealth }
        };
    }
    public void FromDictionary(Godot.Collections.Dictionary<string, Variant> dictionary)
    {
        MaxEnergy = dictionary.TryGetValue("max_energy", out var maxEnergy) ? (double)maxEnergy : MaxEnergy;
        MaxHealth = dictionary.TryGetValue("max_health", out var maxHealth) ? (int)maxHealth : MaxHealth;
        Health = dictionary.TryGetValue("health",        out var health) ? (int)health : MaxHealth;
    }
}