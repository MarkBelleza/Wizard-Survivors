using UnityEngine;

public class GameOptionsController : MonoBehaviour
{
    public void Quit()
    {

        //Close the application. If we're in the editor, stop playing instead.
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
