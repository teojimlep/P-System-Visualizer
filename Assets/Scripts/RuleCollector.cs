using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RuleCollector : MonoBehaviour
{
    public List<PSystemRule> collectedRules = new(); // List to store collected rules
    public Transform parentTransform; // Assign the 'Rules' GameObject in the Inspector
    public static List<PSystemRule> SelectedRules = null;
    public List<PSystemRule> GetRules()
    {
        collectedRules.Clear();

        foreach (Transform rule in parentTransform) // Loop through each RuleSetter_#
        {
            // Access the TMP_InputField directly under MulPredecessor and MulSucessor
            TMP_InputField predecessorInput = rule.Find("MulPredecessor").GetComponentInChildren<TMP_InputField>();
            TMP_InputField successorInput = rule.Find("MulSuccessor").GetComponentInChildren<TMP_InputField>();
            TMP_InputField labelsInput = rule.Find("TargetLabels").GetComponentInChildren<TMP_InputField>();

            if (predecessorInput != null && successorInput != null)
            {
                Debug.Log(successorInput.text);
                Debug.Log(predecessorInput.text);
                Debug.Log(labelsInput.text);

                // Create a new PSystemRule and add it to the list
                List<(float Probability, string Product)> newProduct = new()
                {
                    (1f, successorInput.text)
                };

                // Store target membrane labels in a list
                List<char> targetLabels = new List<char>(labelsInput.text.ToList());

                PSystemRule newRule = new(reactiveMultiset: predecessorInput.text, newProduct, targetLabels);
                collectedRules.Add(newRule);
            }
            else
            {
                Debug.LogWarning($"Missing TMP_InputField in rule: {rule.name}");
            }
        }

        Debug.Log($"Collected {collectedRules.Count} rules.");
        return collectedRules;
    }
}
