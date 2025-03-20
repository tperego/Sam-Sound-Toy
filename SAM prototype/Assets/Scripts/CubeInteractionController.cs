using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInteractionController : MonoBehaviour
{
    private enum InteractionMode { None, Dragging, Rotating }
    private InteractionMode currentMode = InteractionMode.None;

    // For dragging
    private Vector3 dragOffset;
    private float dragZCoord;

    // For momentum computation using a time window
    private List<Vector3> dragPositions = new List<Vector3>();
    private List<float> dragTimes = new List<float>();

    [SerializeField, Tooltip("Seconds over which to calculate momentum (increased for smoother velocity)")]
    private float momentumTimeWindow = 0.3f;

    private Vector3 computedVelocity = Vector3.zero;

    // Inspector-exposed settings for momentum scaling per axis
    [Header("Momentum Scaling Settings")]
    [SerializeField, Tooltip("Multiply the computed momentum on each axis.")]
    private Vector3 momentumScale = new Vector3(0.5f, 0.3f, 0.5f);

    // Inspector-exposed settings for momentum clamping per axis
    [Header("Momentum Clamping Settings")]
    [SerializeField, Tooltip("Maximum allowed momentum on each axis.")]
    private Vector3 maxMomentum = new Vector3(10f, 2f, 10f);

    // --- Floating (bobbing) settings ---
    [Header("Floating Settings")]
    [SerializeField, Tooltip("Base amplitude of floating bob.")]
    private float floatAmplitude = 0.5f;
    [SerializeField, Tooltip("Frequency of floating bob.")]
    private float floatFrequency = 1f;
    private Vector3 initialPosition;
    private bool resumeFloating = true; // Determines whether floating (bobbing) is active

    // Reference to the Rigidbody
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing!");
        }
        initialPosition = transform.position;

        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Please tag your camera as 'MainCamera'.");
        }

        // Start with floating: disable physics control
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        // Floating bobbing effect when not interacting.
        if (resumeFloating)
        {
            Vector3 bobbingOffset = new Vector3(0, Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, 0);
            transform.position = initialPosition + bobbingOffset;
        }
    }

    void OnMouseDown()
    {
        // Disable floating during interaction.
        resumeFloating = false;

        // Reset momentum buffers
        dragPositions.Clear();
        dragTimes.Clear();
        dragPositions.Add(transform.position);
        dragTimes.Add(Time.time);

        // Check for rotation mode if desired (here we just focus on dragging)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMode = InteractionMode.Rotating;
            // Rotation logic could be added here.
        }
        else
        {
            currentMode = InteractionMode.Dragging;
            dragZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
            dragOffset = transform.position - GetMouseWorldPos(dragZCoord);
        }

        // Ensure physics isn’t interfering while dragging.
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnMouseDrag()
    {
        if (currentMode == InteractionMode.Dragging)
        {
            Vector3 newPos = GetMouseWorldPos(dragZCoord) + dragOffset;
            transform.position = newPos;

            // Record positions and times for momentum
            dragPositions.Add(newPos);
            dragTimes.Add(Time.time);

            // Remove old samples outside the time window
            while (dragTimes.Count > 0 && Time.time - dragTimes[0] > momentumTimeWindow)
            {
                dragTimes.RemoveAt(0);
                dragPositions.RemoveAt(0);
            }
        }
        else if (currentMode == InteractionMode.Rotating)
        {
            // Implement rotation logic if needed.
        }
    }

    void OnMouseUp()
    {
        // Calculate momentum if dragging.
        if (currentMode == InteractionMode.Dragging && dragPositions.Count >= 2)
        {
            float dt = dragTimes[dragTimes.Count - 1] - dragTimes[0];
            if (dt > 0)
            {
                computedVelocity = (dragPositions[dragPositions.Count - 1] - dragPositions[0]) / dt;
            }
        }

        // Scale and clamp the computed velocity.
        Vector3 scaledVelocity = new Vector3(
            computedVelocity.x * momentumScale.x,
            computedVelocity.y * momentumScale.y,
            computedVelocity.z * momentumScale.z
        );

        float clampedX = Mathf.Clamp(scaledVelocity.x, -maxMomentum.x, maxMomentum.x);
        float clampedY = Mathf.Clamp(scaledVelocity.y, -maxMomentum.y, maxMomentum.y);
        float clampedZ = Mathf.Clamp(scaledVelocity.z, -maxMomentum.z, maxMomentum.z);
        computedVelocity = new Vector3(clampedX, clampedY, clampedZ);

        // Temporarily let physics take over so the impulse is applied.
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(computedVelocity, ForceMode.Impulse);
        }

        // Begin the transition back to floating.
        StartCoroutine(TransitionToFloating());
        currentMode = InteractionMode.None;
    }

    // This coroutine allows the momentum impulse to act before smoothly transitioning back to floating.
    private IEnumerator TransitionToFloating()
    {
        // Let the impulse effect be visible.
        yield return new WaitForSeconds(0.5f);

        // Optionally, allow some time for physics to settle.
        yield return new WaitForSeconds(1.0f);

        // Set the current position as the new baseline for floating.
        initialPosition = transform.position;

        if (rb != null)
        {
            // Stop any residual motion.
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Disable physics so floating can control the cube.
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        // Resume the floating bob.
        resumeFloating = true;
    }

    // Helper: Converts mouse position to world coordinates at a given z depth.
    private Vector3 GetMouseWorldPos(float z)
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}

