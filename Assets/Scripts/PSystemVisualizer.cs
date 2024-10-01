using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PSystemVisualizer : MonoBehaviour
{
    public GameObject BranchPrefab;
    public float BranchUnitLength;
    public float BranchUnitRadius;
    public float Angle;

    public Turtle _turtle;
    public Turtle Turtle{get{return this._turtle;}set{this._turtle=value;}}

    public void CreateBranch(Vector3 startPos, Vector3 endPos, float branchRadius)
    {
        float length = Vector3.Distance(startPos, endPos);
        if (length > 0f)
        {
            // Debug line
            //Debug.DrawLine(startPos, endPos, Color.red, 60f);

            // Scaling of the prefab object before instantiating. 
            // Scale is based on the prefab dimensions (cilinder of R=1, L=2; Length is y-axis)
            BranchPrefab.transform.localScale = new Vector3(branchRadius, length/2f, branchRadius);
            
            // Spawning characteristics of the branch 
            Vector3 spawnPosition = Vector3.Lerp(startPos, endPos, 0.5f);
            Vector3 Orientation = Vector3.Normalize(endPos - startPos);
            Quaternion spawnRotation = Quaternion.FromToRotation(new Vector3(0f,1f,0f), Orientation);
            
            // Instantiation of the branch game object
            GameObject createdBranch = Instantiate(BranchPrefab, spawnPosition, spawnRotation, transform);

            // Renaming that could be more suitable if related to the hierarchy of the corresponding membrane
            // createdBranch.name = "Branch_" + NumBranches.ToString();
        }   
    }

    public void DrawMembrane(Membrane drawnMembrane)
    {
        // Counting the number of W's and F's to determine the width and length of the created branch
        float branchRadius = (1 + Mathf.Log(drawnMembrane.Multiset["W"])) * this.BranchUnitRadius;
        float branchLength = drawnMembrane.Multiset["F"]*  this.BranchUnitLength;
        // Determining the net count of +'s to determine the angle to be rotated
        float upRotationAngleMultiplier = drawnMembrane.Multiset["+"] - drawnMembrane.Multiset["-"];        
        float leftRotationAngleMultiplier = drawnMembrane.Multiset["&"] - drawnMembrane.Multiset["^"];
        float headRotationAngleMultiplier = drawnMembrane.Multiset[">"] - drawnMembrane.Multiset["<"];

        // Turtle operations:
        Vector3 startPos = _turtle.State.Position; // Store initial position
        // Turtle rotates, advances and pushes its state into its stack
        _turtle.Rotate(upRotationAngleMultiplier*this.Angle, _turtle.State.Orientation.up); // Rotate first
        _turtle.Rotate(leftRotationAngleMultiplier*this.Angle, _turtle.State.Orientation.left); // Rotate first
        _turtle.Rotate(headRotationAngleMultiplier*this.Angle, _turtle.State.Orientation.head); // Rotate first
        _turtle.Forward(branchLength); // Once rotated, the turtle can advance the desired distance

        // Creation of the branch after the turtle operations:
        this.CreateBranch(startPos, _turtle.State.Position, branchRadius);

        // Iterate over the inner membranes repeating this process recursively
        foreach (Membrane innerMembrane in drawnMembrane.InnerMembranes)
        {        
            _turtle.PushState(); // Saving current state
            this.DrawMembrane(innerMembrane); // Draw inner membrane
            _turtle.PopState(); // Going back to previous state
        }
    }
}
