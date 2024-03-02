using Godot;
using System;
using System.Collections.Generic;

public enum State : int
{
    Idle,
    Running,
    Jump,
    Fall,
    Landing,
    WallSlide,
    WallJump
}

public partial class Player : CharacterBody2D
{
    public readonly HashSet<State> GroundedStates = new HashSet<State>
    {
        State.Idle,
        State.Running,
        State.Landing
    };
    private       float defaultGravity    = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private const float RunSpeed          = 160f;
    private const float JumpSpeed         = -320f;
    private const float FloorAcceleration = RunSpeed / 0.2f;
    private const float AirAcceleration   = RunSpeed / 0.02f;
    private       float Acceleration => IsOnFloor() ? FloorAcceleration : AirAcceleration;

    public          bool            IsFirstTick = false;
    [Export] public Node2D          Graphics;
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public Timer           CoyoteTimer;
    [Export] public Timer           JumpRequestTimer;
    [Export] public RayCast2D       HandChecker;
    [Export] public RayCast2D       FootChecker;
    private    readonly     Vector2         _wallJumpVelocity  = new Vector2(1000, -320);
    public override void _UnhandledInput(InputEvent @event)
    {
        using var @input = @event;
        if (@input.IsActionPressed("jump"))
        {
            JumpRequestTimer.Start();
        }
        if (@event.IsActionReleased("jump"))
        {
            JumpRequestTimer.Stop();

            var jump = JumpSpeed / 2;

            Velocity = Velocity.Y < jump
                ? Velocity with
                {
                    Y = JumpSpeed / 2
                }
                : Velocity;
        }
    }
    public void TransitionState(int from, int to)
    {
        TransitionState((State)from, (State)to);
    }
    public void TransitionState(State from, State to)
    {
        if (!GroundedStates.Contains(from) && GroundedStates.Contains(to))
        {
            CoyoteTimer.Stop();
        }
        switch (to)
        {

            case State.Idle:
                AnimationPlayer.Play("idle");
                break;
            case State.Running:
                AnimationPlayer.Play("running");
                break;
            case State.Jump:

                AnimationPlayer.Play("jump");
                Velocity = Velocity with
                {
                    Y = JumpSpeed
                };
                CoyoteTimer.Stop();
                JumpRequestTimer.Stop();

                break;
            case State.Fall:
                AnimationPlayer.Play("fall");

                if (GroundedStates.Contains(from))
                {
                    CoyoteTimer.Start();
                }
                break;
            case State.Landing:
                AnimationPlayer.Play("landing");
                break;
            case State.WallSlide:
                AnimationPlayer.Play("wall_sliding");
                break;
            case State.WallJump:
                AnimationPlayer.Play("jump");
                Velocity = _wallJumpVelocity with
                {
                    X = GetWallNormal().X * _wallJumpVelocity.X
                };
                JumpRequestTimer.Stop();

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(to), to, null);
        }
        if (to == State.Jump)
        {
            Engine.TimeScale = 0.3;
        }
        if (from == State.Jump)
        {
            Engine.TimeScale = 1;
        }
        IsFirstTick = true;
    }
    public int GetNextState(int currentState)
    {
        return (int)GetNextState((State)currentState);
    }
    public State GetNextState(State state)
    {
        var canJump    = CoyoteTimer.TimeLeft > 0 || IsOnFloor();
        var shouldJump = canJump && JumpRequestTimer.TimeLeft > 0;
        if (shouldJump)
        {
            return State.Jump;
        }
        var direction = Input.GetAxis("move_left", "move_right");
        var isStill   = direction == 0 && Velocity.X == 0;

        switch (state)
        {
            case State.Idle:
                if (!IsOnFloor())
                {
                    return State.Fall;
                }
                if (!isStill)
                {
                    return State.Running;
                }
                break;
            case State.Running:
                if (!IsOnFloor())
                {
                    return State.Fall;
                }
                if (isStill)
                {
                    return State.Idle;
                }
                break;
            case State.Jump:
                if (Velocity.Y <= 0)
                {
                    return State.Fall;
                }
                break;
            case State.Fall:
                if (IsOnFloor())
                {
                    // return isStill ? State.Idle : State.Running;
                    return isStill ? State.Landing : State.Running;
                }
                if (IsOnWall() && HandChecker.IsColliding() && FootChecker.IsColliding())
                {
                    return State.WallSlide;
                }
                break;
            case State.Landing:
                if (!isStill)
                {
                    return State.Running;
                }
                if (!AnimationPlayer.IsPlaying())
                {
                    return State.Idle;
                }
                break;
            case State.WallSlide:
                if (JumpRequestTimer.TimeLeft > 0)
                {
                    return State.WallJump;
                }
                if (IsOnFloor())
                {
                    return State.Idle;
                }
                if (!IsOnWall())
                {
                    return State.Fall;
                }
                break;
            case State.WallJump:
                if (Velocity.Y>=0)
                {
                    return State.Fall;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
        return state;
    }

    public void TickPhysics(int state, double delta)
    {
        var currentState = (State)state;
        switch (currentState)
        {

            case State.Running:
            case State.Fall:
            case State.Idle:
                Move(defaultGravity, delta);
                break;
            case State.Jump:
                Move(IsFirstTick ? 0 : defaultGravity, delta);
                break;
            case State.Landing:
                Stand(delta);
                break;
            case State.WallSlide:
                Move(defaultGravity / 3, delta);
                Graphics.Scale = Graphics.Scale with
                {
                    X = GetWallNormal().X,
                };
                break;
            case State.WallJump:
                Move(IsFirstTick ? 0 : defaultGravity, delta);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        IsFirstTick = false;

    }
    public void Stand(double delta)
    {
        Velocity = Velocity with
        {
            Y = Velocity.Y + defaultGravity * (float)delta,
            X =
            (float)Mathf.MoveToward(Velocity.X, 0, Acceleration * delta)
        };
        MoveAndSlide();
    }
    public void Move(float gravity, double delta)
    {
        var direction = Input.GetAxis("move_left", "move_right");
        Velocity = Velocity with
        {
            Y = Velocity.Y + gravity * (float)delta,
            X =
            (float)Mathf.MoveToward(Velocity.X, direction * RunSpeed, Acceleration * delta)
        };

        if (direction != 0)
        {
            Graphics.Scale = Graphics.Scale with
            {
                X = direction < 0 ? -1 : 1,
            };
        }

        MoveAndSlide();
    }
}