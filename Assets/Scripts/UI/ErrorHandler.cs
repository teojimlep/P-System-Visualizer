using UnityEngine;
using TMPro;

public class LogManager : MonoBehaviour
{
    public TextMeshProUGUI logText;
    public static string currentLogText = "";

    public void Update()
    {
        logText.text = LogManager.currentLogText;
    }
}
