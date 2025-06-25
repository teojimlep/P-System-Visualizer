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
using UnityEditor;

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
            LogManager.currentLogText = "There are no previous saved rules. Starting from scratch.";
        }
    }
    public void PromptedSaveRules()
    {
        // Obtener las reglas y ángulos que deseas guardar
        List<PSystemRule> collectedRules = ruleCollector.GetRules();
        List<string> collectedParams = ruleCollector.GetParams();

        // Abrir un cuadro de diálogo para seleccionar la ruta del archivo
        // string filePath = EditorUtility.SaveFilePanel("Guardar reglas", "", "rules.json", "json");
        var filePath = StandaloneFileBrowser.SaveFilePanel("Save rules", "", "", "json");

        // Verificar si se ha seleccionado un archivo
        if (!string.IsNullOrEmpty(filePath))
        {
            // Guardar las reglas en el archivo seleccionado
            SaveRules(collectedRules, collectedParams, filePath);
        }
        else
        {
            LogManager.currentLogText = "No se seleccionó ningún archivo para guardar.";
        }
    }
    public static void SaveRules(List<PSystemRule> rulesToSave, List<string> collectedParams, string filePath)
    {
        if (rulesToSave == null)
        {
            LogManager.currentLogText += " Non valid rules.";
            return;
        }
        else
        {
            JSONRules rootRules = new JSONRules
            {
                Rules = new List<JSONRule>(),
                HeadAngle = collectedParams[0],
                LeftAngle = collectedParams[1],
                UpAngle = collectedParams[2],
                Axiom = collectedParams[3]
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
                        Probability = product.Probability.ToString("F3").Replace(',', '.')
                    });
                }

                rootRules.Rules.Add(jsonRule);
            }

            // Convertir a JSON
            string json = JsonUtility.ToJson(rootRules, true);

            // Guardar en archivo
            File.WriteAllText(filePath, json);

            LogManager.currentLogText = "Rules were saved successfully.";
        }
    }
    public void PromptedLoadRules()
    {
        // Abrir un cuadro de diálogo para seleccionar el archivo JSON
        //string filePath = EditorUtility.OpenFilePanel("Cargar reglas", "", "json");
        //string filePath = new OpenFileDialog { Filter = "Archivos JSON (*.json)|*.json" }.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : "";
        string filePath = null;
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Cargar reglas", "", "json", false);
        if (paths != null && paths.Length > 0)
        {
            filePath = paths[0];
            // Proceed with loading file at filePath
        }
        else
        {
            LogManager.currentLogText = "No file has been selected or the operation was cancelled.";
            // Handle cancel or no selection appropriately
        }

        // Verificar si se ha seleccionado un archivo
        if (!string.IsNullOrEmpty(filePath))
        {
            // Cargar y procesar las reglas desde el archivo
            List<PSystemRule> loadedRules = LoadRules(filePath);

            if (loadedRules != null)
            {
                ruleCollector.SetRules(loadedRules);
                LogManager.currentLogText = "Loaded rules successfully.";
            }
            else
            {
                LogManager.currentLogText = "There was an error loading the json file.";
            }
        }
        else
        {
            LogManager.currentLogText = "The json file was not found.";
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
            LogManager.currentLogText = $"The selected file does not exist.";
            return null;
        }

        // Leer el contenido del archivo JSON
        string json = File.ReadAllText(filePath);

        // Deserializar el JSON a la estructura JSONRules
        //Debug.Log($"JSON leído:\n{json}");
        JSONRules rootRules = JsonUtility.FromJson<JSONRules>(json);

        // Write parameters in the UI
        GameObject.Find("AngleHead").GetComponentInChildren<TMP_InputField>().text = rootRules.HeadAngle;
        GameObject.Find("AngleLeft").GetComponentInChildren<TMP_InputField>().text = rootRules.LeftAngle;
        GameObject.Find("AngleUp").GetComponentInChildren<TMP_InputField>().text = rootRules.UpAngle;
        GameObject.Find("Axiom").GetComponentInChildren<TMP_InputField>().text = rootRules.Axiom;

        if (rootRules == null || rootRules.Rules == null)
        {
            LogManager.currentLogText = "The selected file does not contain a set of valid rules. Check syntax.";
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
            // Buscamos el objeto RuleSetter que se corresponde con el ruleIndex actual.
            // Si no existe, lo creamos pulsando el botón "+" del RuleSetter anterior.
            // Almacenamos su transform en ruleSetterTransform.
            Transform ruleSetterTransform = rulesContainer.Find($"RuleSetter_{ruleIndex}");
            if (ruleSetterTransform == null)
            {
                Transform addRuleButton = rulesContainer.Find($"RuleSetter_{ruleIndex-1}/AddNewRule");
                addRuleButton.gameObject.GetComponent<Button>().onClick.Invoke();
                ruleSetter = rulesContainer.Find($"RuleSetter_{ruleIndex}").gameObject;
            }
            else
            {
                ruleSetter = ruleSetterTransform.gameObject;
            }
            ruleSetterTransform = ruleSetter.transform;

            // Buscamos los inputs de texto en los que se sitúan predecesores y etiquetas.
            // Escribimos en estos campos lo que indica el json que estamos leyendo.
            Transform productsTransform = ruleSetterTransform.Find("Products");
            ruleSetter.transform.Find("MulPredecessor").GetComponentInChildren<TMP_InputField>().text = jsonRule.Predecessor;
            ruleSetter.transform.Find("TargetLabels").GetComponentInChildren<TMP_InputField>().text = jsonRule.TargetLabels;

            // Creamos la regla con el predecessor y las etiquetas que indica el json.
            // Inicializamos con una lista vacía de productos vacía.
            PSystemRule rule = new PSystemRule
            (
                reactiveMultiset: jsonRule.Predecessor,
                membraneLabels: new List<char>(jsonRule.TargetLabels.ToCharArray()),
                possibleProducts: new List<(float,string)>()
            );


            // Recorremos los productos indicados en el json.
            // Comprobamos si el producto existe o no en la interfaz, y lo creamos si es necesario.
            int productIndex = 1;
            foreach (var product in jsonRule.Products)
            {
                Transform currentProductTransform = productsTransform.Find($"Product_{productIndex}");
                if (currentProductTransform == null)
                {
                    Transform addProductButton = productsTransform.Find($"Product_{productIndex-1}/AddNewProduct");
                    addProductButton.gameObject.GetComponent<Button>().onClick.Invoke();
                    currentProduct = productsTransform.Find($"Product_{productIndex}").gameObject;
                }
                else
                {
                    currentProduct = currentProductTransform.gameObject;
                }
                currentProductTransform = currentProduct.transform;

                // Buscamos los campos sucesor y probabilidad y asignamos los valores que indica el json.
                currentProductTransform.Find("MulSuccessor").GetComponentInChildren<TMP_InputField>().text = product.Successor;
                currentProductTransform.Find("Probability").GetComponentInChildren<TMP_InputField>().text = product.Probability.Replace(',', '.');
                
                // Add the current porduct and probability to the rule products
                rule.PossibleProducts.Add((float.Parse(product.Probability), product.Successor));

                productIndex++;
            }
            loadedRules.Add(rule);
            ruleIndex++;
        }

        return loadedRules;
    }
}

[Serializable]
public class JSONPossibleProduct
{
    public string Successor;
    public string Probability;
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
    public string Axiom;
    public string HeadAngle;
    public string LeftAngle;
    public string UpAngle;
    public List<JSONRule> Rules;
}
