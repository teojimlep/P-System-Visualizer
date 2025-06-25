using UnityEngine;

public class AppControl : MonoBehaviour
{
    public void CloseApp()
    {
        Application.Quit();

        // For editor testing only
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}