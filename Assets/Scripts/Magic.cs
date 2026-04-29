using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class Magic : MonoBehaviour
{
    // for determining which spell is selected
    public int spell;
    // for accessing the spell variable in ButtonScript
    public UIController uiController;
    // for accessing the primary button input action
    [SerializeField] private InputActionReference primaryButtonAction;

    public GameObject squarePrefab;
    public GameObject ballPrefab;

    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask destructibleLayers;

    [Header("Material Settings")]
    [SerializeField] private Material targetMaterial;

    // Track the currently highlighted object and its original material
    private GameObject currentHighlightedObject;
    private Material originalMaterial;

    // Update is called once per frame
    void Update()
    {
        spell = uiController.spell;
        
        if (spell == 3)
        {
            // Change material of pointed object
            ChangePointedObjectMaterial();
        }
    }

    void OnEnable()
    {
        primaryButtonAction.action.performed += OnPrimaryButtonPressed;
        primaryButtonAction.action.Enable();
    }

    void OnDisable()
    {
        primaryButtonAction.action.performed -= OnPrimaryButtonPressed;
        primaryButtonAction.action.Disable();
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Primary button pressed!");
        whichSpell();
    }

    void whichSpell()
    {
        if (spell == 0)
        {
            Debug.Log("No spell selected");
        }

        if (spell == 1)
        {
            Debug.Log("First spell selected");
            SpawnObjectAtRaycast(squarePrefab);
        }

        if (spell == 2)
        {
            Debug.Log("Second spell selected");
            SpawnObjectAtRaycast(ballPrefab);
        }

        if (spell == 3)
        {
            Debug.Log("Third spell selected");
            DestroyPointedObject();
        }
    }

    void SpawnObjectAtRaycast(GameObject prefab)
    {
        RaycastHit hit;

        // Cast a ray from the controller's position in the forward direction
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance))
        {
            // Spawn at the hit point with rotation aligned to the surface normal
            Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
            Debug.Log($"Spawned {prefab.name} at {hit.point}");
        }
        else
        {
            // If no hit, spawn at max distance
            Vector3 spawnPosition = transform.position + transform.forward * rayDistance;
            Instantiate(prefab, spawnPosition, Quaternion.identity);
            Debug.Log($"No surface hit, spawned {prefab.name} at max distance");
        }
    }

    void DestroyPointedObject()
    {
        RaycastHit hit;

        // Cast a ray from the controller's position in the forward direction
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance))
        {
            // Check if the object has the "Interactible" tag
            if (hit.collider.gameObject.CompareTag("Interactible"))
            {
                Debug.Log($"Destroying interactible object: {hit.collider.gameObject.name}");
                Destroy(hit.collider.gameObject);
            }
            else
            {
                Debug.Log($"Hit object '{hit.collider.gameObject.name}' but it's not tagged as Interactible");
            }
        }
        else
        {
            Debug.Log("No object in range to destroy");
        }
    }

    void ChangePointedObjectMaterial()
    {
        RaycastHit hit;
        GameObject hitObject = null;

        // Cast a ray from the controller's position in the forward direction
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance))
        {
            // Check if the object has the "Interactible" tag
            if (hit.collider.gameObject.CompareTag("Interactible"))
            {
                hitObject = hit.collider.gameObject;
            }
        }

        // If we're pointing at a new object
        if (hitObject != currentHighlightedObject)
        {
            // Restore the previous object's material
            RestoreOriginalMaterial();

            // If we hit a new object, change its material
            if (hitObject != null)
            {
                Renderer renderer = hitObject.GetComponent<Renderer>();
                
                if (renderer != null && targetMaterial != null)
                {
                    // Store the original material and object reference
                    originalMaterial = renderer.material;
                    currentHighlightedObject = hitObject;
                    
                    // Apply the new material
                    renderer.material = targetMaterial;
                }
            }
        }
    }

    void RestoreOriginalMaterial()
    {
        if (currentHighlightedObject != null)
        {
            Renderer renderer = currentHighlightedObject.GetComponent<Renderer>();
            
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }

            // Clear references
            currentHighlightedObject = null;
            originalMaterial = null;
        }
    }
}
