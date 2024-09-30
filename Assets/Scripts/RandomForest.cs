using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class RandomForest : MonoBehaviour
{
    public Plant plant;
    public GameObject branchPrefab; 
    public int nPlants;
/*
        {
      "Name": "plant_e",
      "Axiom": "X",
      "Rules": [
        {
          "predecessor": "X",
          "successor": "F[+X][-X]FX"
        },
        {
          "predecessor": "F",
          "successor": "FF"
        }
      ],
      "Angle": 25.7
    },
*/
    void Start()
    {
        LSystemLoader lSystemLoader = new LSystemLoader();
        for (int i = 0; i < nPlants; i++)
        {
            /*
            List<Rule> rules = new List<Rule>();
            rules.Add(new Rule { predecessor = 'X', successor = "F[+X][-X]FX"});
            rules.Add(new Rule { predecessor = 'F', successor = "FF" });
            LSystem lsystem = new LSystem("SampleTree",'X', rules, 25.7f);
            */
            LSystem lsystem = lSystemLoader.LoadLSystem("", "LSystems2D", true);
            Vector3 spawnPosition = new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f));
            int nIterations = Random.Range(3, 6);
            Debug.Log("Iterations: " + nIterations);
            SpawnTree(lsystem, spawnPosition, nIterations);
        }
    }

    void SpawnTree(LSystem lsystem, Vector3 spawnPosition, int nIterations)
    {
        // Set up L-System and Turtle
        plant.LSystem = lsystem;
        // Instantiate turle orientation and object
        TurtleOrientation initOrientation = new(Vector3.up, Vector3.left, Vector3.forward);
        plant.Turtle = new Turtle(spawnPosition, initOrientation);
        //plant.LSystem.Angle = 25f;

        // Set up branch parameters
        plant.BranchPrefab = branchPrefab;
        plant.BranchRadius = 0.4f;
        plant.BranchLength = 1f;
 
        // Grow the plant with nIterations
        plant.Grow(nIterations);
    }
}
