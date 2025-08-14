using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State currentState { get; private set; }
    public bool canChangeState = true;

    public void Initialize(State startState)
    {
        canChangeState = true;
        currentState = startState;
        currentState.Enter();
    }
    public void ChangeState(State newState)
    {
        if (canChangeState == false)
            return;

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void UpdateActiveState()
    {
        currentState.Update();
    }

    public void SwitchOffStateMachine() => canChangeState = false;
}