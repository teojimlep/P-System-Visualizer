using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RuleSetter : MonoBehaviour
{
    public float verticalPadding = 10f;
    public GameObject RuleSetterPrefab;
    public GameObject RulesObject = null;

    public void Awake()
    {
        Update();
    }
    public void Update()
    {
        if (this.name == "Rules")
        {
            AdjustRuleSetterPositions(this.RulesObject);
        }
    }
    public void AddNewRule()
    {
        // Contar cuántos RuleSetter existen
        Transform rulesContainer = GameObject.Find("Rules").transform;
        int ruleIndex = rulesContainer.childCount;
        string newRuleName = $"RuleSetter_{ruleIndex+1}";

        // Calcular la nueva posición
        Vector3 newPosition = Vector3.zero;
        if (ruleIndex > 0)
        {
            Transform lastRule = rulesContainer.GetChild(ruleIndex - 1);
            Transform panel = lastRule.Find("Panel");

            if (panel != null)
            {
                float panelHeight = panel.GetComponent<RectTransform>().rect.height;
                newPosition = lastRule.position - new Vector3(0, panelHeight + verticalPadding, 0);
            }
        }

        // Instanciar el nuevo RuleSetter
        GameObject newRule = Instantiate(RuleSetterPrefab, newPosition, Quaternion.identity, rulesContainer);
        newRule.name = newRuleName;

        // Disable the AddNewRule button
        ToggleChildByPath($"RuleSetter_{ruleIndex}/AddNewRule");

        // Get the $"RuleSetter_{ruleIndex+1}/AddNewRule" object. Point to the AddNewRule method in the object Rules (to set up the button)
        Transform newAddButton = newRule.transform.Find("AddNewRule");
        if (newAddButton != null)
        {
            // Encontrar el objeto Rules en la escena
            GameObject rulesObject = GameObject.Find("Rules");
            if (rulesObject != null)
            {
                // Obtener el componente que contiene el método AddNewRule (en este caso, el script RuleSetter)
                RuleSetter ruleSetterScript = rulesObject.GetComponent<RuleSetter>();
                if (ruleSetterScript != null)
                {
                    // Eliminar todos los listeners previos
                    newAddButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();

                    // Asignar el método AddNewRule del objeto Rules al evento del botón
                    newAddButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ruleSetterScript.AddNewRule);
                }
                else
                {
                    Debug.LogWarning("No se encontró el componente RuleSetter en el objeto Rules.");
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el objeto 'Rules' en la escena.");
            }
        }
        // Ajustar el tamaño del contenedor Rules
        //AdjustRuleSetterPositions(this.RulesObject);
    }

    private void AdjustRuleSetterPositions(GameObject rulesObject)
    {
        // Take every child of RulesObject (They are RuleSetter_i) (do not take grand children, only immediate)
        // Position RuleSetter_1 at the top of the Rules container
        // Postition RuleSetter_2 just below (plus padding) - To do this, take into account the height of the panel object in RuleSetter_i-1
        // Go on for all children
        Transform rulesTransform = rulesObject.transform;
        float currentY = 257f;  // Start positioning from the top
        foreach (Transform ruleSetter in rulesTransform)
        {
            if (ruleSetter.parent == rulesTransform && ruleSetter.name!="DeletingRuleSetter")
            {
                RectTransform panel = ruleSetter.Find("Panel")?.GetComponent<RectTransform>();

                if (panel != null)
                {
                    // Move RuleSetter to the correct position
                    ruleSetter.localPosition = new Vector3(ruleSetter.localPosition.x, -currentY, 0);

                    // Update Y position for the next RuleSetter
                    currentY += panel.rect.height + verticalPadding;
                }
            }
        }
        // Adjust the container height to fit all rule setters
        RectTransform containerRect = rulesObject.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, currentY);
        }
    }
    public void Delete()
    {
        // Get the index of the deleted object
        string[] nameParts = gameObject.name.Split('_');
        int deletedIndex = int.Parse(nameParts[^1]);
        // Get all sibling objects with the same naming convention
        Transform parentTransform = transform.parent;
        var allObjects = parentTransform.GetComponentsInChildren<Transform>(false);
        // Get the number of rule setters
        int nRuleSetters = 0;
        foreach (var obj in allObjects)
        {
            if (obj.parent == parentTransform)
            {
                nRuleSetters++;
            }
        }
        if (nRuleSetters != 1)
        {
            // Iterate through all objects and decrement names of those with higher numbers
            foreach (var obj in allObjects)
            {
                if (obj.parent == parentTransform)
                {
                    string[] objNameParts = obj.name.Split('_');
                    int currentIndex = int.Parse(objNameParts[^1]);
                    if (currentIndex > deletedIndex)
                    {
                        obj.name = $"RuleSetter_{currentIndex - 1}";
                    }
                }
            }
            gameObject.name = "DeletingRuleSetter";
            // Ajustar el tamaño del contenedor Rules
            //AdjustRuleSetterPositions(parentTransform.gameObject);
            // Destroy the object
            Destroy(gameObject);
            if (nRuleSetters == deletedIndex)
            {
                Debug.Log($"Entrado en el toggle: RuleSetter_{nRuleSetters - 1}/AddNewRule");
                //ToggleChildByPath($"./RuleSetter_{nRuleSetters-1}/AddNewRule");
                string rootName = transform.parent.name;
                string path = $"{rootName}/RuleSetter_{nRuleSetters - 1}/AddNewRule";
                Debug.Log(path);
                ToggleChildByPath(path, absPath: true);
            }
        }
    }
    public void ToggleChildByPath(string path, bool absPath = false)
    {
        Transform child;
        if (absPath)
        {
            // Absolute path: start from the root GameObject
            GameObject root = GameObject.Find(path.Split('/')[0]);
            if (root == null)
            {
                Debug.LogWarning($"Root object '{path.Split('/')[0]}' not found.");
                return;
            }

            string subPath = path.Contains("/") ? path.Substring(path.IndexOf('/') + 1) : "";
            child = string.IsNullOrEmpty(subPath) ? root.transform : root.transform.Find(subPath);
        }
        else
        {
            child = transform.Find(path);
        }
        Debug.Log($"Toggling game transform {child}");
        if (child != null)
        {
            bool currentState = child.gameObject.activeSelf;
            Debug.Log($"Toggling game object {child.gameObject}");
            child.gameObject.SetActive(!currentState); // Toggle the state
        }
        else
        {
            Debug.LogWarning($"Child with path '{path}' not found.");
        }
    }

    public void DeleteChildByPath(string path)
    {
        Transform child = transform.Find(path);
        if (child != null)
        {
            Destroy(child.gameObject);
        }
        else
        {
            Debug.LogWarning($"Child with path '{path}' not found.");
        }
    }
}
