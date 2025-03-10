using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public RuleCollector ruleCollector;
    public void LoadScene(string sceneName)
    {
        if (sceneName == "TreeScene")
        {
            List<PSystemRule> collectedRules = ruleCollector.GetRules();
            RuleCollector.SelectedRules = collectedRules;
            //RuleManager.Instance.SetRules(collectedRules); // Send rules to the singleton
            //Debug.Log("Rules sent to RuleManager");
            JSONWriter.SaveRules(RuleCollector.SelectedRules,"Assets/rules.json");
        }
        
        SceneManager.LoadScene(sceneName);
    }

}

