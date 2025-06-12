using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.GameCenter;
using Unity.VisualScripting;
using SFB;

public class FocusCamera : MonoBehaviour
{
    public float relPadding = 0.1f; // Padding around the object for zoom out
    public float autoZoomSpeed = 1f; // Speed of zoom in/out
    public float panSpeed = 20f;  // Speed of panning
    public float zoomSpeed = 200f;  // Speed of zooming
    public float rotateSpeed = 20000f; // Speed of rotation
    public float minZoom = 1f;    // Minimum zoom distance
    public float maxZoom = 500f;   // Maximum zoom distance

    private Camera mainCamera;

    private (Vector3 pos, Quaternion rot, Vector3 scale) originalCameraConfig;

    private Vector3 lastMousePosition;
    private GameObject targetObject;
    private List<float> xLim;
    private List<float> yLim;
    private List<float> zLim;
    private Vector3 targetCenter;
    private float targetRadius;

    void Start()
    {
        mainCamera = Camera.main;
        // Store
        originalCameraConfig = (mainCamera.transform.position, mainCamera.transform.rotation, mainCamera.transform.localScale);
    }

    void Update()
    {
        if (targetObject != null)
        {
            float distanceToTarget = Vector3.Distance(mainCamera.transform.position, targetObject.transform.position);
            // Debug.Log($"distance {distanceToTarget}");
            // Zoom with scroll wheel
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0)
            {
                Debug.Log($"zoom {zoom}");
                Vector3 direction = (mainCamera.transform.position - targetCenter).normalized;
                Vector3 zoomStep = zoom * zoomSpeed * Time.deltaTime * direction;
                Vector3 newCameraPosition = mainCamera.transform.position + zoomStep;
                float newDistance = Vector3.Distance(newCameraPosition, targetCenter);
                mainCamera.transform.position = newCameraPosition;
            }

            // Pan with left mouse button
            if (Input.GetMouseButton(0))
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                Vector3 move = (-mainCamera.transform.right * mouseDelta.x - mainCamera.transform.up * mouseDelta.y) * panSpeed * Time.deltaTime;
                mainCamera.transform.Translate(move, Space.World);
            }

            // Rotate with right mouse button
            if (Input.GetMouseButton(1))
            {
                float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                float rotateY = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

                if (Mathf.Abs(rotateX) > Mathf.Abs(rotateY))
                {
                    mainCamera.transform.RotateAround(targetObject.transform.position, Vector3.up, rotateX);
                }
                else
                {
                    mainCamera.transform.RotateAround(targetCenter, mainCamera.transform.right, rotateY);
                }
            }

            // Focus on the target
            if (Input.GetKeyDown(KeyCode.F))
            {
                FocusOnTarget();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                FocusOnTarget();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                FocusOnTarget();
                StartCoroutine(CaptureScreenshot(mainCamera));
            }

            lastMousePosition = Input.mousePosition;
        }
    }

    public void FocusOnTarget()
    {
        (mainCamera.transform.position, mainCamera.transform.rotation, mainCamera.transform.localScale) = originalCameraConfig;
        // Locate the object of interest
        targetObject = GameObject.Find("PTree");
        // Access the PSystemVisualizer component in targetObject
        PSystemVisualizer visualizer = targetObject.GetComponent<PSystemVisualizer>();

        if (visualizer != null)
        {
            // Get the center of the explored space
            this.targetCenter = visualizer.Turtle.ExploredSpace.GetCenter();
            //visualizer.Turtle.ExploredSpace.DrawBoundingBox();
            //Debug.Log($"Calculated center {targetCenter}");

            // Get the bounding box from the turtle space
            xLim = visualizer.Turtle.ExploredSpace.XLim;
            yLim = visualizer.Turtle.ExploredSpace.YLim;
            zLim = visualizer.Turtle.ExploredSpace.ZLim;

            // Calculate the size of the bounding box
            float objectSize = Mathf.Max(xLim[1] - xLim[0], yLim[1] - yLim[0], zLim[1] - zLim[0]);

            targetRadius = Mathf.Max(xLim[1] - xLim[0], yLim[1] - yLim[0]);
            Debug.Log($"target radius {targetRadius}");

            // Calculate the camera's required distance to fit the object on screen
            float distance = objectSize / (2f * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));

            // Adjust the camera's position to focus on the object
            Vector3 desiredPosition = targetCenter - mainCamera.transform.forward * (distance * (1 + relPadding));
            StartCoroutine(SmoothCameraMove(mainCamera.transform.position, desiredPosition, autoZoomSpeed));
        }
        else
        {
            Debug.LogError("PSystemVisualizer component not found on target object.");
        }
    }

    private IEnumerator SmoothCameraMove(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = endPos;
    }
    private IEnumerator CaptureScreenshot(Camera cam)
    {
        yield return new WaitForSeconds(0.5f);  // wait half a second for camera to settle
        int width = Screen.width;
        int height = Screen.height;

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        var path = StandaloneFileBrowser.SaveFilePanel("Save Screenshot", "", "screenshot", "png");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log($"Screenshot saved to: {path}");
        }
        else
        {
            Debug.Log("Screenshot save cancelled.");
        }

        yield return null;
    }
}
