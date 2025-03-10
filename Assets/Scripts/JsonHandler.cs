using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor; 

public class JSONWriter : MonoBehaviour
{
    public RuleCollector ruleCollector;

    public void PromptedSaveRules()
    {
        // Obtener las reglas que deseas guardar
        List<PSystemRule> collectedRules = ruleCollector.GetRules();

        // Abrir un cuadro de diálogo para seleccionar la ruta del archivo
        string filePath = EditorUtility.SaveFilePanel("Guardar reglas", "", "rules.json", "json");

        // Verificar si se ha seleccionado un archivo
        if (!string.IsNullOrEmpty(filePath))
        {
            // Guardar las reglas en el archivo seleccionado
            SaveRules(collectedRules, filePath);
        }
        else
        {
            Debug.Log("No se seleccionó ningún archivo para guardar.");
        }
    }
    public static void SaveRules(List<PSystemRule> rulesToSave, string filePath)
    {
        JSONRules rootRules = new JSONRules
        {
            Rules = new List<JSONRule>()
        };

        // Convertir PSystemRule a JSONRule
        foreach (PSystemRule rule in rulesToSave)
        {
            JSONRule jsonRule = new JSONRule
            {
                Predecessor = rule.ReactiveMultiset,
                TargetLabels = new string(rule.MembraneLabels.ToArray()),
                Products = new List<JSONPossibleProduct>()
            };

            foreach (var product in rule.PossibleProducts)
            {
                jsonRule.Products.Add(new JSONPossibleProduct
                {
                    Successor = product.Product,
                    Probability = product.Probability
                });
            }

            rootRules.Rules.Add(jsonRule);
        }

        // Convertir a JSON
        string json = JsonUtility.ToJson(rootRules, true);

        // Guardar en archivo
        File.WriteAllText(filePath, json);

        Debug.Log($"Reglas guardadas en: {filePath}");
    }
}


[Serializable]
public class JSONPossibleProduct
{
    public string Successor;
    public float Probability;
}

[Serializable]
public class JSONRule
{
    public string Predecessor;
    public string TargetLabels;
    public List<JSONPossibleProduct> Products;
}

[Serializable]
public class JSONRules
{
    public List<JSONRule> Rules;
}
