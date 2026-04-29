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

    // Use local or world transform when applying the closed transform
    public bool useLocalTransform = true;

    // How long to move back to the original transform (seconds). 0 = instant.
    public float closeMoveDuration = 0.5f;

    // Captured original transform
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Coroutine handle so we don't start multiple simultaneous moves
    private Coroutine closeCoroutine;

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

        // Capture the original transform on awake
        if (useLocalTransform)
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
        else
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
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
        // If we're moving back and the user grabs the book, stop the move
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        isBookOpen = true;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isBookOpen = false;
    }

    void OpenBook()
    {
        animator.SetInteger("Anim", openBookAnim);
        //UI.SetActive(true);
    }

    void CloseBook()
    {
        animator.SetInteger("Anim", closeBookAnim);
        //UI.SetActive(false);

        // If already moving back, don't start another coroutine
        if (closeCoroutine == null)
        {
            closeCoroutine = StartCoroutine(MoveToOriginal(closeMoveDuration));
        }
    }

    private IEnumerator MoveToOriginal(float duration)
    {
        if (duration <= 0f)
        {
            // Instant snap
            if (useLocalTransform)
            {
                transform.localPosition = originalPosition;
                transform.localRotation = originalRotation;
            }
            else
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
            }

            closeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        Vector3 startPos = useLocalTransform ? transform.localPosition : transform.position;
        Quaternion startRot = useLocalTransform ? transform.localRotation : transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(startPos, originalPosition, t);
            Quaternion rot = Quaternion.Slerp(startRot, originalRotation, t);

            if (useLocalTransform)
            {
                transform.localPosition = pos;
                transform.localRotation = rot;
            }
            else
            {
                transform.position = pos;
                transform.rotation = rot;
            }

            yield return null;
        }

        // Ensure final values are exact
        if (useLocalTransform)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
        }
        else
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }

        closeCoroutine = null;
    }
}