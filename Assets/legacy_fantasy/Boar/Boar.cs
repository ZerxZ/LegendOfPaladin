using Godot;
using System;

public partial class Boar : Enemy
{
    public enum State : int
    {
        Idle,
        Walk,
        Run,
        Hit,
    }

    public          string[]  StateNames = Enum.GetNames(typeof(State));
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
        if (CanSeePlayer)
        {
            return State.Run;
        }
        switch (state)
        {

            case State.Idle:
                if (StateMachine.StateTime > 2)
                {
                    return State.Walk;
                }

                break;
            case State.Walk:
                if (WallChecker.IsColliding() || !FloorChecker.IsColliding())
                {
                    return State.Idle;
                }
                break;
            case State.Run:
                if (CalmDownTimer.IsStopped())
                {
                    return State.Walk;
                }
                break;
            case State.Hit:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
        return state;
    }
    public bool CanSeePlayer=>PlayerChecker.IsColliding() && PlayerChecker.GetCollider() is Player;
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
            case State.Hit:
                AnimationPlayer.Play("hit");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(to), to, null);
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
            case State.Hit:

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}