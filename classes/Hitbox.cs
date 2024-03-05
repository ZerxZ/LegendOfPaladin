using Godot;

namespace 勇者传说.classes;

[GlobalClass]
public partial class Hitbox : Area2D
{
    [Signal] public delegate void HitEnteredEventHandler(Hurtbox hurtbox);

    // Called when the node enters the scene tree for the first time.
    public Hitbox()
    {
        AreaEntered += OnAreaEntered;
        //HitEntered += hurtbox => GD.Print($"[Hit] {Owner.Name} => {hurtbox.Owner.Name}");
    }
    public void OnAreaEntered(Area2D area)
    {
        if (area is not Hurtbox hurtbox) return;
        // GD.Print($"[Hit] {Owner.Name} => {area.Owner.Name}");
        EmitSignal(SignalName.HitEntered, hurtbox);
        hurtbox.EmitSignal(Hurtbox.SignalName.HurtEntered, this);

    }
}