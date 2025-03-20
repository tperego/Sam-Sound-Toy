using UnityEngine;

public class DiceLiftReleaseConstraints : MonoBehaviour
{
    private Rigidbody rb;
    private bool isHeld = false;

    [Header("Force Settings")]
    // The upward impulse when clicking (lifting)
    public float liftForce = 5f;
    // The impulse applied on release in the camera's forward direction.
    public float releaseForceMultiplier = 3f;

    [Header("Custom Gravity Settings")]
    // When true, disable Unity's built-in gravity.
    public bool useCustomGravity = true;
    // Custom gravity force (0.5× Earth’s gravity, roughly 4.9 m/s² downward).
    public Vector3 customGravity = new Vector3(0, -4.9f, 0);

    [Header("Position Constraints")]
    // The dice will be clamped so that X never exceeds this value.
    public float maxX = 0.71f;
    // The dice will be clamped so that Z never exceeds this value.
    public float maxZ = 10.78f;
    // The dice will never drop below this Y value.
    public float minY = 0f;

    [Header("Camera Constraint")]
    // The dice should always remain at least this far in front of the camera.
    public float minDistanceFromCamera = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (useCustomGravity)
        {
            rb.useGravity = false;
        }
    }

    void FixedUpdate()
    {
        // Apply custom gravity if enabled and the dice isn’t being held.
        if (useCustomGravity && !isHeld)
        {
            rb.AddForce(customGravity * rb.mass, ForceMode.Acceleration);
        }

        // --- Enforce Position Constraints ---
        Vector3 pos = transform.position;
        bool clamped = false;

        // Clamp X so it never goes past maxX.
        if (pos.x > maxX)
        {
            pos.x = maxX;
            if (rb.velocity.x > 0)
                rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
            clamped = true;
        }

        // Clamp Z so it never goes past maxZ.
        if (pos.z > maxZ)
        {
            pos.z = maxZ;
            if (rb.velocity.z > 0)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
            clamped = true;
        }

        // Clamp Y so it never falls below minY.
        if (pos.y < minY)
        {
            pos.y = minY;
            if (rb.velocity.y < 0)
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            clamped = true;
        }

        // --- Enforce Camera Constraint ---
        // Ensure the dice always stays in front of the camera.
        Vector3 cameraToDice = pos - Camera.main.transform.position;
        // If the projection on the camera's forward is too small, it's behind or too close.
        if (Vector3.Dot(cameraToDice, Camera.main.transform.forward) < minDistanceFromCamera)
        {
            pos = Camera.main.transform.position + Camera.main.transform.forward * minDistanceFromCamera;
            rb.velocity = Vector3.zero;
            clamped = true;
        }

        // If any clamping was required, update the position.
        if (clamped)
        {
            transform.position = pos;
        }
    }

    // On mouse press, lift the dice upward.
    void OnMouseDown()
    {
        isHeld = true;
        rb.AddForce(Vector3.up * liftForce, ForceMode.Impulse);
    }

    // On mouse release, apply an impulse in the camera's forward direction.
    void OnMouseUp()
    {
        isHeld = false;
        Vector3 forwardForce = Camera.main.transform.forward * releaseForceMultiplier;
        rb.AddForce(forwardForce, ForceMode.Impulse);
    }
}
