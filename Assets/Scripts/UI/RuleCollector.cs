using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine.SceneManagement;

public class RuleCollector : MonoBehaviour
{
    public List<PSystemRule> collectedRules = new(); // List to store collected rules
    public List<string> collectedParams = new();
    public Transform parentTransform; // Assign the 'Rules' GameObject in the Inspector
    public static List<PSystemRule> SelectedRules = null;
    public static List<string> SelectedParams = null;

    public List<string> GetParams()
    {
        collectedParams.Clear();
        collectedParams.Add(GameObject.Find("AngleHead").GetComponentInChildren<TMP_InputField>().text);
        collectedParams.Add(GameObject.Find("AngleLeft").GetComponentInChildren<TMP_InputField>().text);
        collectedParams.Add(GameObject.Find("AngleUp").GetComponentInChildren<TMP_InputField>().text);
        collectedParams.Add(GameObject.Find("Axiom").GetComponentInChildren<TMP_InputField>().text);

        return collectedParams;
    }
    public List<PSystemRule> GetRules()
    {
        LogManager.currentLogText = "";
        collectedRules.Clear();

        foreach (Transform rule in parentTransform) // Loop through each RuleSetter_#
        {
            // Access the TMP_InputField directly under MulPredecessor and MulSucessor
            TMP_InputField predecessorInput = rule.Find("MulPredecessor").GetComponentInChildren<TMP_InputField>();
            TMP_InputField labelsInput = rule.Find("TargetLabels").GetComponentInChildren<TMP_InputField>();

            if (string.IsNullOrEmpty(predecessorInput.text))
            {
                LogManager.currentLogText = "Every rule must be tied to a valid predecessor.";
                return null;
            }
            else
            {
                // Store target membrane labels in a list
                List<char> targetLabels = null;
                try
                {
                    targetLabels = new List<char>(labelsInput.text.ToList());
                }
                catch
                {
                    LogManager.currentLogText = "Labels must be comma separated single characters.";
                    return null;
                }

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
                        if (probInput != null && successorInput != null)
                        {
                            float prob;
                            try
                            {
                                prob = float.Parse(probInput.text, CultureInfo.InvariantCulture.NumberFormat);
                            }
                            catch
                            {
                                LogManager.currentLogText = "Probabilities must be numbers.";
                                return null;
                            }
                            ruleProducts.Add((prob, successorInput.text));
                        }
                        else
                        {
                            LogManager.currentLogText = "Probabilities and products must be indicated.";
                            return null;
                        }
                    }
                }

                try
                {
                    PSystemRule newRule = new(reactiveMultiset: predecessorInput.text, ruleProducts, targetLabels);
                    collectedRules.Add(newRule);
                }
                catch
                {
                    LogManager.currentLogText = "Predecessors, prodcuts and labels must be in the correct format.";
                    return null;
                }
            }
        }
        LogManager.currentLogText = "Rules are in the correct format.";
        return collectedRules;
    }

    public void SetRules(List<PSystemRule> newRules)
    {
        this.collectedRules = newRules;
    }
}
