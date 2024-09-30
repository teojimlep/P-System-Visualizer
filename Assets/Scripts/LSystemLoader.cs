using UnityEngine;
using System;
using System.Collections.Generic;

public class LSystemLoader
{
    [Serializable]
    public class LSystemsData
    {
        public List<LSystem> LSystems;
    }

    public LSystem LoadLSystem(string name, string jsonName, bool random_choice)
    {
        // Load the text asset containing JSON data
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonName);
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load JSON file: " + jsonName);
            return null;
        }

        // Deserialize JSON string to LSystemsData object
        LSystemsData LSystemsData = JsonUtility.FromJson<LSystemsData>(jsonFile.text);
        if (LSystemsData == null)
        {
            Debug.LogError("Failed to deserialize JSON data: " + jsonName);
            return null;
        }

        int totalLSystems = LSystemsData.LSystems.Count;

        if (random_choice)
        {
            int randomIndex = UnityEngine.Random.Range(0, totalLSystems);
            Debug.Log("Randomly selected index: " + randomIndex);
            return LSystemsData.LSystems[randomIndex];
        }

        // Find the L-system by name
        LSystem selectedLSystem = LSystemsData.LSystems.Find(lsys => lsys.Name == name);
        if (selectedLSystem != null)
        {
            return selectedLSystem;
        }
        else
        {
            // L-system with specified name not found
            Debug.LogError("L-system with name '" + name + "' not found.");
            return null;
        }
    }
}
