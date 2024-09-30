using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// New Program class to simulate the membrane evolution
public class MembraneExample : MonoBehaviour
{

    public GameObject branchPrefab;


    public void Start()
    {
    }

    public void TestWeird()
    {
        Membrane weirdMembrane = new(easyMembrane: "[]");
        Debug.Log($"Hierarchy: {weirdMembrane.GetHierarchy()}");
        Debug.Log($"Multiset: {weirdMembrane.Multiset.ToString()}");
        Debug.Log($"Membrane: {weirdMembrane.ToString()}");
    }

    public void TestEvolution()
    {
        // Create a rule that reacts with a multiset and produces a product
        List<(float Probability, string Product)> possibleProducts = new()
        {
            (0.3f, "(a:2)"),
            (0.3f, "(b:1)"),
            (0.4f, "")
        };
        PSystemRule rule = new(reactiveMultiset: "(a:1)", possibleProducts);
        List<PSystemRule> rules = new() { rule };

        // Create a membrane with the initial multiset and rule
        Membrane someMembrane = new(easyMembrane: "[(a:2)]", rules: rules);
        Debug.Log("Initial Membrane:");
        Debug.Log(someMembrane);
        
        // Call evolve to simulate the membrane process
        Debug.Log("Evolving");
        someMembrane.Evolve(nIterations: 8);

        // Print the updated multiset after evolution
        Debug.Log("Updated Membrane:");
        Debug.Log(someMembrane);
    }
}
