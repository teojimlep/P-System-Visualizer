using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using System.Windows.Forms;
using TMPro;
using SFB;

public class JSONHandler : MonoBehaviour
{
    public RuleCollector ruleCollector;

    public void Start()
    {
        string rootPath = Directory.GetParent(Application.dataPath).FullName;
        string lastRulesPath = Path.Combine(rootPath, "LastRules/last_rules.json");
        if (File.Exists(lastRulesPath))
        {
            LoadRules(lastRulesPath);
        }
        else
        {
            Debug.Log("El archivo no existe");
        }
    }
    public void PromptedSaveRules()
    {
        // Obtener las reglas que deseas guardar
        List<PSystemRule> collectedRules = ruleCollector.GetRules();

        // Abrir un cuadro de diálogo para seleccionar la ruta del archivo
        // string filePath = EditorUtility.SaveFilePanel("Guardar reglas", "", "rules.json", "json");
        var filePath = StandaloneFileBrowser.SaveFilePanel("Guardar reglas", "", "", "json");

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
    public void PromptedLoadRules()
    {
        // Abrir un cuadro de diálogo para seleccionar el archivo JSON
        //string filePath = EditorUtility.OpenFilePanel("Cargar reglas", "", "json");
        //string filePath = new OpenFileDialog { Filter = "Archivos JSON (*.json)|*.json" }.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : "";
        string filePath = StandaloneFileBrowser.OpenFilePanel("Cargar reglas", "", "json", false)[0];

        // Verificar si se ha seleccionado un archivo
        if (!string.IsNullOrEmpty(filePath))
        {
            // Cargar y procesar las reglas desde el archivo
            List<PSystemRule> loadedRules = LoadRules(filePath);

            if (loadedRules != null)
            {
                ruleCollector.SetRules(loadedRules);
                Debug.Log("Reglas cargadas correctamente.");
            }
            else
            {
                Debug.LogError("Error al cargar el archivo JSON.");
            }
        }
        else
        {
            Debug.Log("No se seleccionó ningún archivo.");
        }
    }

    public List<PSystemRule> LoadRules(string filePath)
    {
        // Key objects to load the data into the UI
        // Get the parent object trasform
        Transform canvasTransform = transform.parent;
        
        // Verificar si el archivo existe
        if (!File.Exists(filePath))
        {
            Debug.LogError($"El archivo {filePath} no existe.");
            return null;
        }

        // Leer el contenido del archivo JSON
        string json = File.ReadAllText(filePath);

        // Deserializar el JSON a la estructura JSONRules
        Debug.Log($"JSON leído:\n{json}");
        JSONRules rootRules = JsonUtility.FromJson<JSONRules>(json);

        if (rootRules == null || rootRules.Rules == null)
        {
            Debug.LogError("El archivo JSON no contiene reglas válidas.");
            return null;
        }

        List<PSystemRule> loadedRules = new List<PSystemRule>();

        // Subir niveles hasta llegar a "Rules"
        Transform rulesContainer = canvasTransform.Find("Scrollbar/ScrollArea/View/Rules");

        GameObject ruleSetter;
        GameObject currentProduct;
        int ruleIndex = 1;
        // Convertir JSONRule a PSystemRule
        foreach (JSONRule jsonRule in rootRules.Rules)
        {
            Transform ruleSetterTransform = rulesContainer.Find($"RuleSetter_{ruleIndex}");
            if (ruleSetterTransform == null)
            {
                Debug.Log($"RuleSetter_{ruleIndex} no existe. ¿Pulsando botón?");
                Transform addRuleButton = rulesContainer.Find($"RuleSetter_{ruleIndex-1}/AddNewRule");
                addRuleButton.gameObject.GetComponent<Button>().onClick.Invoke();
                ruleSetter = rulesContainer.Find($"RuleSetter_{ruleIndex}").gameObject;
            }
            else
            {
                ruleSetter = ruleSetterTransform.gameObject;
                Debug.Log($"{ruleSetter.name} existe.");
            }
            ruleSetterTransform = ruleSetter.transform;
            Transform productsTransform = ruleSetterTransform.Find("Products");

            ruleSetter.transform.Find("MulPredecessor").GetComponentInChildren<TMP_InputField>().text = jsonRule.Predecessor;
            ruleSetter.transform.Find("TargetLabels").GetComponentInChildren<TMP_InputField>().text = jsonRule.TargetLabels;

            PSystemRule rule = new PSystemRule
            (
                reactiveMultiset: jsonRule.Predecessor,
                membraneLabels: new List<char>(jsonRule.TargetLabels.ToCharArray()),
                possibleProducts: new List<(float,string)>()
            );

            int productIndex = 1;
            foreach (var product in jsonRule.Products)
            {
                Transform currentProductTransform = productsTransform.Find($"Product_{productIndex}");
                if (currentProductTransform == null)
                {
                    Debug.Log($"Product_{productIndex} no existe. ¿Pulsando botón?");
                    Transform addProductButton = productsTransform.Find($"Product_{productIndex-1}/AddNewProduct");
                    addProductButton.gameObject.GetComponent<Button>().onClick.Invoke();
                    currentProduct = productsTransform.Find($"Product_{productIndex}").gameObject;
                }
                else
                {
                    currentProduct = currentProductTransform.gameObject;
                    Debug.Log($"{currentProduct.name} existe.");
                }
                currentProductTransform = currentProduct.transform;

                currentProductTransform.Find("MulSuccessor").GetComponentInChildren<TMP_InputField>().text = product.Successor;
                currentProductTransform.Find("Probability").GetComponentInChildren<TMP_InputField>().text = product.Probability.ToString("F3").Replace(',', '.');;
                
                // Add the current porduct and probability to the rule products
                rule.PossibleProducts.Add((product.Probability, product.Successor));

                productIndex++;
            }
            loadedRules.Add(rule);
            ruleIndex++;
        }

        Debug.Log(loadedRules.ToString());
        return loadedRules;
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