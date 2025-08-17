using UnityEngine;

public class AnimationTriggers : MonoBehaviour
{
    public bool triggerCalled;

    public void AnimationTrigger()
    {
        triggerCalled = true;
    }
}