using UnityEngine;

public class MouseDragController : MonoBehaviour
{
    private Transform selectedObject;
    private Vector3 offset;
    private float zCoord;
    private Rigidbody selectedRigidbody;

    void Update()
    {
        // Mouse button pressed down
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the raycast hit an object with a collider
            if (Physics.Raycast(ray, out hit))
            {
                selectedObject = hit.transform;
                Debug.Log("Selected object: " + selectedObject.name);

                // Calculate z coordinate from camera and offset for smooth dragging
                zCoord = Camera.main.WorldToScreenPoint(selectedObject.position).z;
                offset = selectedObject.position - GetMouseWorldPos();

                // Handle Rigidbody (if available)
                selectedRigidbody = selectedObject.GetComponent<Rigidbody>();
                if (selectedRigidbody != null)
                {
                    selectedRigidbody.useGravity = false;
                    selectedRigidbody.isKinematic = true;
                }
            }
            else
            {
                Debug.Log("No object hit by raycast.");
            }
        }

        // Drag the selected object
        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            selectedObject.position = GetMouseWorldPos() + offset;
        }

        // Release the object on mouse button up
        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            if (selectedRigidbody != null)
            {
                selectedRigidbody.useGravity = true;
                selectedRigidbody.isKinematic = false;
            }
            Debug.Log("Released object: " + selectedObject.name);
            selectedObject = null;
        }
    }

    // Converts mouse screen position to world position at the correct depth (zCoord)
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
