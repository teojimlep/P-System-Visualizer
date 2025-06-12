using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProductSetter : MonoBehaviour
{
    // Este script se asignará a objetos llamados Product_i, cuyos padres son "Products".
    // Otro hijo de "Products" es "PanelProducts".
    public float verticalPadding = 10f;

    private Transform productsParent;  // Padre de los Product_i
    private RectTransform panelProducts;  // Panel que contiene los productos
    private RectTransform panelRules;  // Panel que contiene las reglas


    private void Awake()
    {
        productsParent = transform.parent;
        Debug.Log($"I am {transform.gameObject.name}, son of {transform.parent.gameObject.name}, son of {transform.parent.parent.gameObject.name}");
        panelProducts = productsParent.Find("PanelProducts")?.GetComponent<RectTransform>();
        Transform rulesSetterParent = transform.parent.parent;
        panelRules = rulesSetterParent.Find("Panel")?.GetComponent<RectTransform>();
    }

    public void Replicate()
    {
        // Obtener el índice del producto actual
        int index = int.Parse(gameObject.name.Split('_')[1]);

        // Crear el nuevo nombre incrementando el índice
        string newProductName = $"Product_{index + 1}";

        // Calcular la nueva posición
        Vector3 newPosition = transform.localPosition - new Vector3(0, GetComponent<RectTransform>().rect.height + verticalPadding, 0);

        // Crear una copia del producto actual
        GameObject newProduct = Instantiate(gameObject, productsParent);
        newProduct.name = newProductName;
        newProduct.transform.localPosition = newPosition;

        // Delete replicate button
        ToggleChildByPath($"AddNewProduct");

        float shift = Mathf.Abs(GetComponent<RectTransform>().rect.height + verticalPadding);
        ExpandPanelDownwards(panelProducts, shift);
        ExpandPanelDownwards(panelRules, shift);
    }

    public void Delete()
    {
        
        string[] nameParts = gameObject.name.Split('_');
        int deletedIndex = int.Parse(nameParts[^1]);
        Transform parentTransform = transform.parent;
        var allObjects = parentTransform.GetComponentsInChildren<Transform>(false);
        Dictionary<string, Vector3> savedPositions = new Dictionary<string, Vector3>();
        foreach (var obj in allObjects)
        {
            if (obj.parent == parentTransform && obj.name != "PanelProducts")
            {
                savedPositions.Add(obj.name, obj.transform.position);
            }
        }
        if (savedPositions.Count > 1)
        {
            foreach (var obj in allObjects)
            {
                if (obj.parent == parentTransform && obj.name != "PanelProducts")
                {
                    string[] objNameParts = obj.name.Split('_');
                    int currentIndex = int.Parse(objNameParts[^1]);
                    if (currentIndex > deletedIndex)
                    {
                        Vector3 targetPos = savedPositions[$"Product_{currentIndex - 1}"];
                        obj.name = $"Product_{currentIndex - 1}";
                        obj.transform.position = targetPos;
                    }
                }
            }

            Destroy(gameObject);

            float shift = Mathf.Abs(GetComponent<RectTransform>().rect.height + verticalPadding);
            ExpandPanelDownwards(panelProducts,-shift);
            ExpandPanelDownwards(panelRules,-shift);
        
            if (deletedIndex == savedPositions.Count)
            {
                string absPathofParent = GetFullPath(transform.parent.parent);
                Debug.Log(absPathofParent);
                string path = $"{absPathofParent}/Products/Product_{deletedIndex - 1}/AddNewProduct";
                ToggleChildByPath(path, absPath: true);
            }
        }
    }
    string GetFullPath(Transform t)
    {
        if (t.parent == null)
            return t.name;
        return GetFullPath(t.parent) + "/" + t.name;
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

    public void ExpandPanelDownwards(RectTransform panel,float shift)
    {
        
        Debug.Log($" Trying: I am {transform.gameObject.name}, son of {transform.parent.gameObject.name}, son of {transform.parent.parent.gameObject.name}");
        Debug.Log(shift);
        // Increase the height of the panel
        Vector2 currentSize = panel.sizeDelta;
        currentSize.y += shift; // Add the additional height
        panel.sizeDelta = currentSize; // Apply the new size
        
        Debug.Log($" Successed: I am {transform.gameObject.name}, son of {transform.parent.gameObject.name}, son of {transform.parent.parent.gameObject.name}");
    }
}
