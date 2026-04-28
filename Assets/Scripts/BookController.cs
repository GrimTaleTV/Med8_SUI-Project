using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BookController : MonoBehaviour
{
    public Animator animator;
    private int closeBookAnim = 0;
    private int openBookAnim = 1;
    public bool isBookOpen;
    private bool noLoop;

    public GameObject UI;

    // Optional: assign in inspector or it'll be grabbed from the same GameObject
    public XRBaseInteractable interactable;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (interactable == null)
        {
            interactable = GetComponent<XRBaseInteractable>();
        }
    }

    void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }
    }

    void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    void Update()
    {
        if (isBookOpen == true && noLoop == false)
        {
            OpenBook();
            noLoop = true;
        }

        if (isBookOpen == false)
        {
            CloseBook();
            noLoop = false;
        }
    }

    // XR grab (select) callbacks
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isBookOpen = true;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isBookOpen = false;
    }

    void OpenBook()
    {
        animator.SetInteger("Anim", openBookAnim);
        UI.SetActive(true);
    }

    void CloseBook()
    {
        animator.SetInteger("Anim", closeBookAnim);
        UI.SetActive(false);
    }
}