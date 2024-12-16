
using System.Collections.Generic;
using UnityEngine;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance; // Singleton instance
    public List<PSystemRule> collectedRules = new(); // List to store rules

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    public void SetRules(List<PSystemRule> rules)
    {
        collectedRules = new List<PSystemRule>(rules);
    }

    public List<PSystemRule> GetRules()
    {
        return collectedRules;
    }
}
