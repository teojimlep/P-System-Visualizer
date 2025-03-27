using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FocusCamera : MonoBehaviour
{
    public float padding = 2f; // Padding around the object for zoom out
    public float zoomSpeed = 2f; // Speed of zoom in/out

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void FocusOnTarget()
    {
        GameObject targetObject = GameObject.Find("PTree");

        // Access the PSystemVisualizer component in targetObject
        PSystemVisualizer visualizer = targetObject.GetComponent<PSystemVisualizer>();

        if (visualizer != null)
        {
            // Get the center of the explored space
            Vector3 objectCenter = visualizer.Turtle.ExploredSpace.GetCenter();

            // Get the bounding box from the turtle space
            List<float> xLim = visualizer.Turtle.ExploredSpace.XLim;
            List<float> yLim = visualizer.Turtle.ExploredSpace.YLim;
            List<float> zLim = visualizer.Turtle.ExploredSpace.ZLim;

            // Calculate the size of the bounding box
            float objectSize = Mathf.Max(xLim[1] - xLim[0], yLim[1] - yLim[0], zLim[1] - zLim[0]);

            // Calculate the camera's required distance to fit the object on screen
            float distance = objectSize / (2f * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));

            // Adjust the camera's position to focus on the object
            Vector3 desiredPosition = objectCenter - mainCamera.transform.forward * (distance + padding);
            StartCoroutine(SmoothCameraMove(mainCamera.transform.position, desiredPosition, zoomSpeed));
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
}
