using Godot;
using System;
using 勇者传说.Assets.generic_char.player;
[GlobalClass]
public partial class Interactable : Area2D
{
	[Signal] public delegate void InteractedEventHandler();
	public Interactable()
	{
		CollisionLayer = 0;
		CollisionMask = 0;
		SetCollisionMaskValue(2,true);
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}
	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.RegisterInteractable(this);
	}
	private void OnBodyExited(Node2D body)
	{
		if (body is not Player player) return;
		player.UnregisterInteractable(this);
	}
	public virtual void Interact()
	{
		GD.Print($"[Interact] {Name}");
		EmitSignal(SignalName.Interacted);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
