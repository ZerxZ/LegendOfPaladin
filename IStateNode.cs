namespace 勇者传说;

public interface IStateNode
{
    int  GetNextState(int state);
    void TickPhysics(int state, double delta);
    void TransitionState(int from, int to);
}