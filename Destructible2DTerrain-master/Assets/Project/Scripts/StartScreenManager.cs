using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("DestructibleSandbox"); // Name deiner Hauptspiel-Scene
    }
}