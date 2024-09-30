using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PTree : MonoBehaviour
{
    //public TextMeshPro output;
    public TMP_InputField InputEasyMembrane;
    public TMP_InputField NumIterations;
    public GameObject PVisualizerPrefab;
    //public string EasyMembrane;

    public void EvolveAndSpawn()
    {
        // Create a rule that reacts with a multiset and produces a product
        List<(float Probability, string Product)> possibleProducts = new()
        {
            (1f, "(F:1)(W:1)[(f:1)(w:1)(+:1)][(f:1)(w:1)(-:1)]")
        };
        PSystemRule rule = new(reactiveMultiset: "(f:1)(w:1)", possibleProducts);
        List<PSystemRule> rules = new() { rule };
        Membrane treeMembrane = new(easyMembrane: this.InputEasyMembrane.text, rules: rules);
        treeMembrane.Evolve(nIterations: int.Parse(this.NumIterations.text));
        Vector3 spawnPosition = new(0f,0f,0f);
        this.SpawnTree(treeMembrane: treeMembrane, spawnPosition: spawnPosition);
    }

    public void SpawnTree(Membrane treeMembrane, Vector3 spawnPosition)
    {
        // Instantiate PVisualizer object. This is a game object that runs the PSystemVisualizer script.
        GameObject PVisualizerObject = Instantiate(PVisualizerPrefab, spawnPosition, Quaternion.identity, transform);
        // Give a name to the PVisualizer object, which will represent the tree as a group
        PVisualizerObject.name = "PTree";
        // Get PSystemVisualizer object from PVisualizerObject
        PSystemVisualizer PVisualizer = PVisualizerObject.GetComponent<PSystemVisualizer>();
    
        // Prepare the visualizer
        // Instantiate turle orientation and object
        TurtleOrientation initOrientation = new(Vector3.up, Vector3.left, Vector3.forward);
        PVisualizer.Turtle = new Turtle(spawnPosition, initOrientation);
        // Draw the membrane
        Debug.Log($"Drawing membrane: {treeMembrane.ToString()}");
        PVisualizer.DrawMembrane(drawnMembrane: treeMembrane);
    }
}
