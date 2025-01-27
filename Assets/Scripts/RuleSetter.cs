using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RuleSetter : MonoBehaviour
{
    public float verticalPadding = 50f;

    // Method to replicate the entire object
    public void Replicate()
    {
        // Create a copy of this object
        GameObject replicatedObject = Instantiate(gameObject);

        // Adjust the position of the replicated object
        Vector3 currentPosition = transform.position;
        replicatedObject.transform.position = new Vector3(
            currentPosition.x,
            currentPosition.y - verticalPadding,
            currentPosition.z
        );

        // Make the replicated object a sibling (or child, if necessary)
        replicatedObject.transform.SetParent(transform.parent);

        // Optional: Clear "Clone" suffix from the name
        string[] nameParts = gameObject.name.Split('_');
        replicatedObject.name = $"{string.Join("_", nameParts, 0, nameParts.Length - 1)}_{int.Parse(nameParts[^1]) + 1}";

        //replicatedObject.name = gameObject.name;

        replicatedObject.transform.Find("MulPredecessor").GetComponentInChildren<TMP_InputField>().text = "";
        replicatedObject.transform.Find("MulPredecessor").GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = "Reactives";
        replicatedObject.transform.Find("MulSuccessor").GetComponentInChildren<TMP_InputField>().text = "";
        replicatedObject.transform.Find("MulSuccessor").GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = "Products";

        // Change the create button to destroy button
        DeleteChildByName("AddNewRule");

        // Increase the Rules height to make it responsive
        RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
        Vector2 currentRulesPosition = parentRectTransform.anchoredPosition;
        parentRectTransform.sizeDelta += new Vector2(0, +verticalPadding);
        parentRectTransform.anchoredPosition = currentRulesPosition;
    }

    public void Delete()
    {
        // Get the index of the deleted object
        string[] nameParts = gameObject.name.Split('_');
        int deletedIndex = int.Parse(nameParts[^1]);
        Debug.Log($"{deletedIndex}");

        // Get all sibling objects with the same naming convention
        Transform parentTransform = transform.parent;
        var allObjects = parentTransform.GetComponentsInChildren<Transform>(false);
        Debug.Log(parentTransform.name);
        // Iterate through all objects and decrement names of those with higher numbers
        foreach (var obj in allObjects)
        {
            Debug.Log(obj.name);
            if (obj.parent == parentTransform)
            {
                string[] objNameParts = obj.name.Split('_');
                int currentIndex = int.Parse(objNameParts[^1]);
                if (currentIndex > deletedIndex)
                {
                    obj.name = $"RuleSetter_{currentIndex - 1}";
                    Vector3 currentPosition = obj.transform.position;
                    obj.transform.position = new Vector3(
                        currentPosition.x,
                        currentPosition.y + verticalPadding,
                        currentPosition.z
                    );
                }
            }
        }

        // Reduce the Rules height to make it responsive
        RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
        Vector2 currentRulesPosition = parentRectTransform.anchoredPosition;
        parentRectTransform.sizeDelta += new Vector2(0, -verticalPadding);
        parentRectTransform.anchoredPosition = currentRulesPosition;

        // Destroy the object
        Destroy(gameObject);
    }

    public void DeleteChildByName(string childName)
    {
        Transform child = transform.Find(childName);
        if (child != null)
        {
            Destroy(child.gameObject);
        }
        else
        {
            Debug.LogWarning($"Child with name {childName} not found.");
        }
    }
}
