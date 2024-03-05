using Godot;

namespace 勇者传说.classes;

[GlobalClass]
public partial class Hurtbox : Area2D
{
    [Signal] public delegate void HurtEnteredEventHandler(Hitbox hitbox);
    // public Hurtbox()
    // {
    //     AreaEntered += OnAreaEntered;
    //     HurtEntered += hitbox => GD.Print($"[Hurt] {Owner.Name} => {hitbox.Owner.Name}");
    // }
    // public void OnAreaEntered(Area2D area)
    // {
    //     if (area is not Hitbox hitbox) return;
    //     //GD.Print($"[Hurt] {Owner.Name} => {area.Owner.Name}");
    //
    //    
    //     EmitSignal(SignalName.HurtEntered, hitbox);
    //     hitbox.EmitSignal(Hitbox.SignalName.HitEntered, this);
    //
    // }
}