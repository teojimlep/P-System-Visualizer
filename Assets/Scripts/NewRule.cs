using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewRule : MonoBehaviour
{
    public GameObject uiPrefab; // Assign prefab 
    public Transform parent; // Assign the parent where the new elements will be placed
    private int ruleCount = 2; // Counter to track the number of copies

    public void CopyAndPaste()
    {
        if (uiPrefab != null && parent != null && ruleCount<13)
        {
            Debug.Log("i am doing something");
            // Instantiate a new copy of the prefab
            GameObject newElement = Instantiate(uiPrefab, parent);

            // Get the RectTransform of the original and new element
            Transform originalTransform = uiPrefab.transform;
            Transform newTransform = newElement.transform;

            if (originalTransform != null && newTransform != null)
            {
                // Place the new element just below the original
                Vector3 offset = new Vector3(originalTransform.position.x, originalTransform.position.y -((float)ruleCount - 1f) * 40f, 0f); 
                newTransform.position = parent.position + offset;
            }

            // Assign a unique name to the new element
            newElement.name = $"{uiPrefab.name.Substring(0, uiPrefab.name.Length - 1)}{ruleCount}";
            ruleCount++; // Increment the rule counter
        }
        else if (ruleCount > 12)
        {
            Debug.Log("Too many rules");
        }
    }
}
