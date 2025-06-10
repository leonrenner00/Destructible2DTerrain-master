using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // sicherstellen, dass Zeit normal läuft beim Neustart
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}