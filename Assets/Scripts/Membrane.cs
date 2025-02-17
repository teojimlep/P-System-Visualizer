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
using TMPro;

public class PSystemBuilder

{
    // Given an input string, a multiset entry is extracted from the string.
    // Example: 
    //      - Input:  (R:3)(G:65)[(A:2)][(B:1)][(D:12)(A:1)[(C:3)]]
    //      - Outputs:  (R:3)
    //                  (G:65)[(A:2)][(B:1)][(D:12)(A:1)[(C:3)]]
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
                builtMultiset[newKey.ToString()] += int.Parse(newValue.ToString());
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
    
    public static (string, string) ExtractMultisetEntry(string input)
    {
        // Check if the input string is non-empty and it starts with '('
        if (string.IsNullOrEmpty(input) || input[0] != '(')
        {
            throw new Exception("A multiset entry must start with (");
        }

        // Initialize the multiset entry to be extracted
        string multisetEntry = "";

        // Iterate over the characters of the input string
        foreach (char c in input)
        {
            // Build the mutiset entry
            multisetEntry += c;
            if (c == ')')
            {
                break;
            }
        }

        //Debug.LogWarning($"Mentry: {multisetEntry}.");
        // Validate multiset entry
        if (!multisetEntry.Contains(':'))
        {
            throw new Exception("Missing ':' character to separate key:value");
        }
        if (!multisetEntry.Contains(')'))
        {
            throw new Exception("Parentheses mismatch");
        }
        return (multisetEntry, input.Substring(multisetEntry.Length));
    }


    // Given an input string, a membrane is extracted from the string.
    // Example: 
    //      - Input:  [(A:2)][(B:1)][(D:12)(A:1)[(C:3)]]
    //      - Outputs:  [(A:2)]
    //                  [(B:1)][(D:12)(A:1)[(C:3)]]
    public static (string, string) ExtractMembrane(string input)
    {
        // Check if the input string is non-empty and it starts with '['
        if (string.IsNullOrEmpty(input) || input[0] != '[')
        {
            throw new Exception("A membrane must start with [");
        }

        // Initialize a bracket counter, which increases with ']' and decreases with '['. 
        // When brackets are compensated, the counter will be zero and the membrane will be extracted.
        int bracketCounter = 0;
        // Initialize the membrane to be extracted
        string membrane = "";

        // Iterate over the characters of the input string
        for (int i = 0; i < input.Length; i++)
        {
            // When the bracket counter gets to zero, we are dealing with the closing of
            // the extracted membrane.
            if (bracketCounter == 0 && i > 0)
            {
                if (input[i] == '_')
                {
                    // A label (k) could be found it the end of the membrane is "]_k"
                    if (i + 1 < input.Length && !"[(".Contains(input[i + 1]))
                    {
                        membrane += "_" + input[i + 1];
                    }
                    else
                    {
                        throw new Exception("Invalid label character");
                    }
                }
                // After looking for a label, we are ready to extract the membrane
                // As a second output, the rest of the string is provided 
                return (membrane, input.Substring(membrane.Length));
            }
            // If the bracket counter is not zero yet, the membrane keeps growing
            membrane += input[i];
            
            // The bracket counter gets updated
            if (input[i] == '[')
                bracketCounter++;
            else if (input[i] == ']')
                bracketCounter--;
        }
        return (membrane, input.Substring(membrane.Length));
        //throw new Exception("Bracket mismatch");
    }

    public static (char label, List<string> multisets, List<string> innerMembranes) GetMembraneContent(string membrane)
    {
        if (string.IsNullOrEmpty(membrane))
        {
            throw new ArgumentException("Membrane string cannot be empty.");
        }

        string content;
        char label = '0'; // Default label

        if (membrane[^1] == ']') // Último carácter es ']'
        {
            content = membrane[1..^1];
        }
        else if (membrane.Length > 2 && membrane[^2] == '_') // Últimos caracteres son "_X"
        {
            if (!"[](".Contains(membrane[^1]))
            {
                content = membrane[1..^3];
                label = membrane[^1];
            }
            else
            {
                throw new Exception("Invalid label character");
            }
        }
        else
        {
            throw new Exception("Invalid membrane format. Membranes must end in ] or _tag");
        }

        List<string> multisets = new List<string>();
        List<string> innerMembranes = new List<string>();

        while (!string.IsNullOrEmpty(content))
        {
            if (content[0] == '(')
            {
                (string extractedMultiset, string remainingContent) = ExtractMultisetEntry(content);
                multisets.Add(extractedMultiset);
                content = remainingContent;
            }
            else if (content[0] == '[')
            {
                (string extractedMembrane, string remainingContent) = ExtractMembrane(content);
                innerMembranes.Add(extractedMembrane);
                content = remainingContent;
            }
            else
            {
                throw new Exception("Unexpected character in membrane content");
            }
        }

        return (label, multisets, innerMembranes);
    }

    public static void BuildMembrane(Membrane builtMembrane, string easyMembrane)
    {
        var (label, multisetEntries, easyInnerMembranes) = PSystemBuilder.GetMembraneContent(easyMembrane);
        Console.WriteLine($"Label: {label}");
        // Append multisetEntries, which are strings, into a single string
        string easyMultiset = string.Join("", multisetEntries);
        Multiset multiset = new(easyMultiset: easyMultiset);
        builtMembrane.Multiset = multiset;    
        builtMembrane.Label = label;

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
        //Debug.Log($"Generating product multiset {builtProduct.ProductMultiset}");
        builtProduct.ProductMembranes = auxMembrane.InnerMembranes;
        //foreach (Membrane innerMembrane in builtProduct.ProductMembranes) 
        //{
        //    Debug.Log($"Generating product membrane {innerMembrane}");
        //}
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
    public char Label;

    public Membrane(string easyMembrane = null, Membrane parentMembrane = null, List<Membrane> innerMembranes = null, Multiset multiset = null)
    {
        this.ParentMembrane = parentMembrane;    
        if (easyMembrane != null)
        {
            this.InnerMembranes = new List<Membrane>();   
            this.Multiset = new Multiset();
            this.Label = '0';
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

    // Add a membrane to the list of InnerMembranes
    // Make this membrane its parent
    public void IncorporateMembrane(Membrane childMembrane)
    {
        this.InnerMembranes.Add(childMembrane);
        childMembrane.ParentMembrane = this;
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

    // Method to recursively create a deep copy of the Membrane
    public Membrane Copy()
    {
        Membrane membraneCopy = new(easyMembrane: this.ToString());
        return membraneCopy;
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
        items.Add("_");
        items.Add(Label.ToString());
        return $"{string.Join("", items)}";
    }
}

public class PSystem
{
    public string MembraneString;
    public List<PSystemRule> Rules;
    
    public PSystem(string membraneString, List<PSystemRule> rules)
    {
        this.MembraneString = membraneString;
        this.Rules = rules;
    }

    public Membrane GetMembrane(string membraneString)
    {
        if (membraneString == null)
        {
            membraneString = this.MembraneString;
        }
        Membrane membrane = new(easyMembrane:membraneString);        
        return membrane;
    }

    public void EvolveMembrane(Membrane membrane = null, int nIterations = 1, int depth = 0)
    {
        if (membrane == null)
        {
            membrane = this.GetMembrane(this.MembraneString);
        }
        for (int iter = 0; iter < nIterations; iter++)
        {
            Multiset totalProducedMultiset = new();
            List<Membrane> totalProducedMembranes = new();
            List<PSystemRule> usableRules = new();
            foreach (PSystemRule ruleToCopy in this.Rules)
            {
                if (ruleToCopy.CanBeAppliedTo(membrane.Multiset))
                {
                    usableRules.Add(ruleToCopy.Copy());
                }
            }
            // Use reactives in a maximally parallel way. 
            //  - Maximally: Apply rules randomly until none of them can be used due to lack of needed reactives
            //  - Paralelly: Save products (multisets and membranes) to incorporate them later 
            List<int> ruleIndices = Enumerable.Range(0, usableRules.Count).ToList();
            System.Random rng = new System.Random();
            ruleIndices = ruleIndices.OrderBy(x => rng.Next()).ToList();
            
            foreach (int i in ruleIndices)
            {
                while (usableRules[i].CanBeAppliedTo(membrane.Multiset))
                {
                    // Get specific rule to use
                    PSystemRule usingRule = usableRules[i];
                    // Substract used objects from the current multiset
                    Multiset reactiveMultiset = new(easyMultiset: usingRule.ReactiveMultiset);
                    membrane.UpdateMultiset(reactiveMultiset.GetInverse());

                    // Get the product randomly produced by the rule (through its probabilities)
                    PSystemProduct pSystemProduct = new(easyProduct: usingRule.SelectRandomProduct());
                    Multiset productMultiset = pSystemProduct.ProductMultiset;
                    List<Membrane> productMembranes = pSystemProduct.ProductMembranes;

                    // Add products to the totalProducedMultiset
                    totalProducedMultiset.CombineWith(productMultiset);
                    // Add produced membranes to the list of produced membranes
                    foreach (Membrane productMembrane in productMembranes)
                    {
                        totalProducedMembranes.Add(productMembrane.Copy());
                    }
                }
            }

            // Apply rules to inner membranes
            foreach (Membrane innerMembrane in membrane.InnerMembranes)
            {
                this.EvolveMembrane(membrane: innerMembrane, depth:depth+1);
            }
            // Incorporate the products
            //  - Membranes
            foreach (Membrane incorporatingMembrane in totalProducedMembranes)
            {
                membrane.IncorporateMembrane(incorporatingMembrane);
            }
            //  - Multiset
            membrane.UpdateMultiset(totalProducedMultiset);
        }
        if (depth == 0)
        {
            Debug.Log($"Before PSystem membrane {this.MembraneString}");
            Debug.Log($"Length of the before string {this.MembraneString.Length}");
            this.MembraneString = membrane.ToString();
            Debug.Log($"After PSystem membrane {this.MembraneString}");
            Debug.Log($"Length of the after string {this.MembraneString.Length}");
        }
    }

}
