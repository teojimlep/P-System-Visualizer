using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.Subsystems;

public class PTree : MonoBehaviour
{
    //public TextMeshPro output;
    public TMP_InputField InputEasyMembrane;
    public TMP_InputField NumIterations;
    public GameObject PVisualizerPrefab;
    //public string EasyMembrane;

    // Rotation flag
    public bool CanRotate = false;
    public bool Rotating = false;
    public int Example = 0;
    // Tree parent gameobject
    public GameObject PVisualizerObject;
    // Current tree membrane
    public Membrane TreeMembrane = null;

    public void Start()
    {
        Dictionary<int, string> examples = new Dictionary<int, string>();
        examples.Add(1, "[(L:1)(E:1)[(L:1)(E:1)(W:1)(F:1)(BL:1)(BS1:1)]]");
        examples.Add(2, "[(L:1)(T:1)(B1:1)(B2:1)]");
        examples.Add(3, "[(L:1)(Bp:1)(Bm:1)(Ba:1)(Bc:1)]");

        InputEasyMembrane.text = examples[Example];
    }

    public List<PSystemRule> GetRules()
    {
        List<PSystemRule> rules = new();
        // Create a rule that reacts with a multiset and produces a product
        /*
        // Basic symmetrical tree
        List<(float Probability, string Product)> possibleProducts = new()
        {
            (1f, "(F:1)(W:1)[(f:1)(w:1)(+:1)][(f:1)(w:1)(-:1)]")
        };
        PSystemRule rule = new(reactiveMultiset: "(f:1)(w:1)", possibleProducts);
        List<PSystemRule> rules = new() { rule };
        */
        
        // The following rules are intended for the initial membrane; [(L:1)(E:1)[(L:1)(E:1)(W:1)(F:1)(BL:1)(BS1:1)]]
        if (Example == 1)
        {
            List<(float Probability, string Product)> possibleProductBS1 = new()
            {
                (0.75f, "[(L:1)(E:1)(W:1)(F:1)(BS2:1)(BR:1)]"),
                (0.25f, "(&:1)[(L:1)(E:1)(W:1)(F:1)(BS2:1)(BR:1)]")
            };
            List<(float Probability, string Product)> possibleProductBS2 = new()
            {
                (0.75f, "[(L:1)(E:1)(W:1)(F:1)(BS1:1)(BL:1)]"),
                (0.25f, "(^:1)[(L:1)(E:1)(W:1)(F:1)(BS1:1)(BL:1)]")
            };
            List<(float Probability, string Product)> possibleProductBL = new()
            {
                (0.5f, "[(+:1)(L:1)(E:1)(W:1)(F:1)(BS1:1)(BL:1)]"),
                (0.5f, "[(+:1)(L:1)(E:1)(W:1)(F:1)(BS1:1)(BL:1)(&:1)]")
            };
            List<(float Probability, string Product)> possibleProductBR = new()
            {
                (0.5f, "[(-:1)(L:1)(E:1)(W:1)(F:1)(BS1:1)(BL:1)]"),
                (0.5f, "[(-:1)(L:1)(E:1)(W:1)(F:1)(BS1:1)(BL:1)(^:1)]")
            };
            List<(float Probability, string Product)> possibleProductL = new()
            {
                (1f, "(L:1)(F:1)")
            };
            List<(float Probability, string Product)> possibleProductE = new()
            {
                (1f, "(E:1)(W:1)")
            };
            PSystemRule ruleBS1 = new(reactiveMultiset: "(BS1:1)", possibleProductBS1);
            PSystemRule ruleBS2 = new(reactiveMultiset: "(BS2:1)", possibleProductBS2);
            PSystemRule ruleBL = new(reactiveMultiset: "(BL:1)", possibleProductBL);
            PSystemRule ruleBR = new(reactiveMultiset: "(BR:1)", possibleProductBR);
            PSystemRule ruleL = new(reactiveMultiset: "(L:1)", possibleProductL);
            PSystemRule ruleE = new(reactiveMultiset: "(E:1)", possibleProductE);
            rules = new() { ruleBS1, ruleBS2, ruleBL, ruleBR, ruleL, ruleE};
        }
        else if (Example == 2)
        {
            // The following rules are intended for the initial membrane; [(L:1)(T:1)(B1:1)(B2:1)]
            List<(float Probability, string Product)> possibleProductT = new()
            {
                (1f, "[(L:1)(T:1)(B1:1)(B2:1)]")
            };
            List<(float Probability, string Product)> possibleProductB1 = new()
            {
                (0.5f, "[(+:1)(L:1)(T:1)(B1:1)(B2:1)]"),
                (0.5f, "[(-:1)(L:1)(T:1)(B1:1)(B2:1)]")
            };  
            List<(float Probability, string Product)> possibleProductB2 = new()
            {
                (0.5f, "[(&:1)(L:1)(T:1)(B1:1)(B2:1)]"),
                (0.5f, "[(^:1)(L:1)(T:1)(B1:1)(B2:1)]")
            };
            List<(float Probability, string Product)> possibleProductL = new()
            {
                (1f, "(L:1)(W:1)(F:1)")
            };
            PSystemRule ruleT = new(reactiveMultiset: "(T:1)", possibleProductT);
            PSystemRule ruleB1 = new(reactiveMultiset: "(B1:1)", possibleProductB1);
            PSystemRule ruleB2 = new(reactiveMultiset: "(B2:1)", possibleProductB2);
            PSystemRule ruleL = new(reactiveMultiset: "(L:1)", possibleProductL);
            rules = new() {ruleT, ruleB1, ruleB2, ruleL};
        }
        else if (Example == 3)
        {
            // 4 branches
            // The following rules are intended for the initial membrane; [(L:1)(Bp:1)(Bm:1)(Ba:1)(Bc:1)]
            List<(float Probability, string Product)> possibleProductBp = new()
            {
                (1f, "[(+:1)(L:1)(Bp:1)(Bm:1)(Ba:1)(Bc:1)]")
            };
            List<(float Probability, string Product)> possibleProductBm = new()
            {
                (1f, "[(-:1)(L:1)(Bp:1)(Bm:1)(Ba:1)(Bc:1)]")
            };  
            List<(float Probability, string Product)> possibleProductBa = new()
            {
                (1f, "[(&:1)(L:1)(Bp:1)(Bm:1)(Ba:1)(Bc:1)]")
            };
            List<(float Probability, string Product)> possibleProductBc = new()
            {
                (1f, "[(^:1)(L:1)(Bp:1)(Bm:1)(Ba:1)(Bc:1)]")
            };  
            List<(float Probability, string Product)> possibleProductL = new()
            {
                (1f, "(L:1)(W:1)(F:1)")
            };
            PSystemRule ruleBp = new(reactiveMultiset: "(Bp:1)", possibleProductBp);
            PSystemRule ruleBm = new(reactiveMultiset: "(Bm:1)", possibleProductBm);
            PSystemRule ruleBpa = new(reactiveMultiset: "(Bp:1)", possibleProductBa);
            PSystemRule ruleBa = new(reactiveMultiset: "(Ba:1)", possibleProductBa);
            PSystemRule ruleBc = new(reactiveMultiset: "(Bc:1)", possibleProductBc);
            PSystemRule ruleL = new(reactiveMultiset: "(L:1)", possibleProductL);
            rules = new() {ruleBp, ruleBm, ruleBa, ruleBc, ruleL, ruleBpa};
        }
        return rules;
    }
    
    public void InitTreeMembrane()
    {
        List<PSystemRule> rules =  this.GetRules();
        this.TreeMembrane = new(easyMembrane: this.InputEasyMembrane.text, rules: rules);
    }

    public void EvolveAndSpawn()
    {
        this.InitTreeMembrane();
        this.TreeMembrane.Evolve(nIterations: int.Parse(this.NumIterations.text));
        Vector3 spawnPosition = new(0f,-4f,0f);
        this.SpawnTree(treeMembrane: this.TreeMembrane, spawnPosition: spawnPosition);
    }

    public void GrowNaturally()
    {
        if (this.TreeMembrane != null)
        {
            this.DestroyTree();
        }
        else
        {
            this.InitTreeMembrane();
        }
        this.TreeMembrane.Evolve();
        Vector3 spawnPosition = new(0f,-4f,0f);
        this.SpawnTree(treeMembrane: this.TreeMembrane, spawnPosition: spawnPosition);
    }

    public void SpawnTree(Membrane treeMembrane, Vector3 spawnPosition)
    {
        // Instantiate PVisualizer object. This is a game object that runs the PSystemVisualizer script.
        this.PVisualizerObject = Instantiate(PVisualizerPrefab, spawnPosition, Quaternion.identity, transform);
        // Give a name to the PVisualizer object, which will represent the tree as a group
        this.PVisualizerObject.name = "PTree";
        // Get PSystemVisualizer object from PVisualizerObject
        PSystemVisualizer PVisualizer = this.PVisualizerObject.GetComponent<PSystemVisualizer>();
    
        // Prepare the visualizer
        // Instantiate turle orientation and object
        TurtleOrientation initOrientation = new(Vector3.up, Vector3.left, Vector3.forward);
        PVisualizer.Turtle = new Turtle(spawnPosition, initOrientation);
        // Draw the membrane
        //Debug.Log($"Drawing membrane: {treeMembrane.ToString()}");
        PVisualizer.DrawMembrane(drawnMembrane: treeMembrane);

        // Once the tree has been completely generated, it can rotate if desired
        this.CanRotate = true;
    }

    public void RotateTree()
    {
        if (this.CanRotate)
        { 
            this.Rotating = !this.Rotating;
        }
    }

    public void DestroyTree()
    {
        Destroy(PVisualizerObject);
        this.CanRotate = false;
        this.Rotating = false;
    }

    public void Update()
    {
        if (this.Rotating)
        {
            //Debug.Log("I am suposed to be rotating");
            this.PVisualizerObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
}
