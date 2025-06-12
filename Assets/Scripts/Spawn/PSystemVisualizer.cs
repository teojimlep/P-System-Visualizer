using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics.Tracing;
using UnityEngine.Animations;

public class PSystemVisualizer : MonoBehaviour
{
    public GameObject BranchPrefab;
    public float BranchUnitLength;
    public float BranchUnitRadius;
    public float headAngle;
    public float leftAngle;
    public float upAngle;

    public Turtle _turtle;
    public Turtle Turtle{get{return this._turtle;}set{this._turtle=value;}}

    public void CreateBranch(Vector3 startPos, Vector3 endPos, float branchRadius, bool isLeaf, string name)
    {
        float length = Vector3.Distance(startPos, endPos);
        if (length > 0f)
        {
            // Debug line
            Debug.DrawLine(startPos, endPos, Color.red, 5f);

            // Scale prefab before
            BranchPrefab.transform.localScale = new Vector3(branchRadius, length / 2f, branchRadius);

            // Spawning characteristics of the branch 
            Vector3 spawnPosition = Vector3.Lerp(startPos, endPos, 0.5f);
            Vector3 Orientation = Vector3.Normalize(endPos - startPos);
            Quaternion spawnRotation = Quaternion.FromToRotation(new Vector3(0f, 1f, 0f), Orientation);

            // Instantiation of the branch game object
            GameObject createdBranch = Instantiate(BranchPrefab, spawnPosition, spawnRotation, transform);
            //if (parentObject!=null)
            //{
            //    createdBranch.transform.SetParent(parentObject.transform, true);
            //}
            // Renaming that could be more suitable if related to the hierarchy of the corresponding membrane
            createdBranch.name = name;
            if (isLeaf)
            {
                Color vibrantLightGreen = new Color(0.3f, 1f, 0.3f, 1f);
                createdBranch.GetComponent<Renderer>().material.color = vibrantLightGreen;
            }
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
        // Determining if it is a leaf
        bool isLeaf = drawnMembrane.Multiset["Leaf"] == 1;

        // Turtle operations:
        Vector3 startPos = _turtle.State.Position; // Store initial position
        // Turtle rotates, advances and pushes its state into its stack
        _turtle.Rotate(headRotationAngleMultiplier*this.headAngle, _turtle.State.Orientation.head); // Roll
        _turtle.Rotate(leftRotationAngleMultiplier*this.leftAngle, _turtle.State.Orientation.left); // Pitch
        _turtle.Rotate(upRotationAngleMultiplier*this.upAngle, _turtle.State.Orientation.up); // Yaw
        _turtle.Forward(branchLength); // Once rotated, the turtle can advance the desired distance

        // Name of the branch
        string name = "Branch" + drawnMembrane.GetHierarchy();

        // Creation of the branch after the turtle operations:
        this.CreateBranch(startPos, _turtle.State.Position, branchRadius, isLeaf, name);

        // Iterate over the inner membranes repeating this process recursively
        foreach (Membrane innerMembrane in drawnMembrane.InnerMembranes)
        {        
            _turtle.PushState(); // Saving current state
            this.DrawMembrane(innerMembrane); // Draw inner membrane
            _turtle.PopState(); // Going back to previous state
        }
    }
}
