using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Magic : MonoBehaviour
{
    // for determining which spell is selected
    public int spell;
    // for accessing the spell variable in ButtonScript
    public UIController uiController;
    // for accessing the primary button input action
    [SerializeField] private InputActionReference primaryButtonAction;

    [Header("Spell Prefabs")]
    [SerializeField] private GameObject[] spellPrefabs;                             

    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask destructibleLayers;

    [Header("Material Settings")]
    [SerializeField] private Material targetMaterial;

    // MaterialPropertyBlock to avoid instantiating materials per renderer
    private MaterialPropertyBlock mpb;
    // Cache original colors per renderer so we can restore them
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    // (kept for backwards compatibility but no longer used to swap materials)
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    [Header("Pull Spell Settings")]
    [SerializeField] private float pullSpeed = 10f;
    [SerializeField] private float maxPullDuration = 5f;
    [SerializeField] private float pullStopDistance = 0.5f;

    [Header("Push Spell Settings")]
    [SerializeField] private float pushSpeed = 10f;
    [SerializeField] private float maxPushDuration = 5f;
    [SerializeField] private float pushStopDistance = 20f;

    // Track the currently highlighted object and its original material
    private GameObject currentHighlightedObject;
    private Material originalMaterial;
    public GameObject sphere;

    // coroutine handles
    private Coroutine sphereCoroutine;
    private Coroutine pullCoroutine;
    private Coroutine pushCoroutine;

    // cached transform for micro-optimizations
    private Transform cachedTransform;
    // track previous spell to detect changes

    // Update is called once per frame
    void Update()
    {
        spell = uiController.spell;

        // Only perform the highlight logic while the highlight spell is active
        if (spell == 1 || spell >= 14)
        {
            ChangePointedObjectMaterial();
        }
    }

    private void Awake()
    {
        cachedTransform = transform;
        mpb = new MaterialPropertyBlock();
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
        switch (spell)
        {
            case 0:
                Debug.Log("No spell selected");
                break;
            case 1:
                DestroyPointedObject();
                break;
            case 2:
                SpawnObjectAtRaycast(0);
                break;
            case 3:
                SpawnObjectAtRaycast(1);
                break;
            case 4:
                SpawnObjectAtRaycast(2);
                break;
            case 5:
                SpawnObjectAtRaycast(3);
                break;
            case 6:
                SpawnObjectAtRaycast(4);
                break;
            case 7:
                SpawnObjectAtRaycast(5);
                break;
            case 8:
                SpawnObjectAtRaycast(6);
                break;
            case 9:
                SpawnObjectAtRaycast(7);
                break;
            case 10:
                SpawnObjectAtRaycast(8);
                break;
            case 11:
                SpawnObjectAtRaycast(9);
                break;
            case 12:
                SpawnObjectAtRaycast(10);
                break;
            case 13:
                if (sphere != null)
                {
                    sphere.SetActive(true);

                    if (sphereCoroutine != null)
                        StopCoroutine(sphereCoroutine);

                    sphereCoroutine = StartCoroutine(DisableSphereAfterDelay(2f));
                }
                break;
            case 14:
            {
                RaycastHit hit;

                if (Physics.Raycast(cachedTransform.position, cachedTransform.forward, out hit, rayDistance, destructibleLayers))
                {
                    if (hit.collider != null && hit.collider.gameObject.CompareTag("Interactible"))
                    {
                        GameObject target = hit.collider.gameObject;

                        if (pullCoroutine != null)
                            StopCoroutine(pullCoroutine);

                        pullCoroutine = StartCoroutine(PullObjectTowardsCoroutine(target));
                    }
                    else
                    {
                        Debug.Log($"Hit object '{(hit.collider != null ? hit.collider.gameObject.name : "null")}' but it's not tagged as Interactible");
                    }
                }
                else
                {
                    Debug.Log("No object in range to pull");
                }

                break;
            }
            case 15:
            {
                RaycastHit hit;

                if (Physics.Raycast(cachedTransform.position, cachedTransform.forward, out hit, rayDistance, destructibleLayers))
                {
                    if (hit.collider != null && hit.collider.gameObject.CompareTag("Interactible"))
                    {
                        GameObject target = hit.collider.gameObject;

                        if (pushCoroutine != null)
                            StopCoroutine(pushCoroutine);

                        if (pullCoroutine != null)
                        {
                            StopCoroutine(pullCoroutine);
                            pullCoroutine = null;
                        }

                        pushCoroutine = StartCoroutine(PushObjectAwayCoroutine(target));
                    }
                    else
                    {
                        Debug.Log($"Hit object '{(hit.collider != null ? hit.collider.gameObject.name : "null")}' but it's not tagged as Interactible");
                    }
                }
                else
                {
                    Debug.Log("No object in range to push");
                }

                break;
            }

            // Spells 16..24: change color of the pointed object (persisted on press)
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
                ChangePointedObjectColor(spell);
                break;
        }
    }

    void SpawnObjectAtRaycast(int prefabIndex)
    {
        // Check if the index is valid
        if (prefabIndex < 0 || prefabIndex >= spellPrefabs.Length)
        {
            Debug.LogError($"Invalid prefab index: {prefabIndex}");
            return;
        }

        GameObject prefab = spellPrefabs[prefabIndex];

        if (prefab == null)
        {
            Debug.LogError($"Prefab at index {prefabIndex} is null");
            return;
        }

        RaycastHit hit;

        // Cast a ray from the controller's position in the forward direction
        if (Physics.Raycast(cachedTransform.position, cachedTransform.forward, out hit, rayDistance, destructibleLayers))
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
        if (Physics.Raycast(cachedTransform.position, cachedTransform.forward, out hit, rayDistance, destructibleLayers))
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

    // New: change the pointed object's color based on spell (16..24)
    // This method persists the color on the object's material (instancing the material).
    private void ChangePointedObjectColor(int spellIndex)
    {
        RaycastHit hit;

        if (!Physics.Raycast(cachedTransform.position, cachedTransform.forward, out hit, rayDistance, destructibleLayers))
        {
            Debug.Log("No object in range to recolor");
            return;
        }

        if (hit.collider == null || !hit.collider.gameObject.CompareTag("Interactible"))
        {
            Debug.Log($"Hit object '{(hit.collider != null ? hit.collider.gameObject.name : "null")}' but it's not tagged as Interactible");
            return;
        }

        GameObject target = hit.collider.gameObject;

        if (!target.TryGetComponent<Renderer>(out Renderer renderer))
        {
            Debug.Log($"Target '{target.name}' has no Renderer to recolor");
            return;
        }

        // Color mapping
        Color[] fixedColors = new Color[]
        {
            Color.red,   // 16
            Color.blue,  // 17
            Color.yellow,// 18
            Color.green, // 19
            Color.white, // 20
            Color.black, // 21
            Color.gray,  // 22
            Color.magenta// 23
        };

        int idx = spellIndex - 16;
        Color chosenColor = (idx >= 0 && idx < fixedColors.Length) ? fixedColors[idx] : Random.ColorHSV();

        // Determine shader color property if present
        string[] colorProperties = new[] { "_Color", "_BaseColor", "_TintColor" };

        Material matInstance = null;
        try
        {
            // Use renderer.material to ensure we have an instance to write into (persistent change)
            matInstance = renderer.material;
        }
        catch
        {
            matInstance = null;
        }

        bool appliedPersistently = false;

        if (matInstance != null)
        {
            foreach (var prop in colorProperties)
            {
                if (matInstance.HasProperty(prop))
                {
                    matInstance.SetColor(prop, chosenColor);
                    appliedPersistently = true;
                    break;
                }
            }

            // Fallback to material.color if none of the common properties was found
            if (!appliedPersistently)
            {
                try
                {
                    matInstance.color = chosenColor;
                    appliedPersistently = true;
                }
                catch { appliedPersistently = false; }
            }

            // Try emission for visibility if available
            try
            {
                if (matInstance.HasProperty("_EmissionColor"))
                {
                    matInstance.EnableKeyword("_EMISSION");
                    matInstance.SetColor("_EmissionColor", chosenColor);
                }
            }
            catch { /* ignore */ }
        }

        if (appliedPersistently)
        {
            // Update originalColors so highlight/restore uses the new color as the "original"
            // (this prevents RestoreOriginalMaterial from reverting the new color)
            if (matInstance != null)
            {
                Color stored = chosenColor;
                // If material has a property we previously used for highlights, try to use it
                Material matToInspect = renderer.sharedMaterial ?? matInstance;
                string prop = null;
                if (matToInspect != null)
                {
                    if (matToInspect.HasProperty("_Color")) prop = "_Color";
                    else if (matToInspect.HasProperty("_BaseColor")) prop = "_BaseColor";
                    else if (matToInspect.HasProperty("_TintColor")) prop = "_TintColor";
                }

                if (prop != null)
                {
                    // store the color we just set
                    originalColors[renderer] = stored;
                }
                else
                {
                    // fallback store renderer.material.color
                    try { originalColors[renderer] = matInstance.color; }
                    catch { originalColors[renderer] = chosenColor; }
                }
            }
            Debug.Log($"Persisted color {chosenColor} on '{target.name}' (spell {spellIndex})");
        }
        else
        {
            // If we couldn't set material (very unusual), fall back to property block (visual only)
            Material matToInspect2 = renderer.sharedMaterial ?? renderer.material;
            if (matToInspect2 != null)
            {
                foreach (var prop in colorProperties)
                {
                    if (matToInspect2.HasProperty(prop))
                    {
                        // store original if not stored
                        if (!originalColors.ContainsKey(renderer))
                        {
                            Color orig;
                            try { orig = matToInspect2.GetColor(prop); }
                            catch { orig = renderer.material != null ? renderer.material.color : Color.white; }
                            originalColors[renderer] = orig;
                        }

                        renderer.GetPropertyBlock(mpb);
                        mpb.SetColor(prop, chosenColor);
                        renderer.SetPropertyBlock(mpb);
                        Debug.Log($"Applied color via PropertyBlock (non-persistent) on '{target.name}' to {chosenColor} (spell {spellIndex})");
                        appliedPersistently = false;
                        break;
                    }
                }
            }
        }
    }

    void ChangePointedObjectMaterial()
    {
        RaycastHit hit;
        GameObject hitObject = null;

        // Cast a ray from the controller's position in the forward direction
        if (Physics.Raycast(cachedTransform.position, cachedTransform.forward, out hit, rayDistance, destructibleLayers))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Interactible"))
            {
                hitObject = hit.collider.gameObject;
            }
        }

        // If we're pointing at a new object
        if (hitObject != currentHighlightedObject)
        {
            // Restore the previous object's material/color
            RestoreOriginalMaterial();

            // If we hit a new object, apply highlight using MaterialPropertyBlock (no material swap)
            if (hitObject != null)
            {
                if (hitObject.TryGetComponent<Renderer>(out Renderer renderer))
                {
                    // Determine which color property we can set on this renderer's material
                    Material matToInspect = renderer.sharedMaterial ?? renderer.material;
                    string colorProperty = null;
                    if (matToInspect != null)
                    {
                        if (matToInspect.HasProperty("_Color"))
                            colorProperty = "_Color";
                        else if (matToInspect.HasProperty("_BaseColor"))
                            colorProperty = "_BaseColor";
                        else if (matToInspect.HasProperty("_TintColor"))
                            colorProperty = "_TintColor";
                    }

                    // Save current color so we can restore when pointer leaves
                    if (!originalColors.ContainsKey(renderer))
                    {
                        if (matToInspect != null && colorProperty != null)
                        {
                            try { originalColors[renderer] = matToInspect.GetColor(colorProperty); }
                            catch { originalColors[renderer] = renderer.material != null ? renderer.material.color : Color.white; }
                        }
                        else
                        {
                            try { originalColors[renderer] = renderer.material.color; }
                            catch { originalColors[renderer] = Color.white; }
                        }
                    }

                    // Determine highlight color (prefer targetMaterial color if provided)
                    Color highlightColor = Color.white;
                    if (targetMaterial != null)
                    {
                        if (targetMaterial.HasProperty("_Color"))
                            highlightColor = targetMaterial.GetColor("_Color");
                        else if (targetMaterial.HasProperty("_BaseColor"))
                            highlightColor = targetMaterial.GetColor("_BaseColor");
                    }

                    // Apply highlight via MaterialPropertyBlock using the discovered property (or _Color)
                    string propToSet = colorProperty ?? "_Color";
                    renderer.GetPropertyBlock(mpb);
                    mpb.SetColor(propToSet, highlightColor);
                    renderer.SetPropertyBlock(mpb);

                    currentHighlightedObject = hitObject;
                }
            }
        }
    }

    void RestoreOriginalMaterial()
    {
        if (currentHighlightedObject != null)
        {
            if (currentHighlightedObject.TryGetComponent<Renderer>(out Renderer renderer))
            {
                // If we have an original color recorded for this renderer, restore it via property block.
                if (originalColors.TryGetValue(renderer, out Color origColor))
                {
                    Material matToInspect = renderer.sharedMaterial ?? renderer.material;
                    string colorProperty = null;
                    if (matToInspect != null)
                    {
                        if (matToInspect.HasProperty("_Color"))
                            colorProperty = "_Color";
                        else if (matToInspect.HasProperty("_BaseColor"))
                            colorProperty = "_BaseColor";
                        else if (matToInspect.HasProperty("_TintColor"))
                            colorProperty = "_TintColor";
                    }

                    string propToSet = colorProperty ?? "_Color";
                    renderer.GetPropertyBlock(mpb);
                    mpb.SetColor(propToSet, origColor);
                    renderer.SetPropertyBlock(mpb);

                    // keep originalColors entry if it represents the material's persistent color
                    // otherwise remove it so future highlights restore properly
                    originalColors.Remove(renderer);
                }
                else
                {
                    // Nothing recorded -> clear property block so no highlight remains
                    renderer.SetPropertyBlock(null);
                }
            }

            currentHighlightedObject = null;
        }
    }

    private IEnumerator DisableSphereAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (sphere != null)
            sphere.SetActive(false);

        sphereCoroutine = null;
    }

    private IEnumerator PullObjectTowardsCoroutine(GameObject target)
    {
        if (target == null)
            yield break;

        Rigidbody rb = null;
        target.TryGetComponent<Rigidbody>(out rb);
        float elapsed = 0f;

        while (target != null && elapsed < maxPullDuration)
        {
            Vector3 targetPosition = transform.position;
            Vector3 currentPosition = target.transform.position;
            Vector3 toTarget = targetPosition - currentPosition;
            float distance = toTarget.magnitude;

            if (distance <= pullStopDistance)
                break;

            Vector3 direction = toTarget.normalized;

            if (rb != null && !rb.isKinematic)
            {
                // Apply a velocity towards the pull point
                rb.linearVelocity = direction * pullSpeed;
            }
            else
            {
                // Move transform directly if no usable Rigidbody
                target.transform.position = Vector3.MoveTowards(currentPosition, targetPosition, pullSpeed * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null && !rb.isKinematic)
            rb.linearVelocity = Vector3.zero;

        pullCoroutine = null;
    }

    private IEnumerator PushObjectAwayCoroutine(GameObject target)
    {
        if (target == null)
            yield break;

        Rigidbody rb = null;
        target.TryGetComponent<Rigidbody>(out rb);
        float elapsed = 0f;

        while (target != null && elapsed < maxPushDuration)
        {
            Vector3 origin = transform.position;
            Vector3 currentPosition = target.transform.position;
            Vector3 away = currentPosition - origin;
            float distance = away.magnitude;

            if (distance >= pushStopDistance)
                break;

            Vector3 direction = away.normalized;

            if (rb != null && !rb.isKinematic)
            {
                // Apply a velocity away from the origin
                rb.linearVelocity = direction * pushSpeed;
            }
            else
            {
                // Move transform directly if no usable Rigidbody
                target.transform.position += direction * pushSpeed * Time.deltaTime;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null && !rb.isKinematic)
            rb.linearVelocity = Vector3.zero;

        pushCoroutine = null;
    }
}
