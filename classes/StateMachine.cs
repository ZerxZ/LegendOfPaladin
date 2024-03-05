using Godot;

namespace 勇者传说.classes;

public partial class StateMachine : Node
{
    public const int KeepCurrentState = -1;
    private      int _currentState    = -1;
    public int CurrentState
    {
        get => _currentState;
        protected set
        {
            OwnerState?.TransitionState(_currentState, value);
            _currentState = value;
            StateTime = 0;
        }
    }
    public IStateNode OwnerState { get; private set; } = null;
    public bool       IsOwnerReady = false;
    public double     StateTime    = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        if (Owner is not IStateNode stateNode) return;
        OwnerState = stateNode;
        Owner.Ready += OwnerReady;


    }
    private void OwnerReady()
    {
        CurrentState = 0;
        IsOwnerReady = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (OwnerState is null || !IsOwnerReady)
        {
            return;
        }
        var nextState = OwnerState.GetNextState(CurrentState);
        while (nextState != KeepCurrentState)
        {
            CurrentState = nextState;
            nextState = OwnerState.GetNextState(CurrentState);
        }
        OwnerState.TickPhysics(_currentState, delta);
        StateTime += delta;

    }
}