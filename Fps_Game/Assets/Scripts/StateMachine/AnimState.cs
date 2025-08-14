using UnityEngine;

public abstract class AnimState : State
{
    protected string animBoolName;
    protected Animator anim;

    protected AnimState(StateMachine stateMachine, string animBoolName, Animator anim) : base(stateMachine)
    {
        this.animBoolName = animBoolName;
        this.anim = anim;
    }

    public override void Enter()
    {
        base.Enter();
        anim.SetBool(animBoolName, true);
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool(animBoolName, false);
    }
}