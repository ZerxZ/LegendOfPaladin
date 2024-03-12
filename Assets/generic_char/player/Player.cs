using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using 勇者传说.globals;

namespace 勇者传说.Assets.generic_char.player;

public enum Direction : int
{
    Left  = -1,
    Right = 1,
}

public partial class Player : CharacterBody2D, 勇者传说.IStateNode

{


    public enum State : int
    {
        Idle,
        Running,
        Jump,
        Fall,
        Landing,
        WallSlide,
        WallJump,
        Attack1,
        Attack2,
        Attack3,
        Hurt,
        Dying,
        SlidingStart,
        SlidingLoop,
        SlidingEnd,
    }

    public readonly HashSet<State> GroundedStates = new HashSet<State>
    {
        State.Idle,
        State.Running,
        State.Landing,
        State.Attack1,
        State.Attack2,
        State.Attack3,
    };
    public        string[] StateNames        = Enum.GetNames(typeof(State));
    private       float    _defaultGravity   = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private const float    RunSpeed          = 160f;
    private const float    JumpSpeed         = -320f;
    private const float    FloorAcceleration = RunSpeed / 0.2f;
    private const float    AirAcceleration   = RunSpeed / 0.1f;
    public const  double   SlidingDuration   = 0.5;
    private const float    SlidingSpeed      = 256;
    private const float    LandingHeight     = 100;

    private const double SlidingEnergyCost = 1;
    private       float  Acceleration => IsOnFloor() ? FloorAcceleration : AirAcceleration;
    public        float  FallFromY = 0;

    public          bool                 IsFirstTick      = false;
    public          bool                 IsComboRequested = false;
    [Export] public Node2D               Graphics;
    [Export] public AnimationPlayer      AnimationPlayer;
    [Export] public classes.StateMachine StateMachine;
    [Export] public Timer                CoyoteTimer;
    [Export] public Timer                JumpRequestTimer;
    [Export] public Timer                InvincibleTimer;
    [Export] public Timer                SlideRequestTimer;
    [Export] public RayCast2D            HandChecker;
    [Export] public RayCast2D            FootChecker;
    [Export] public classes.Stats        Stats;
    [Export] public classes.Hitbox       Hitbox;
    [Export] public classes.Hurtbox      Hurtbox;
    [Export] public AnimatedSprite2D     InteractIcon;
    [Export] public GameOverScreen       GameOverScreen;
    [Export] public ui.StatusPanel       StatusPanel;
    private         Direction            _direction = Direction.Left;
    [Export] public Direction Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            if (!IsNodeReady())
            {

                return;
            }
            Graphics.Scale = Graphics.Scale with
            {
                X = (int)_direction
            };
        }
    }
    [Export] public  bool                       CanCombo;
    public           classes.Damage             PendingDamage;
    public           List<classes.Interactable> InteractableWith  = new List<classes.Interactable>();
    private readonly Vector2                    _wallJumpVelocity = new Vector2(380, -300);
    public           bool                       CanWallSlide => IsOnWall() && HandChecker.IsColliding() && FootChecker.IsColliding();
    public bool ShouldSlide()
    {
        if (SlideRequestTimer.IsStopped()) return false;
        if (Stats.Energy < SlidingEnergyCost) return false;
        return !FootChecker.IsColliding();
    }


    public override void _Ready()
    {
        Stand(_defaultGravity, 0.01);
        Hurtbox.HurtEntered += OnHurtEntered;
        Direction = Direction;
    }
    public void RegisterInteractable(classes.Interactable interactable)
    {
        if (StateMachine.CurrentState == (int)State.Dying)
        {
            return;
        }
        if (InteractableWith.Contains(interactable))
        {
            return;
        }
        InteractableWith.Add(interactable);
    }
    public void UnregisterInteractable(classes.Interactable interactable)
    {
        InteractableWith.Remove(interactable);
    }
    private void OnHurtEntered(classes.Hitbox hitbox)
    {
        if (InvincibleTimer.TimeLeft > 0)
        {
            return;
        }
        PendingDamage = new classes.Damage()
        {
            Amount = 1,
            Source = hitbox
        };
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        using var @input = @event;
        if (@input.IsActionPressed("jump"))
        {
            JumpRequestTimer.Start();
        }
        if (@input.IsActionReleased("jump"))
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
        if (@input.IsActionPressed("attack") && CanCombo)
        {
            IsComboRequested = true;
        }
        if (@input.IsActionPressed("slide"))
        {
            SlideRequestTimer.Start();
        }
        if (input.IsActionPressed("interact") && InteractableWith.Count > 0)
        {
            InteractableWith.Last().Interact();
        }
    }
    public void TransitionState(int from, int to)
    {

        // GD.Print($"[{Engine.GetPhysicsFrames():0000}] {(from == -1 ? "<Start>":StateNames[from]),-10} => {StateNames[to],10}");
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
                SoundManager.Instance.PlaySfx("Jump");
                break;
            case State.Fall:
                AnimationPlayer.Play("fall");

                if (GroundedStates.Contains(from))
                {
                    CoyoteTimer.Start();
                }
                FallFromY = GlobalPosition.Y;
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
            case State.Attack1:
            case State.Attack2:
            case State.Attack3:
                AnimationPlayer.Play(to switch
                {

                    State.Attack1 => "attack_1",
                    State.Attack2 => "attack_2",
                    State.Attack3 => "attack_3",
                });
                SoundManager.Instance.PlaySfx("Attack");
                IsComboRequested = false;
                break;
            case State.Hurt:
                AnimationPlayer.Play("hurt");
                Stats.Health -= PendingDamage.Amount;

                var dir = PendingDamage.Source.GlobalPosition.DirectionTo(GlobalPosition);
                Velocity = dir * KnockbackAmout;
                InvincibleTimer.Start();
                PendingDamage = null;
                break;
            case State.Dying:
                AnimationPlayer.Play("die");
                InvincibleTimer.Stop();
                InteractableWith.Clear();
                break;
            case State.SlidingStart:
                AnimationPlayer.Play("sliding_start");
                SlideRequestTimer.Stop();
                Stats.Energy -= SlidingEnergyCost;
                break;
            case State.SlidingLoop:
                AnimationPlayer.Play("sliding_loop");
                break;
            case State.SlidingEnd:
                AnimationPlayer.Play("sliding_end");
                break;
        }
        // if (to == State.Jump)
        // {
        //     Engine.TimeScale = 0.3;
        // }
        // if (from == State.Jump)
        // {
        //     Engine.TimeScale = 1;
        // }
        IsFirstTick = true;
    }
    public const float KnockbackAmout = 512;
    public int GetNextState(int currentState)
    {
        return (int)GetNextState((State)currentState);
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
        var canJump    = CoyoteTimer.TimeLeft > 0 || IsOnFloor();
        var shouldJump = canJump && JumpRequestTimer.TimeLeft > 0;
        if (shouldJump)
        {
            return State.Jump;
        }
        if (GroundedStates.Contains(state) && !IsOnFloor())
        {
            return State.Fall;
        }
        var direction = Input.GetAxis("move_left", "move_right");
        var isStill   = direction == 0 && Velocity.X == 0;

        switch (state)
        {
            case State.Idle:
                if (Input.IsActionJustPressed("attack"))
                {
                    return State.Attack1;
                }
                if (ShouldSlide())
                {
                    return State.SlidingStart;
                }
                if (!isStill)
                {
                    return State.Running;
                }
                break;
            case State.Running:
                if (Input.IsActionJustPressed("attack"))
                {
                    return State.Attack1;
                }
                if (ShouldSlide())
                {
                    return State.SlidingStart;
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
                    var height = GlobalPosition.Y - FallFromY;
                    // return isStill ? State.Idle : State.Running;
                    return height > LandingHeight ? State.Landing : State.Running;
                }
                if (CanWallSlide)
                {
                    return State.WallSlide;
                }
                break;
            case State.Landing:
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
                if (CanWallSlide && !IsFirstTick)
                {
                    return State.WallSlide;
                }
                if (Velocity.Y >= 0)
                {
                    return State.Fall;
                }
                break;
            case State.Attack1:
                if (!AnimationPlayer.IsPlaying())
                {
                    return IsComboRequested ? State.Attack2 : State.Idle;
                }
                break;
            case State.Attack2:
                if (!AnimationPlayer.IsPlaying())
                {
                    return IsComboRequested ? State.Attack3 : State.Idle;
                }
                break;
            case State.Attack3:
                if (!AnimationPlayer.IsPlaying())
                {
                    return State.Idle;
                }
                break;
            case State.Hurt:
                if (!AnimationPlayer.IsPlaying())
                {
                    return State.Idle;
                }
                break;
            case State.Dying:
                break;
            case State.SlidingStart:
                if (!AnimationPlayer.IsPlaying())
                {
                    return State.SlidingLoop;
                }
                break;
            case State.SlidingLoop:
                if (StateMachine.StateTime > SlidingDuration || IsOnWall())
                {
                    return State.SlidingEnd;
                }
                break;
            case State.SlidingEnd:
                if (!AnimationPlayer.IsPlaying())
                {
                    return State.Idle;
                }
                break;
        }
        return (State)classes.StateMachine.KeepCurrentState;
    }


    public void TickPhysics(int state, double delta)
    {
        InteractIcon.Visible = InteractableWith is { Count: > 0 };
        if (InvincibleTimer.TimeLeft > 0)
        {
            Graphics.Modulate = Graphics.Modulate with
            {
                // ReSharper disable once PossibleLossOfFraction
                A = Mathf.Sin(Time.GetTicksMsec() / 30) * 0.5f + 0.5f
            };
        }
        else
        {
            Graphics.Modulate = Graphics.Modulate with
            {
                A = 1
            };
        }

        var currentState = (State)state;
        switch (currentState)
        {

            case State.Running:
            case State.Fall:
            case State.Idle:
                Move(_defaultGravity, delta);
                break;
            case State.Jump:
                Move(IsFirstTick ? 0 : _defaultGravity, delta);
                break;
            case State.Landing:
                Stand(_defaultGravity, delta);
                break;
            case State.WallSlide:
                Move(_defaultGravity / 3, delta);
                Direction = GetWallNormal().X > 0 ? Direction.Right : Direction.Left;
                break;
            case State.WallJump:
                if (StateMachine.StateTime < 0.1)
                {
                    Stand(IsFirstTick ? 0 : _defaultGravity, delta);
                    Direction = GetWallNormal().X > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    Move(_defaultGravity, delta);

                }
                break;
            case State.Attack1:
            case State.Attack2:
            case State.Attack3:
                Stand(_defaultGravity, delta);
                break;
            case State.Dying:
            case State.Hurt:
                Stand(_defaultGravity, delta);
                break;
            case State.SlidingStart:
            case State.SlidingLoop:
                Slide(delta);
                break;
            case State.SlidingEnd:
                Stand(_defaultGravity, delta);
                break;
        }
        IsFirstTick = false;

    }
    private void Slide(double delta)
    {
        Velocity = Velocity with
        {
            Y = Velocity.Y + _defaultGravity * (float)delta,
            X = Graphics.Scale.X * SlidingSpeed,
        };
        MoveAndSlide();
    }

    public void Stand(float gravity, double delta)
    {
        Velocity = Velocity with
        {
            Y = Velocity.Y + gravity * (float)delta,
            X =
            (float)Mathf.MoveToward(Velocity.X, 0, Acceleration * delta)
        };
        MoveAndSlide();
    }
    public void Die()
    {
        GameOverScreen.ShowGameOver();
    }
    public void Move(float gravity, double delta)
    {
        var movement = Input.GetAxis("move_left", "move_right");
        Velocity = Velocity with
        {
            Y = Velocity.Y + gravity * (float)delta,
            X =
            (float)Mathf.MoveToward(Velocity.X, movement * RunSpeed, Acceleration * delta)
        };

        if (movement != 0)
        {
            Direction = movement > 0 ? Direction.Right : Direction.Left;
        }

        MoveAndSlide();
    }
}