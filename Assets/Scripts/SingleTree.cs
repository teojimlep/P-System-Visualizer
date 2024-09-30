using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class SingleTree : MonoBehaviour
{
    public GameObject plantPrefab;
    public int nIterations;
    public int numTrees {get; private set;}

/* JSON:
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

    is equivalent to:

    List<Rule> rules = new List<Rule>();
    rules.Add(new Rule { predecessor = 'X', successor = "F[+X][-X]FX"});
    rules.Add(new Rule { predecessor = 'F', successor = "FF" });
    LSystem lsystem = new LSystem("SampleTree",'X', rules, 25.7f);  
*/
    void Start()
    {
      /*
        LSystemLoader lSystemLoader = new LSystemLoader();
        LSystem lsystem = lSystemLoader.LoadLSystem("3D-HilbertCurve", "LSystems2D", false);

        //List<Rule> rules = new List<Rule>();
        //rules.Add(new Rule { predecessor = "X", successor = "^<XF^<XFX-F^>>XFX&F+>>XFX-F>X->"});
        //LSystem lsystem = new LSystem("SampleTree","X", rules, 90.0f); 
        Vector3 spawnPosition = new Vector3(0f,0f,0f);
        
        SpawnTree(lsystem, spawnPosition, nIterations);
        */
    }

    void SpawnTree(LSystem lsystem, Vector3 spawnPosition, int nIterations)
    {
      // Instantiate plant object
      GameObject plantObject = Instantiate(plantPrefab, spawnPosition, Quaternion.identity, transform);
      numTrees+=1;
      plantObject.name = "Tree_" + numTrees.ToString();
      // Get plant object from plant GameObject
      Plant plant = plantObject.GetComponent<Plant>();
      // Set up L-System and Turtle
      plant.LSystem = lsystem;
      // Instantiate turle orientation and object
      TurtleOrientation initOrientation = new(Vector3.up, Vector3.left, Vector3.forward);
      plant.Turtle = new Turtle(spawnPosition, initOrientation);
      // Make the plant grow for nIterations
      plant.Grow(nIterations);
    }
}
