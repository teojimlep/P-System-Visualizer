using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        replicatedObject.name = gameObject.name;

        // Change the create button to destroy button
        DeleteChildByName("AddNewRule");
    }

    public void Delete()
    {
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
