using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;  // Speed of panning
    public float zoomSpeed = 5f;  // Speed of zooming
    public float rotateSpeed = 50f; // Speed of rotation
    public float minZoom = 5f;    // Minimum zoom distance
    public float maxZoom = 50f;   // Maximum zoom distance

    private Camera mainCamera;
    private Vector3 lastMousePosition;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Handle zooming with mouse scroll wheel
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        if (zoom != 0)
        {
            mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - zoom * zoomSpeed, 20f, 80f);
        }

        // Handle panning with right mouse button
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-mouseDelta.x * panSpeed * Time.deltaTime, -mouseDelta.y * panSpeed * Time.deltaTime, 0);
            mainCamera.transform.Translate(move, Space.World);
        }

        // Handle rotation with middle mouse button or left mouse button (or custom input)
        if (Input.GetMouseButton(2))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            float rotateY = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

            // Rotate around the object's center
            transform.RotateAround(transform.position, Vector3.up, rotateX);
            transform.RotateAround(transform.position, transform.right, rotateY);
        }

        lastMousePosition = Input.mousePosition;
    }
}
