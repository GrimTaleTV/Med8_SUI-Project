using UnityEngine;
using UnityEngine.InputSystem;

public class animationController : MonoBehaviour
{
    public InputActionProperty grabAction;
    public InputActionProperty triggerAction;

    public Animator animator;

    // Update is called once per frame
    void Update()
    {
       float grabValue = grabAction.action.ReadValue<float>();
       animator.SetFloat("Grab", grabValue);

       float triggerValue = triggerAction.action.ReadValue<float>();
       animator.SetFloat("Trigger", triggerValue);
    }
}
