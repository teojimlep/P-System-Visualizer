using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Rule
{
    public string predecessor;
    public string successor;
}

// This is a standalone script. Since it is not attached
// to any GameObject, it doesn't need to be MonoBehaviour.
[Serializable]
public class LSystem
{
    // Declaration of the fields of the class.
    // These are accesible from other classes.
    public string Name;
    public string Axiom;
    public List<Rule> Rules;
    public float Angle;

    [NonSerialized]
    public string CurrentString;

    // Constructor of the class.
    // Reading the axiom and dictionary of rules when an instance is created.
    public LSystem(string name, string axiom, List<Rule> rules, float angle)
    {
        this.Name = name;
        this.Axiom = axiom;
        this.Rules = rules;
        this.Angle = angle;
        this.CurrentString = Axiom;
    }
    // Rewrite method.
    // This method uses the rules of the LSystem to rewrite currentString.
    public void Rewrite(bool log)
    {
        Dictionary<char, string> rulesDictionary = new Dictionary<char, string>();
        foreach (Rule rule in Rules)
        {
            rulesDictionary[rule.predecessor[0]] = rule.successor;
        }

        string rewrittenString = "";

        foreach (char c in CurrentString)
        {
            if (rulesDictionary.ContainsKey(c))
            {
                rewrittenString += rulesDictionary[c];
            }
            else
            {
                rewrittenString += c;
            }
        }
        CurrentString = rewrittenString;
        if (log)
        {
            Debug.Log("Current String: " + CurrentString);
        }
    }
    // IterRewrite method.
    // This metod rewrites for a given number of iterations.
    public void IterRewrite(int iterations, bool log)
    {
        CurrentString = Axiom;
        for (int i = 0; i < iterations; i++)
        {
            Rewrite(log);
        }
    }
}