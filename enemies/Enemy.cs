using Godot;
using System;
using 勇者传说;

public enum Direction
{
    Left  = -1,
    Right = 1
}

public partial class Enemy : CharacterBody2D, IStateNode
{
    private         Direction _direction   = Direction.Left;
    public          float     Gravity      = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    [Export] public float     MaxSpeed     = 180.0f;
    [Export] public float     Acceleration = 2000.0f;
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
                X = -((int)_direction)
            };
        }
    }
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    [Export] public Node2D          Graphics;
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public StateMachine    StateMachine;
    public virtual int GetNextState(int state)
    {
        return 0;
    }
    public virtual void TickPhysics(int state, double delta)
    {

    }
    public virtual void TransitionState(int from, int to)
    {
    }
    public void Move(float speed, double delta)
    {
        Velocity = Velocity with
        {
            X = Mathf.MoveToward(Velocity.X, (float)speed * (int)Direction, Acceleration * (float)delta),
            Y = Velocity.Y + Gravity * (float)delta,
        };
        MoveAndSlide();
    }
}