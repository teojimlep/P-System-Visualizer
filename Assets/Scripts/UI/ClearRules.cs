using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleCleaner : MonoBehaviour
{
    // Start is called before the first frame update
    public static void ClearRules()
    {
        GameObject rulesObject = GameObject.Find("Rules");
        GameObject ruleSetterPrefab = Resources.Load<GameObject>("Prefabs/RuleSetter");

        Transform rulesTransform = rulesObject.transform;
        Vector3 ruleSetter1Position = new();
        foreach (Transform ruleSetter in rulesTransform)
        {
            if (ruleSetter.name=="RuleSetter_1")
            {
                ruleSetter1Position = ruleSetter.position;
                //newPosition -> posicion de ruleSetter
            }
            Destroy(ruleSetter.gameObject);
        }
        GameObject newRule = Instantiate(ruleSetterPrefab, ruleSetter1Position, Quaternion.identity, rulesTransform);
        newRule.name = "RuleSetter_1";

        // Get the $"RuleSetter_{ruleIndex+1}/AddNewRule" object. Point to the AddNewRule method in the object Rules (to set up the button)
        Transform newAddButton = newRule.transform.Find("AddNewRule");
        if (newAddButton != null)
        {
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
            }
        }
    }
}
