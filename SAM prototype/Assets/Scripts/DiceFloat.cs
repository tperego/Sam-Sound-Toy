using UnityEngine;

public class DiceFloatPhysics : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Floating Settings")]
    // Factor to counteract gravity (1.0 cancels gravity entirely).
    public float floatFactor = 1.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Apply an upward force that cancels gravity.
        // Physics.gravity is normally (0, -9.81, 0), so this force will be (0, 9.81, 0) * mass * floatFactor.
        rb.AddForce(-Physics.gravity * rb.mass * floatFactor, ForceMode.Force);
    }
}
