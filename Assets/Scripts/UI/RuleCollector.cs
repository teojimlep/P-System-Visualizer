using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine.SceneManagement;

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
            TMP_InputField labelsInput = rule.Find("TargetLabels").GetComponentInChildren<TMP_InputField>();

            if (predecessorInput != null)
            {
                
                Debug.Log(predecessorInput.text);
                Debug.Log(labelsInput.text);

                // Store target membrane labels in a list
                List<char> targetLabels = new List<char>(labelsInput.text.ToList());

                // Create a new PSystemRule and add it to the list
                List<(float Probability, string Product)> ruleProducts = new();
                // Find the Products child of the rule object
                Transform productsTransform = rule.Find("Products");
                foreach (Transform product in productsTransform)
                {
                    if (product.name != "PanelProducts")
                    {
                        TMP_InputField successorInput = product.Find("MulSuccessor").GetComponentInChildren<TMP_InputField>();
                        TMP_InputField probInput = product.Find("Probability").GetComponentInChildren<TMP_InputField>();
                        Debug.Log(successorInput.text);
                        if (probInput != null && successorInput != null)
                        {
                            Debug.Log(probInput.text);
                            ruleProducts.Add((float.Parse(probInput.text, CultureInfo.InvariantCulture.NumberFormat), successorInput.text));
                        }
                    }
                }

                PSystemRule newRule = new(reactiveMultiset: predecessorInput.text, ruleProducts, targetLabels);
                Debug.Log(newRule.ToString());
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

    public void SetRules(List<PSystemRule> newRules)
    {
        this.collectedRules = newRules;
    }
}
