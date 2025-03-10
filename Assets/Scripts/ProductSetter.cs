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


    private void Start()
    {
        productsParent = transform.parent;
        panelProducts = productsParent.Find("PanelProducts")?.GetComponent<RectTransform>();
        Transform rulesSetterParent = transform.parent.parent;
        panelRules = rulesSetterParent.Find("Panel")?.GetComponent<RectTransform>();
        // Ensure that the pivot and anchors are set to the top
        float initialHeight = panelProducts.rect.height;
        Vector3 initialPosition = panelProducts.localPosition;
        //panelProducts.pivot = new Vector2(0.5f, 1f); // Anchor the pivot at the top
        //panelProducts.anchorMin = new Vector2(0f, 1f); // Set anchorMin to top
        //panelProducts.anchorMax = new Vector2(1f, 1f); // Set anchorMax to top
        // Increase the height of the panel
        //Vector2 currentSize = panelProducts.sizeDelta;
        //currentSize.y = initialHeight; // Add the additional height
        //panelProducts.sizeDelta = currentSize; // Apply the new size
        //panelProducts.localPosition = initialPosition;
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
        DeleteChildByPath($"AddNewProduct");

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
        foreach (var obj in allObjects)
        {
            if (obj.parent == parentTransform && obj.name != "PanelProducts")
            {
                string[] objNameParts = obj.name.Split('_');
                int currentIndex = int.Parse(objNameParts[^1]);
                if (currentIndex > deletedIndex)
                {
                    Vector3 targetPos = savedPositions[$"Product_{currentIndex-1}"];
                    obj.name = $"Product_{currentIndex - 1}";
                    obj.transform.position = targetPos;
                }
            }
        }

        Destroy(gameObject);

        float shift = Mathf.Abs(GetComponent<RectTransform>().rect.height + verticalPadding);
        ExpandPanelDownwards(panelProducts,-shift);
        ExpandPanelDownwards(panelRules,-shift);
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
        Debug.Log(shift);
        
        // Increase the height of the panel
        Vector2 currentSize = panel.sizeDelta;
        currentSize.y += shift; // Add the additional height
        panel.sizeDelta = currentSize; // Apply the new size
    }
}
