using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Text;

public class PSystemBuilder
{
    // Build a multiset from a string of well structured characters
    // Example: "(A:2)(R:3)(G:65)"
    public static void BuildMultiset(Multiset builtMultiset, string easyMultiset)
    {
        // Variables to store the key and value as they are being built
        StringBuilder newKey = new StringBuilder();
        StringBuilder newValue = new StringBuilder();
        bool buildKey = false;
        bool buildValue = false;

        // Iterate over the characters in easyMultiset to populate the multiset
        foreach (char c in easyMultiset)
        {
            if (c == '(')
            {
                // Start building the key
                buildKey = true;
                buildValue = false;
                newKey.Clear();
                newValue.Clear();
            }
            else if (c == ':')
            {
                // Finished building key, now start building the value
                buildKey = false;
                buildValue = true;
            }
            else if (c == ')')
            {
                // Finished building value, add to multiset
                builtMultiset[newKey.ToString()] = int.Parse(newValue.ToString());
                buildKey = false;
                buildValue = false;
            }
            else if (c == '[')
            {
                break;
            }
            else
            {
                // Append characters to the key or value based on current state
                if (buildKey)
                {
                    newKey.Append(c);
                }
                else if (buildValue)
                {
                    newValue.Append(c);
                }
            }
        }
    }

    public static void BuildMembrane(Membrane builtMembrane, string easyMembrane)
    {
        // Variables to store the multiset and inner membranes as they are being built
        StringBuilder multisetStringBuilder = new();
        StringBuilder innerMembranesStringBuilder = new();
        bool collectMultiset = false;
        bool collectMembranes = false;

        int nOpeningBrackets = 0;
        int nClosingBrackets = 0;

        foreach (char c in easyMembrane)
        {
            if (c == '[')
            {
                nOpeningBrackets += 1;
                if (nOpeningBrackets == 1)
                {
                    collectMultiset = true;
                }
                else if (nOpeningBrackets == 2)
                {
                    collectMultiset = false;
                    collectMembranes = true;
                }
            }
            else if (c == ']')
            {
                nClosingBrackets += 1;
            }
            else if (collectMultiset)
            {
                multisetStringBuilder.Append(c);
            }

            if (nOpeningBrackets == nClosingBrackets)
            {
                collectMembranes = false;
            }
            if (collectMembranes)
            {
                innerMembranesStringBuilder.Append(c);
            }
        }

        // Create multiset and apply it in the built membrane
        Multiset multiset = new(easyMultiset: multisetStringBuilder.ToString());
        builtMembrane.Multiset = multiset;

        // Create list of easy inner membranes
        List<string> easyInnerMembranes = SeparateEasyInnerMembranes(innerMembranesString: innerMembranesStringBuilder.ToString());
        foreach (string easyInnerMembrane in easyInnerMembranes)
        {
            // Create inner membrane 
            Membrane innerMembrane = new(easyMembrane: easyInnerMembrane);
            // Incorporate inner membrane in the built membrane
            builtMembrane.IncorporateMembrane(childMembrane: innerMembrane);
        }
    }

    public static void BuildProduct(PSystemProduct builtProduct, string easyProduct)
    {
        // Add brackets to turn easy product into an easy membrane
        string auxEasyMembrane = "[" + easyProduct + "]";
        // Create an auxiliary membrane from the auxiliary easy membrane
        Membrane auxMembrane = new(easyMembrane: auxEasyMembrane);
        // Use the build auxiliary membrane to build the builtProduct
        builtProduct.ProductMultiset = auxMembrane.Multiset;
        builtProduct.ProductMembranes = auxMembrane.InnerMembranes;
    }

    // From a string containing several membranes and no main multiset (only the multisets in the membranes),
    // extracct the list of separate membranes inside.
    // Example:
    //  - Input: "[(A:2)][(B:1)][(D:12)(A:1)[(C:3)]]"
    //  - Output: "[(A:2)]", "[(B:1)]", "[(D:12)(A:1)[(C:3)]]".
    public static List<string> SeparateEasyInnerMembranes(string innerMembranesString)
    {
        List<string> easyInnerMembranes = new();
        StringBuilder currentMembraneBuilder = new();
        int closedBracketsNeeded = 0;

        foreach (char c in innerMembranesString)
        {
            // Add current character
            currentMembraneBuilder.Append(c);
            if (c == '[')
            {
                // Delay the closing of the brackets
                closedBracketsNeeded += 1;
            }
            else if (c == ']')
            {
                // Accelerate the closing of the brackets
                closedBracketsNeeded-=1;
                // Close the brackets. A valid membrane has been encapsulated in a string builder
                if (closedBracketsNeeded == 0)
                {
                    // Store the easy membrane string in the list of easy inner membranes
                    easyInnerMembranes.Add(currentMembraneBuilder.ToString());
                    // Restart the membrane builder
                    currentMembraneBuilder.Clear();
                }
            }
        }
        return easyInnerMembranes;
    }
}

public class PSystemProduct
{
    // Product multiset associated with this probability
    public Multiset ProductMultiset;
    // Product membranes associated with this probability
    public List<Membrane> ProductMembranes;

    // Constructor for initialization
    public PSystemProduct(string easyProduct = null)
    {
        ProductMultiset = new();
        ProductMembranes = new();
        PSystemBuilder.BuildProduct(builtProduct: this, easyProduct: easyProduct);
    }
}

public class PSystemRule
{
    // Multiset required to apply the rule
    public string ReactiveMultiset;
    // List of PossibleProducts (multiset + membranes and their probabilities)
    // associated with this ReactiveMultiset
    public List<(float Probability, string Product)> PossibleProducts;

    // Constructor for initialization
    public PSystemRule(string reactiveMultiset, List<(float Probability, string Product)> possibleProducts)
    {
        ReactiveMultiset = reactiveMultiset;
        PossibleProducts = possibleProducts;
    }

    // Choose with weighted randomness a given number of PSystemProducts
    public string SelectRandomProduct()
    {
        List<float> probabilities = new();
        List<string> candidates = new();
        float totalProbability = 0f;
        foreach (var tuple in this.PossibleProducts)
        {
            candidates.Add(tuple.Product);
            probabilities.Add(tuple.Probability);
            totalProbability += tuple.Probability;
        }
        // Check if the sum of the probabilities equals 1
        if (totalProbability != 1f)
        {
            throw new ArgumentException("The total probability must be 1.");
        }
        // Generate random float number fom 0 to 1
        float randomPoint = UnityEngine.Random.Range(0f, totalProbability);
        // Calculate the chosen candidate 
        for (int i = 0; i < probabilities.Count; i++)
        {
            if (randomPoint < probabilities[i])
            {
                return candidates[i];
            }
            randomPoint -= probabilities[i];
        }
        throw new Exception("Random number problem"); 
    }

    // Check is this rule can be applied to a multiset
    public bool CanBeAppliedTo(Multiset multiset)
    {
        Multiset builtReactiveMultiset = new(easyMultiset: this.ReactiveMultiset);
        return multiset.CanCombineWith(builtReactiveMultiset.GetInverse());
    }

    // Copy method
    public PSystemRule Copy()
    {
        // Create a deep copy of the object
        return new PSystemRule(
            this.ReactiveMultiset, // Safe to copy directly (immutable string)
            new List<(float Probability, string Product)>(this.PossibleProducts) // Copy the list
        );
    }
}

public class Multiset
{
    public Dictionary<string, int> Counter;

    public Multiset(string easyMultiset = null, Dictionary<string, int> counter = null)
    {
        if (easyMultiset != null)
        {
            this.Counter = new Dictionary<string, int>();
            PSystemBuilder.BuildMultiset(this, easyMultiset);
        }
        else
        {
            this.Counter = counter ?? new Dictionary<string, int>();
        }
    }

    // Modify the indexig setter and getter to handle unknown keys
    public int this[string key]
    {
        get 
        {
            // unknown_key:0
            if (!this.Counter.ContainsKey(key))
            {
                return 0;
            }
            // known_key:value
            return this.Counter[key];
        }
        set
        {
            // Add unknown_key:value
            if (!this.Counter.ContainsKey(key))
            {
                this.Counter.Add(key, value);
            }
            else
            {
                // known_key:value
                this.Counter[key] = value;
            }
        }
    }

    // Add the count of the other_multiset to this one
    public void CombineWith(Multiset other_multiset)
    {
        if (this.CanCombineWith(other_multiset))
        {
            foreach (var item in other_multiset.Counter)
            {
                this[item.Key]+=item.Value;
                if (this[item.Key]==0)
                {
                    this.Remove(item.Key);
                }
            } 
        }
    }

    // Check if this multiset can combine with another
    public bool CanCombineWith(Multiset other_multiset)
    {
        foreach (var item in other_multiset.Counter)
        {
            int difference = this[item.Key] + item.Value;
            if (difference < 0)
            {
                return false;
            }
        } 
        return true;
    }

    // Inverse multiset (change sign)
    public Multiset GetInverse()
    {
        Multiset NegMultiset = this.Copy();
        foreach (var item in this.Counter)
        {
            NegMultiset[item.Key] = -item.Value;
        }
        return NegMultiset;
    }

    // Forward the ContainsKey method to the Dictionary (Counter)
    public bool ContainsKey(string key)
    {
        return Counter.ContainsKey(key);
    }

    // Remove key. To be used when key:0
    public void Remove(string key)
    {
        if (Counter.ContainsKey(key))
        {
            Counter.Remove(key);
        }
        else
        {
            Debug.LogWarning($"{key} does not exist.");
        }
    }

    // Copy the multiset
    public Multiset Copy()
    {
        // Create a new Multiset with a copy of the Counter dictionary
        return new Multiset(easyMultiset: this.ToString());
    }

    // Override ToString() to represent Multiset
    public override string ToString()
    {
        List<string> items = new List<string>();
        foreach (var kv in Counter)
        {
            items.Add($"({kv.Key}:{kv.Value})");
        }
        return $"{string.Join("", items)}";
    }
}

public class Membrane
{
    public Membrane ParentMembrane;
    public List<Membrane> InnerMembranes;
    public Multiset Multiset;
    public List<PSystemRule> Rules;
    public bool InheritingRules;

    public Membrane(string easyMembrane = null, Membrane parentMembrane = null, List<Membrane> innerMembranes = null, Multiset multiset = null, List<PSystemRule> rules = null, bool inheritingRules = true)
    {
        this.ParentMembrane = parentMembrane;    
        this.Rules = rules ?? new List<PSystemRule>();
        this.InheritingRules = inheritingRules;
        if (easyMembrane != null)
        {
            this.InnerMembranes = new List<Membrane>();   
            this.Multiset = new Multiset();
            PSystemBuilder.BuildMembrane(this, easyMembrane);
        }
        else
        {
            this.InnerMembranes = innerMembranes ?? new List<Membrane>();   
            this.Multiset = multiset ?? new Multiset();
        }
    }

    // Method to recursively calculate the hierarchy of the membrane.
    // Skin membrane has hierarchy 1
    public int GetHierarchy()
    {
        if (this.ParentMembrane == null)
        {
            return 1;
        }
        return this.ParentMembrane.GetHierarchy() + 1;
    }
    
    // Make every inner membrane follow the rules of this membrane
    public void TeachRules()
    {
        foreach (Membrane innerMembrane in this.InnerMembranes)
        {
            innerMembrane.Rules = this.Rules;
            innerMembrane.TeachRules();
        }
    }

    // Add a membrane to the list of InnerMembranes
    // Make this membrane its parent
    public void IncorporateMembrane(Membrane childMembrane)
    {
        this.InnerMembranes.Add(childMembrane);
        childMembrane.ParentMembrane = this;

        if (this.InheritingRules)
        {
        childMembrane.Rules = this.Rules;
        childMembrane.TeachRules();
        }
    }

    // Combine the multiset of this membrane with the productMultiset
    public void UpdateMultiset(Multiset productMultiset)
    {
        this.Multiset.CombineWith(productMultiset);
    }

    // Check if there is any rule (from a given list) that can be applied to the current membrane
    public bool CanEvolve(List<PSystemRule> possibleRules)
    {
        foreach (PSystemRule rule in possibleRules)
        {
            if (rule.CanBeAppliedTo(this.Multiset))
            {
                return true;
            }
        }
        return false;
    }

    // Apply rules to this membrane and its inner ones
    public void Evolve(int nIterations = 1)
    {
        for (int iter = 0; iter < nIterations; iter++)
        {
            Debug.Log($"Evolving membrane: {this.ToString()}");
            Debug.Log($"Hierarchy: {this.GetHierarchy()}");
            Debug.Log($"Iteration: {iter + 1}");
            Debug.Log($"Number of rules: {this.Rules.Count}");

            // Maybe shuffle rules first to ensure fair choices
            Multiset totalProducedMultiset = new();
            List<Membrane> totalProducedMembranes = new();
            List<PSystemRule> usableRules = new();
            foreach (PSystemRule ruleToCopy in this.Rules)
            {
                usableRules.Add(ruleToCopy.Copy());
            }
            // Use reactives in a maximally parallel way. 
            //  - Maximally: Apply rules randomly until none of them can be used due to lack of needed reactives
            //  - Paralelly: Save products (multisets and membranes) to incorporate them later 
            int i = 0;
            while (this.CanEvolve(usableRules))
            {
                if (usableRules[i].CanBeAppliedTo(this.Multiset))
                {
                    // Get specific rule to use
                    PSystemRule usingRule = usableRules[i];
                    // Substract used objects from the current multiset
                    Multiset reactiveMultiset = new(easyMultiset: usingRule.ReactiveMultiset);
                    this.UpdateMultiset(reactiveMultiset.GetInverse());

                    // Get the product randomly produced by the rule (through its probabilities)
                    PSystemProduct pSystemProduct = new(easyProduct: usingRule.SelectRandomProduct());
                    Multiset productMultiset = pSystemProduct.ProductMultiset;
                    List<Membrane> productMembranes = pSystemProduct.ProductMembranes;

                    // Add products to the totalProducedMultiset
                    totalProducedMultiset.CombineWith(productMultiset);
                    // Add produced membranes to the list of produced membranes
                    foreach (Membrane membrane in productMembranes)
                    {
                        totalProducedMembranes.Add(membrane.Copy());
                    }
                    // Prepare index to handle the next rule in usableRules
                    i++;
                }
                else
                {
                    // Remove unusable rule from the list of usable rules
                    usableRules.RemoveAt(i);
                }

                // Safety measure to avoid divide by zero error
                if (usableRules.Count == 0)
                {
                    break; // No more usable rules
                }
                // Iterate cyclically over usableRules adapting to the changing length of the list
                i%=usableRules.Count;
            }
            Debug.Log("Reactives available for iteration have been depleted");

            // Apply rules to inner membranes
            foreach (Membrane innerMembrane in this.InnerMembranes)
            {
                Debug.Log($"Found inner membrane: {innerMembrane.ToString()}");
                innerMembrane.Evolve();
            }

            // Incorporate the products
            //  - Membranes
            foreach (Membrane incorporatingMembrane in totalProducedMembranes)
            {
                this.IncorporateMembrane(incorporatingMembrane);
            }
            //  - Multiset
            this.UpdateMultiset(totalProducedMultiset);
        }
        Debug.Log($"Result of the iteration: {this.ToString()}");
    }
    
    // Method to recursively create a deep copy of the Membrane
    public Membrane Copy()
    {
        // Copy the Multiset
        Multiset MultisetCopy = this.Multiset.Copy();
        // Create copy of the top level of this membrane 
        // (parentMembrane = null, innerMembranes = null)
        Membrane MembraneCopy = new Membrane(multiset: MultisetCopy, rules: this.Rules);
        // Add inner structure of membranes
        // Update parent membranes
        foreach (Membrane childMembrane in this.InnerMembranes)
        {
            MembraneCopy.IncorporateMembrane(childMembrane.Copy());
        }
        // Return copy
        return MembraneCopy;
    }

    // Override ToString() to represent Membrane
    public override string ToString()
    {
        List<string> items = new();
        // Beginning of the membrane
        items.Add("[");
        // Add the multiset of the membrane
        items.Add($"{this.Multiset.ToString()}");
        foreach (Membrane membrane in this.InnerMembranes)
        {
            // Add inner membranes
            items.Add($"{membrane.ToString()}");
        }
        // End of the membrane
        items.Add("]");
        return $"{string.Join("", items)}";
    }
}
