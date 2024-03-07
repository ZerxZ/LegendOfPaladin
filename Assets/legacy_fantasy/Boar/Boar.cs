using System;
using Godot;
using 勇者传说.enemies;

namespace 勇者传说.Assets.legacy_fantasy.Boar;

public partial class Boar : enemies.Enemy
{


    public enum State : int
    {
        Idle,
        Walk,
        Run,
        Hurt,
        Dying,
    }

    public override void _Ready()
    {
        Hurtbox.HurtEntered += OnHurtEntered;
        Direction = Direction;
    }
    private void OnHurtEntered(classes.Hitbox hitbox)
    {
        PendingDamage = new classes.Damage
        {
            Amount = 1,
            Source = hitbox,
        };
        // Stats.Health -= 1;
        // GD.Print($"Ouch! {hitbox.Owner.Name} hit me!");
        // GD.Print($"[Hurt] {Stats.Health} / {Stats.MaxHealth}");
        //
        // if (Stats.Health <= 0)
        // {
        //     QueueFree();
        // }

    }
    public const float    KnockbackAmout = 512;
    public       string[] StateNames     = Enum.GetNames(typeof(State));

    [Export] public RayCast2D WallChecker;
    [Export] public RayCast2D FloorChecker;
    [Export] public RayCast2D PlayerChecker;
    [Export] public Timer     CalmDownTimer;
    public override int GetNextState(int state)
    {
        return (int)GetNextState((State)state);
    }
    public State GetNextState(State state)
    {
        if (Stats.Health <= 0)
        {
            return state == State.Dying ? (State)classes.StateMachine.KeepCurrentState : State.Dying;
        }
        if (PendingDamage is not null)
        {
            return State.Hurt;
        }
        switch (state)
        {

            case State.Idle:
                if (CanSeePlayer)
                {
                    return State.Run;
                }
                if (StateMachine.StateTime > 2)
                {
                    return State.Walk;
                }

                break;
            case State.Walk:
                if (CanSeePlayer)
                {
                    return State.Run;
                }
                if (WallChecker.IsColliding() || !FloorChecker.IsColliding())
                {
                    return State.Idle;
                }
                break;
            case State.Run:
                if (!CanSeePlayer && CalmDownTimer.IsStopped())
                {
                    return State.Walk;
                }
                break;

            case State.Hurt:
                if (!AnimationPlayer.IsPlaying())
                {
                    return State.Run;
                }
                break;
            case State.Dying:
                break;
        }
        return (State)classes.StateMachine.KeepCurrentState;
    }
    public bool CanSeePlayer => PlayerChecker.IsColliding() && PlayerChecker.GetCollider() is generic_char.player.Player;
    public override void TransitionState(int from, int to)
    {

        // GD.Print($"[{Engine.GetPhysicsFrames():0000}] {(from == -1 ? "<Start>" : StateNames[from]),-10} => {StateNames[to],10}");
        TransitionState((State)from, (State)to);
    }
    public void TransitionState(State from, State to)
    {
        switch (to)
        {

            case State.Idle:
                AnimationPlayer.Play("idle");
                if (WallChecker.IsColliding())
                {
                    Direction = Direction == Direction.Left ? Direction.Right : Direction.Left;
                }
                break;
            case State.Walk:
                AnimationPlayer.Play("walk");
                if (!FloorChecker.IsColliding())
                {
                    Direction = Direction == Direction.Left ? Direction.Right : Direction.Left;
                    FloorChecker.ForceRaycastUpdate();
                }
                break;
            case State.Run:
                AnimationPlayer.Play("run");
                break;
            case State.Hurt:
                AnimationPlayer.Play("hit");
                Stats.Health -= PendingDamage.Amount;

                var dir = PendingDamage.Source.GlobalPosition.DirectionTo(GlobalPosition);
                Velocity = dir * KnockbackAmout;
                Direction = dir.X < 0 ? Direction.Right : Direction.Left;
                PendingDamage = null;
                break;
            case State.Dying:
                AnimationPlayer.Play("die");
                break;
        }
    }
    public override void TickPhysics(int state, double delta)
    {
        TickPhysics((State)state, delta);
    }
    public void TickPhysics(State state, double delta)
    {

        switch (state)
        {

            case State.Idle:
            case State.Hurt:
            case State.Dying:
                Move(0, delta);
                break;
            case State.Walk:
                Move(MaxSpeed / 3, delta);
                break;
            case State.Run:
                if (WallChecker.IsColliding() || !FloorChecker.IsColliding())
                {
                    Direction = Direction == Direction.Left ? Direction.Right : Direction.Left;
                }
                Move(MaxSpeed, delta);
                if (CanSeePlayer)
                {
                    CalmDownTimer.Start();
                }
                break;

        }
    }
}