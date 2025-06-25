using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    public RuleCollector ruleCollector;
    public void LoadScene(string sceneName)
    {
        if (sceneName == "TreeScene")
        {
            RuleCollector.SelectedRules = ruleCollector.GetRules();
            RuleCollector.SelectedParams = ruleCollector.GetParams();
            string rootPath = Directory.GetParent(Application.dataPath).FullName;
            string lastRulesPath = Path.Combine(rootPath, "LastRules/last_rules.json");
            JSONHandler.SaveRules(RuleCollector.SelectedRules, RuleCollector.SelectedParams, lastRulesPath);
            if (LogManager.currentLogText == "Rules were saved successfully.")
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                return;
            }
        }
        SceneManager.LoadScene(sceneName);
    }

    public static void ReloadScene()
    {
        string rootPath = Directory.GetParent(Application.dataPath).FullName;
        string lastRulesPath = Path.Combine(rootPath, "LastRules/last_rules.json");
        FileDeleter.DeleteFile(lastRulesPath);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}

public static class FileDeleter
{
    public static void DeleteFile(string relativePath)
    {
        string fullPath = Path.Combine(Application.dataPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            LogManager.currentLogText = "Eliminadas las reglas guardadas en memoria.";
        }
        else
        {
            LogManager.currentLogText = "Eliminadas las reglas guardadas en memoria.";
        }
    }
}

