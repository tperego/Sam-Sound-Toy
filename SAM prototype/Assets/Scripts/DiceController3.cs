using UnityEngine;

public class DiceController : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    private Rigidbody rb;

    // These multipliers let you adjust the strength of the torque and force from the inspector.
    public float torqueMultiplier = 0.1f;
    public float forceMultiplier = 0.05f;

    // This will hold the mouse position at the time of clicking.
    private Vector3 lastMousePosition;

    // Assign a Cube Collider (or any Collider) in the Inspector that defines your boundaries.
    public Collider boundaryCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
        // Determine the z distance to the camera for correct screen-to-world conversion.
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();

        // Make the dice kinematic so it can be directly moved without physics interference.
        rb.isKinematic = true;

        // Store the mouse position for calculating movement delta on release.
        lastMousePosition = Input.mousePosition;
    }

    // Converts the current mouse position into world coordinates.
    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        // Calculate the intended target position based on mouse input.
        Vector3 targetPos = GetMouseWorldPos() + offset;

        // If a boundary collider is assigned, clamp the position so the dice stays within its bounds.
        if (boundaryCollider != null)
        {
            Bounds b = boundaryCollider.bounds;
            targetPos.x = Mathf.Clamp(targetPos.x, b.min.x, b.max.x);
            targetPos.y = Mathf.Clamp(targetPos.y, b.min.y, b.max.y);
            targetPos.z = Mathf.Clamp(targetPos.z, b.min.z, b.max.z);
        }

        // Update the dice position.
        transform.position = targetPos;
    }

    void OnMouseUp()
    {
        // Re-enable physics.
        rb.isKinematic = false;

        // Calculate the mouse movement delta.
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;

        // Calculate a torque vector using mouseDelta, with adjustable multiplier.
        Vector3 torque = new Vector3(mouseDelta.y, -mouseDelta.x, 0) * torqueMultiplier;
        rb.AddTorque(torque, ForceMode.Impulse);

        // Optional: Apply a slight linear force in the direction of the mouse movement.
        Vector3 force = new Vector3(mouseDelta.x, mouseDelta.y, 0) * forceMultiplier;
        rb.AddForce(force, ForceMode.Impulse);
    }
}
