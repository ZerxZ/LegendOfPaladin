using Godot;
using System;


public partial class StateMachine : Node
{
    private int _currentState = -1;
    public int CurrentState
    {
        get => _currentState;
        protected set
        {
            Player?.TransitionState(_currentState, value);
            _currentState = value;
            StateTime = 0;
        }
    }
    public Player Player { get; private set; } = null;
    public bool   IsPlayerReady = false;
    public double StateTime = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
      
        if (Owner is not Player player) return;
        Player = player;
        Player.Ready += PlayerReady;


    }
    private void PlayerReady()
    {
        CurrentState = 0;
        IsPlayerReady = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Player is null || !IsPlayerReady)
        {
            return;
        }
        var nextState = Player.GetNextState(CurrentState);
        while (nextState != CurrentState)
        {
            CurrentState = nextState;
            nextState = Player.GetNextState(CurrentState);
        }
     
        Player.TickPhysics(_currentState, delta);
        StateTime += delta;

    }
}