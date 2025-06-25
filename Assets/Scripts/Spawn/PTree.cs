using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.Subsystems;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using UnityEngine.UI;
using System.Linq.Expressions;
using Unity.VisualScripting;


public class PTree : MonoBehaviour
{
    //public TextMeshPro output;
    public TMP_InputField Axiom;
    public TMP_InputField NumIterations;
    public TMP_InputField headAngle;
    public TMP_InputField leftAngle;
    public TMP_InputField upAngle;
    public TMP_InputField Seed;
    public TextMeshProUGUI SizeText;
    public TextMeshProUGUI NBranchesText;
    public TextMeshProUGUI NLeavesText;
    public GameObject PVisualizerPrefab;
    //public string EasyMembrane;

    // Flags
    public bool CanRotate = false;
    public bool Rotating = false;

    // Tree parent gameobject
    public GameObject PVisualizerObject;
    // Current tree membrane
    public Membrane TreeMembrane = null;
    public PSystem TreePSystem = null;

    public void Start()
    {
        List<string> selectedParams = RuleCollector.SelectedParams;
        headAngle.text = selectedParams[0];
        leftAngle.text = selectedParams[1];
        upAngle.text = selectedParams[2];
        Axiom.text = selectedParams[3];
    }
    public void InitTreeMembrane()
    {
        // Get selected rules
        List<PSystemRule> rules = RuleCollector.SelectedRules;
        this.TreePSystem = new(membraneString:this.Axiom.text, rules: rules);
    }

    public void SetRandomSeed()
    {
        string seedText = this.Seed.text;
        if (int.TryParse(seedText, out int seed))
        {
            LogManager.currentLogText = $"Seed set to: {seed}";
        }
        else
        {
            seed = System.Environment.TickCount; // or use DateTime.Now.Millisecond, etc.
            seed = Mathf.Abs(seed % 999);            
            LogManager.currentLogText = $"No valid seed entered — using generated seed: {seed}";
        }
        UnityEngine.Random.InitState(seed);
        this.Seed.text = seed.ToString();
    }

    public void EvolveAndSpawn()
    {
        try
        {
            this.InitTreeMembrane();
        }
        catch
        {
            LogManager.currentLogText = $"Invalid axiom";
            return;
        }
        this.SetRandomSeed();
        try
        {
            this.TreePSystem.EvolveMembrane(nIterations: int.Parse(this.NumIterations.text));
        }
        catch
        {
            LogManager.currentLogText = $"Error evolving. Check number of iterations.";
            return;
        }
        try
        {
            this.SpawnTree(treeMembrane: this.TreePSystem.SystemMembrane, spawnPosition: new(0f, 0f, 0f));
        }
        catch
        {
            LogManager.currentLogText = $"Error spawning. Check angles.";
            return;
        }
    }

    public void SpawnTree(Membrane treeMembrane, Vector3 spawnPosition)
    {
        // Instantiate PVisualizer object. This is a game object that runs the PSystemVisualizer script.
        this.PVisualizerObject = Instantiate(PVisualizerPrefab, spawnPosition, Quaternion.identity, transform);
        // Give a name to the PVisualizer object, which will represent the tree as a group
        this.PVisualizerObject.name = "PTree";

        // Get PSystemVisualizer object from PVisualizerObject
        PSystemVisualizer PVisualizer = this.PVisualizerObject.GetComponent<PSystemVisualizer>();

        // Instantiate turle orientation and object
        TurtleOrientation initOrientation = new(Vector3.up, Vector3.left, Vector3.forward);
        PVisualizer.Turtle = new Turtle(spawnPosition, initOrientation);
        PVisualizer.headAngle = float.Parse(this.headAngle.text, CultureInfo.InvariantCulture);
        PVisualizer.leftAngle = float.Parse(this.leftAngle.text, CultureInfo.InvariantCulture);
        PVisualizer.upAngle = float.Parse(this.upAngle.text, CultureInfo.InvariantCulture);

        // Draw the membrane
        PVisualizer.DrawMembrane(drawnMembrane: treeMembrane);

        // Show data on screen
        this.NBranchesText.text = $"Branches: {PVisualizer.nBranches}";
        this.NLeavesText.text = $"Leaves: {PVisualizer.nLeaves}";
        Vector3 treeDimensions = PVisualizer.Turtle.ExploredSpace.GetDimensions();
        string dimensionsString = string.Format(CultureInfo.InvariantCulture,"Tree size: ({0:F2}, {1:F2}, {2:F2})", treeDimensions.x, treeDimensions.y, treeDimensions.z);
        this.SizeText.text = dimensionsString;

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
            this.PVisualizerObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
}

public class TimerSummary : MonoBehaviour
{
    private Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>();
    private Dictionary<string, long> totalTimes = new Dictionary<string, long>();
    private Dictionary<string, int> callCounts = new Dictionary<string, int>();

    public void StartTimer(string label)
    {
        if (!timers.ContainsKey(label))
            timers[label] = new Stopwatch();

        timers[label].Restart();
    }

    public void StopTimer(string label)
    {
        if (timers.ContainsKey(label))
        {
            timers[label].Stop();
            long elapsed = timers[label].ElapsedMilliseconds;

            if (!totalTimes.ContainsKey(label))
                totalTimes[label] = 0;
            totalTimes[label] += elapsed;

            if (!callCounts.ContainsKey(label))
                callCounts[label] = 0;
            callCounts[label]++;
        }
    }

    public string GetSummary()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Timer Summary ===");
        foreach (var kvp in totalTimes)
        {
            string label = kvp.Key;
            long total = kvp.Value;
            int count = callCounts[label];
            float avg = (float)total / count;
            sb.AppendLine($"{label}: Total = {total} ms, Calls = {count}, Avg = {avg:F2} ms");
        }
        return sb.ToString();
    }
}

