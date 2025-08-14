using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected StateMachine stateMachine;


    protected Rigidbody rb;

    protected float stateTimer;
    protected bool triggerCalled;

    public State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() => triggerCalled = false;
    public virtual void Update() => stateTimer -= Time.deltaTime;

    public virtual void Exit()
    {
    }


    public void AnimationTrigger() => triggerCalled = true;
}