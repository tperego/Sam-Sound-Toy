using UnityEngine;
using FMODUnity;

public class DiceController3 : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    private Rigidbody rb;

    // These multipliers let you adjust the strength of the torque and force.
    public float torqueMultiplier = 0.1f;
    public float forceMultiplier = 0.05f;

    // This will hold the mouse position in world space at the time of clicking.
    private Vector3 lastMouseWorldPosition;

    // Assign a Cube Collider (or any Collider) in the Inspector that defines your boundaries.
    public Collider boundaryCollider;

    // FMOD Event References (assign these in the Inspector)
    [SerializeField] private EventReference fmodEventAtClick;
    [SerializeField] private EventReference fmodEventAtGround;
    [SerializeField] private EventReference fmodEventAtBorder;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
        // Raycast from the camera through the mouse pointer to determine which face was clicked.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                // Convert hit normal (world space) into local space.
                Vector3 localNormal = transform.InverseTransformDirection(hit.normal);
                string face = DetermineFace(localNormal);
                int diceSide = GetDiceSideFromFace(face);

                // Trigger FMOD click event and set parameter "DiceSides" to the corresponding value.
                var clickInstance = RuntimeManager.CreateInstance(fmodEventAtClick);
                clickInstance.setParameterByName("DiceSides", diceSide);
                clickInstance.start();
                clickInstance.release();
            }
        }

        // Determine the z distance to the camera for correct screen-to-world conversion.
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();

        // Make the dice kinematic so it can be directly moved without physics interference.
        rb.isKinematic = true;

        // Store the mouse's world space position for calculating movement delta on release.
        lastMouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zCoord));
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

        // Convert current mouse position to world space.
        Vector3 currentMouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zCoord));
        Vector3 worldDelta = currentMouseWorldPos - lastMouseWorldPosition;

        // Calculate a torque vector using the world-space delta.
        Vector3 torque = new Vector3(worldDelta.y, -worldDelta.x, 0) * torqueMultiplier;
        rb.AddTorque(torque, ForceMode.Impulse);

        // Apply a linear force based on the world-space movement.
        Vector3 force = worldDelta * forceMultiplier;
        rb.AddForce(force, ForceMode.Impulse);
    }

    // Determines which face of the dice was clicked based on the local normal vector.
    private string DetermineFace(Vector3 localNormal)
    {
        float absX = Mathf.Abs(localNormal.x);
        float absY = Mathf.Abs(localNormal.y);
        float absZ = Mathf.Abs(localNormal.z);

        if (absX > absY && absX > absZ)
            return localNormal.x > 0 ? "Right" : "Left";
        else if (absY > absZ)
            return localNormal.y > 0 ? "Top" : "Bottom";
        else
            return localNormal.z > 0 ? "Front" : "Back";
    }

    // Maps the face string to a dice side number (1–6) for the FMOD parameter "DiceSides".
    private int GetDiceSideFromFace(string face)
    {
        switch (face)
        {
            case "Top": return 6;
            case "Bottom": return 1;
            case "Left": return 4;
            case "Right": return 3;
            case "Front": return 2;
            case "Back": return 5;
            default: return 0;
        }
    }

    // Trigger FMOD events on collision with objects tagged "Ground" or "Boundary".
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            RuntimeManager.PlayOneShot(fmodEventAtGround, transform.position);
        }
        if (collision.gameObject.CompareTag("Boundary"))
        {
            RuntimeManager.PlayOneShot(fmodEventAtBorder, transform.position);
        }
    }
}
